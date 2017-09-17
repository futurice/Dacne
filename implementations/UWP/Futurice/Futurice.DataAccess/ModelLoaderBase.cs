using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public abstract class ModelLoaderBase
    {
        private readonly ICache _cache;

        public ModelLoaderBase(ICache defaultCache = null)
        {
            _cache = defaultCache;
        }

        protected abstract void LoadImplementation(ModelIdentifierBase id, IObserver<IOperationState<IBuffer>> target, CancellationToken ct = default);

        protected abstract void ParseImplementation(ModelIdentifierBase id, IBuffer data, IObserver<IOperationState<object>> target, CancellationToken ct = default);

        protected virtual ICache GetCache(ModelIdentifierBase id)
        {
            return _cache;
        }

        private void LoadData(ModelIdentifierBase id, ModelSource source, Subject<IOperationState<IBuffer>> target, CancellationToken ct = default)
        {
            ICache cache = GetCache(id);

            switch (source)
            {
                case ModelSource.Server:
                    target.OnResult(it => cache?.Save(id, it));
                    LoadImplementation(id, target, ct);
                    break;

                case ModelSource.Disk:
                    if (cache == null)
                    {
                        target.OnNextError(new OperationError(message: "Disk cache not set!"), 0, ModelSource.Disk);
                        target.OnError(new InvalidOperationException("Disk cache not set!"));
                        //target.OnCompleted();
                    }
                    else
                    {
                        cache.Load(id, target, ct);
                    }
                    break;

                default:
                    throw new Exception("Unknown source: " + source);
            }
        }

        public IObservable<IOperationState<object>> Load(ModelIdentifierBase id, ModelSource source, double loadOperationProgressShare = 0.8, CancellationToken ct = default)
        {
            var loadOperationStates = new Subject<IOperationState<IBuffer>>();
            var parseOperationStates = new Subject<IOperationState<object>>();
            
            // Loading and parsing might be running in parallel so we need some logic to combine the state updates
            var combinedReplayStates = new ReplaySubject<IOperationState<object>>();
            Observable.CombineLatest(
                    loadOperationStates
                        .OnResult(buffer => ParseImplementation(id, buffer, parseOperationStates, ct))
                        .StartWith(null as IOperationState<IBuffer>),

                    parseOperationStates
                        .StartWith(null as IOperationState<object>),
                   
                    (load, parse) => Tuple.Create(load, parse)
                )
                .Buffer(2,1)
                .Where(pairs => pairs.Count == 2) // Last buffer
                .Select(pairs =>
                {
                    var olds = pairs[0];
                    var oldLoad = olds?.Item1;
                    var oldParse = olds?.Item2;

                    var news = pairs[1];
                    var newLoad = news.Item1;
                    var newParse = news.Item2;

                    var loadError = newLoad.Error ?? oldLoad?.Error;
                    var parseError = newParse?.Error ?? oldParse?.Error;
                    var latestError = oldLoad != newLoad 
                                        ? loadError ?? parseError 
                                        : parseError ?? loadError;

                    return new OperationState<object>(
                        newParse?.Result,
                        newParse?.Progress == 100
                            ? 100
                            : newLoad.Progress * loadOperationProgressShare + ((newParse?.Progress ?? 0) * (1.0 - loadOperationProgressShare)),
                        latestError,
                        false, // TODO: Cancelled?
                        source,
                        newParse?.ResultIdentifier,
                        newParse?.ResultProgress ?? 0);
                })
                .Subscribe(combinedReplayStates);
            
            LoadData(id, source, loadOperationStates, ct);

            return combinedReplayStates;
        }
    }

}

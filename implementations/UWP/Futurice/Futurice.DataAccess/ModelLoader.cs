using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public abstract class ModelLoader
    {
        private readonly ICache _cache;

        public ModelLoader(ICache defaultCache = null)
        {
            _cache = defaultCache;
        }

        protected abstract void LoadImplementation(ModelIdentifier id, IObserver<IOperationState<IBuffer>> target);

        protected abstract void ParseImplementation(ModelIdentifier id, IBuffer data, IObserver<IOperationState<object>> target);

        protected virtual ICache GetCache(ModelIdentifier id)
        {
            return _cache;
        }

        private void LoadData(ModelIdentifier id, ModelSource source, Subject<IOperationState<IBuffer>> target)
        {
            ICache cache = GetCache(id);

            switch (source)
            {
                case ModelSource.Server:
                    target.OnResult(it => cache?.Save(id, it));
                    LoadImplementation(id, target);
                    break;

                case ModelSource.Disk:
                    if (cache == null)
                    {
                        target.OnNextError(new OperationError(message: "Disk cache not set!"), 0, ModelSource.Disk);
                        target.OnCompleted();
                    }
                    else
                    {
                        cache.Load(id, target);
                    }
                    break;

                default:
                    throw new Exception("Unknown source: " + source);
            }
        }

        public IObservable<IOperationState<object>> Load(ModelIdentifier id, ModelSource source, double loadOperationProgressShare = 0.8)
        {
            var loadOperationStates = new Subject<IOperationState<IBuffer>>();
            var parseOperationStates = new Subject<IOperationState<object>>();
            
            var combinedReplayStates = new ReplaySubject<IOperationState<object>>();
            Observable.CombineLatest(
                    loadOperationStates
                        .OnResult(buffer => ParseImplementation(id, buffer, parseOperationStates))
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
                    var latestError = oldLoad != newLoad ? loadError ?? parseError : parseError ?? loadError;

                    return new OperationState<object>(
                        newParse?.Result,
                        newParse?.Progress == 100
                            ? 100
                            : newLoad.Progress * loadOperationProgressShare + ((newParse?.Progress ?? 0) * (1.0 - loadOperationProgressShare)),
                        latestError,
                        false, // TODO: Cancelled?
                        source,
                        newParse?.ResultIdentifier);
                })
                .Subscribe(combinedReplayStates);
            
            LoadData(id, source, loadOperationStates);

            return combinedReplayStates;
        }
    }

}

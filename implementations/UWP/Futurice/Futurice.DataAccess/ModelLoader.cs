using System;
using System.Collections.Generic;
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

        public IObservable<IOperationState<object>> Load(ModelIdentifier id, ModelSource source, double loadOperationProgressShare = 80)
        {
            var loadOperationStates = new Subject<IOperationState<IBuffer>>();
            var parseOperationStates = new Subject<IOperationState<object>>();

            object latestModel = null;
            ModelIdentifier latestId = null;
            OperationError latestError = null;
            double loadProgress = 0;
            double parserProgress = 0;
            
            var combinedReplayStates = new ReplaySubject<IOperationState<object>>();
            Observable.Merge(
                    loadOperationStates.Select(_ => Unit.Default),

                    parseOperationStates
                        .Do(s =>
                        {
                            latestError = s.Error ?? latestError;
                            parserProgress = s.Progress;
                            if (s.Result != null)
                            {
                                latestModel = s.Result;
                                latestId = s.ResultIdentifier;
                            }
                        })
                        .Select(_ => Unit.Default)
                )
                .Select(s => new OperationState<object>(
                                latestModel,
                                parserProgress == 100 ? 100 : loadProgress * loadOperationProgressShare + (parserProgress * (100 - loadOperationProgressShare) / 100), 
                                latestError,
                                false, // TODO: Cancelled?
                                source,
                                latestId)
                        )
                .Subscribe(combinedReplayStates);
            
            loadOperationStates.SubscribeStateChange(
                onProgress: v => loadProgress = v,
                onResult: data => ParseImplementation(id, data, parseOperationStates),
                onError: e => latestError = e ?? latestError
            );

            LoadData(id, source, loadOperationStates);

            return combinedReplayStates;
        }
    }

}

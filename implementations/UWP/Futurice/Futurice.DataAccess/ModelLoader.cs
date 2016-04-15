using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
                        target.OnNextError(new OperationError(message: "Disk cache not set!"), 0);
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

            object latestResult = null;
            ModelIdentifier latestId = null;
            var combinedReplayStates = new ReplaySubject<IOperationState<object>>();
            Observable.Merge(
                loadOperationStates
                    .WhereProgressChanged()
                    .Select(loadState => new OperationState<object>(null, loadState.Progress * loadOperationProgressShare / 100, loadState.Error, loadState.IsCancelled)),

                parseOperationStates
                    .Do(s =>
                    {
                        if (s.Result != null)
                        {
                            latestResult = s.Result;
                            latestId = s.ResultIdentifier;
                        }
                    })
                    .Select(s => new OperationState<object>(latestResult, loadOperationProgressShare + (s.Progress * (100 - loadOperationProgressShare) / 100), s.Error, s.IsCancelled, s.ResultSource, latestId))
            ).Subscribe(combinedReplayStates);


            loadOperationStates.SubscribeStateChange(onResult: data => ParseImplementation(id, data, parseOperationStates));
            LoadData(id, source, loadOperationStates);

            return combinedReplayStates;
        }
    }

}

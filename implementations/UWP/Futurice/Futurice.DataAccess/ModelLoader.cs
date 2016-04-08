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
        private readonly Cache _cache;

        public ModelLoader(Cache defaultCache = null)
        {
            _cache = defaultCache;
        }

        protected abstract IObservable<IOperationState<IBuffer>> LoadImplementation(ModelIdentifier id);

        protected abstract IObservable<IOperationState<object>> ParseImplementation(ModelIdentifier id, IBuffer data);

        protected virtual Cache GetCache(ModelIdentifier id)
        {
            return _cache;
        }

        private IObservable<IOperationState<IBuffer>> LoadData(ModelIdentifier id, ModelSource source)
        {
            Cache cache = GetCache(id);

            switch (source)
            {
                case ModelSource.Server:
                    var fromServer = LoadImplementation(id);
                    return fromServer.OnResult(it => cache?.Save(id, it));

                case ModelSource.Disk:
                    return (cache == null) ?
                        Observable.Return(new OperationState<IBuffer>(null, 0, new OperationError(message: "Disk cache not set!"), false, ModelSource.Disk)) :
                        cache.Load(id);
                default:
                    throw new Exception("Unknown source: " + source);
            }
        }

        public IObservable<IOperationState<object>> Load(ModelIdentifier id, ModelSource source, double loadOperationProgressShare = 80)
        {
            var loadStates = LoadData(id, source);

            object latestResult = null;
            ModelIdentifier latestId = null;
            var subject = new ReplaySubject<IOperationState<object>>();
            Observable.Merge(
                loadStates
                    .WhereProgressChanged()
                    .Select(loadState => new OperationState<object>(null, loadState.Progress * loadOperationProgressShare / 100, loadState.Error, loadState.IsCancelled)),

                loadStates
                    .WhereResultChanged()
                    .SelectMany(state => ParseImplementation(id, state.Result))
                    .Do(s =>
                    {
                        if (s.Result != null)
                        {
                            latestResult = s.Result;
                            latestId = s.ResultIdentifier;
                        }
                    })
                    .Select(s => new OperationState<object>(latestResult, loadOperationProgressShare + (s.Progress * (100 - loadOperationProgressShare) / 100), s.Error, s.IsCancelled, s.ResultSource, latestId))
            ).Subscribe(subject);
            
            return subject;
        }
    }

}

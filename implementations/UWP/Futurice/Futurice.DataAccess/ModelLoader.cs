using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public abstract class ModelLoader
    {
        private Cache _cache;

        public ModelLoader(Cache defaultCache = null)
        {
            _cache = defaultCache;
        }

        protected abstract IObservable<OperationState<IBuffer>> LoadImplementation(ModelIdentifier id);

        protected abstract IObservable<OperationState<object>> ParseImplementation(ModelIdentifier id, IBuffer data);

        protected virtual Cache GetCache(ModelIdentifier id)
        {
            return _cache;
        }

        private IObservable<OperationState<IBuffer>> LoadData(ModelIdentifier id, ModelSource source)
        {
            Cache cache = GetCache(id);

            switch (source)
            {
                case ModelSource.Server:
                    var fromServer = LoadImplementation(id);
                    fromServer.OnResult(it => cache?.Save(id, it)).Subscribe();
                    return fromServer;

                case ModelSource.Disk:
                    return (cache == null) ?
                        Observable.Return(new OperationState<IBuffer>(null, 0, new OperationError(), false, ModelSource.Disk)) :
                        cache.Load(id);
                default:
                    throw new Exception("Unknown source: " + source);
            }
        }

        public IObservable<OperationState<object>> Load(ModelIdentifier id, ModelSource source)
        {
            var loadStates = LoadData(id, source);

            object latestResult = null;
            return Observable.Merge(
                loadStates
                    .WhereProgressChanged()
                    .Select(loadState => new OperationState<object>(null, loadState.Progress, loadState.Error, loadState.IsCancelled)),

                loadStates
                    .WhereResultChanged()
                    .SelectMany(state => ParseImplementation(id, state.Result))
            )
            .Do(s => latestResult = s.Result ?? latestResult)
            .Select(s => new OperationState<object>(latestResult, s.Progress, s.Error, s.IsCancelled));
        }
    }

}

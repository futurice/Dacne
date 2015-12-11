using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Reactive.Subjects;
using System.Collections.Concurrent;

namespace Futurice.DataAccess
{
    public class ModelIdentifierComparer : IEqualityComparer<ModelIdentifier>
    {
        public bool Equals(ModelIdentifier x, ModelIdentifier y)
        {
            return x.Equals(y);
        }

        public int GetHashCode(ModelIdentifier obj)
        {
            return obj.GetHashCode();
        }
    }

    class OperationKey
    {
        public readonly ModelIdentifier Identifier;
        public readonly ModelSource Source;

        public OperationKey(ModelIdentifier identifier, ModelSource source)
        {
            Identifier = identifier;
            Source = source;
        }

        public override bool Equals(object obj)
        {
            var other = obj as OperationKey;
            return Identifier.Equals(other.Identifier) && Source.Equals(other.Source);
        }

        public override int GetHashCode()
        {
            return Identifier.GetHashCode() + 3 * Source.GetHashCode();
        }
    }

    public enum SourcePreference
    {
        Unknown,
        Server,
        Cache,
        ServerWithCacheFallback,
        CacheWithServerFallback,
        FirstCacheThenServer,
    }
    
    public enum ModelSource
    {
        Unknown,
        Memory,
        Disk,
        Server
    }

    public abstract class ModelRepository
    { 
        protected abstract T GetFromMemory<T>(ModelIdentifier id) where T : class;

        private readonly ModelLoader _loader;

        public ModelRepository(ModelLoader loader)
        {
            _loader = loader;
        }
        
        private ConcurrentDictionary<OperationKey, object> operations = new ConcurrentDictionary<OperationKey, object>();

        public IObservable<OperationState<T>> Get<T>(ModelIdentifier id, SourcePreference source = SourcePreference.ServerWithCacheFallback, CancellationToken ct = default(CancellationToken)) where T : class
        {

            switch (source) {
                case SourcePreference.Cache:
                    var result = GetFromMemory<T>(id);
                    return result != null ? Observable.Return(new OperationState<T>(result, 100, source: ModelSource.Memory)) : Get<T>(id, ModelSource.Disk, ct);

                case SourcePreference.Server:
                    return Get<T>(id, ModelSource.Server, ct);
                    
                default:
                    throw new NotImplementedException("Unknown SourcePreference: " + source.ToString());
            }

        }

        private IObservable<OperationState<T>> Get<T>(ModelIdentifier id, ModelSource source, CancellationToken ct = default(CancellationToken)) where T : class
        {
            var key = new OperationKey(id, source);

            return operations.GetOrAdd(key, _ => {
                var newOperation = GetModel<T>(id, source, ct);
                newOperation.Connect();
                return newOperation;
            }) as IObservable<OperationState<T>>;
        }

        private IConnectableObservable<OperationState<T>> GetModel<T>(ModelIdentifier id, ModelSource source, CancellationToken ct = default(CancellationToken)) where T : class
        {
            var operation = _loader.Load(id, source);
            return operation
                .Select(modelsState => {
                    T result = modelsState.Result as T;
                    return new OperationState<T>(result, result != null ? 100 : modelsState.Progress, modelsState.Error, modelsState.IsCancelled, modelsState.ResultSource);
                })
                .Replay();
        }
    }
}

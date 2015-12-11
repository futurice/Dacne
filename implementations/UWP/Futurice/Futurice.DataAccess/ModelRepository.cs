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
        /// <summary>
        /// Default value that throws an exception.
        /// </summary>
        Unknown,

        /// <summary>
        /// Requests from server only.
        /// </summary>
        Server,

        /// <summary>
        /// Requests from cache only.
        /// </summary>
        Cache, // Do we need to seperate between memory and disk ?

        /// <summary>
        /// Requests from server, if the request fails, requests from cache.
        /// </summary>
        ServerWithCacheFallback,

        /// <summary>
        /// Requests from cache, if the request fails, requests from server.
        /// </summary>
        CacheWithServerFallback,

        /// <summary>
        /// Requests from cache, when the request fails or completes, requests from server.
        /// </summary>
        FirstCacheThenServer,

        /// <summary>
        /// Doesn't make any requests, but binds to receive the result from a request started by other operations.
        /// </summary>
        Delayed, // Don't start any operations
    }
    
    public enum ModelSource
    {
        Unknown,
        Memory,
        Disk,
        Server
    }

    /// <summary>
    /// A class that manages ongoing operations and bindings to their updates, manages a memory cache for the models, and has the logic to load models according to different settings.
    /// </summary>
    public abstract class ModelRepository
    { 
        protected abstract T GetFromMemory<T>(ModelIdentifier id) where T : class;

        private readonly ModelLoader _loader;

        public ModelRepository(ModelLoader loader)
        {
            _loader = loader;
        }
        
        private ConcurrentDictionary<OperationKey, object> operations = new ConcurrentDictionary<OperationKey, object>();

        /// <summary>
        /// Takes the model information and operation settings and returns and stream of updates which can be accessed to read the progress and result of the operation.
        /// </summary>
        /// <typeparam name="T">Type of the model object to get.</typeparam>
        /// <param name="id">Identifier for which to get the model object.</param>
        /// <param name="source">Determines the sources from which the model is loaded from, or if an request should be started at all.</param>
        /// <returns>Stream of updates from which the progress and result of the operation can be accessed from.</returns>
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

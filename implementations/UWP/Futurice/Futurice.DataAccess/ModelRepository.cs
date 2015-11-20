using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using System.Reactive.Subjects;

namespace Futurice.DataAccess
{
    public abstract class ModelRepository
    { 
        protected abstract T GetFromMemory<T>(ModelIdentifier id) where T : class;

        private readonly ModelLoader _loader;

        public ModelRepository(ModelLoader loader)
        {
            _loader = loader;
        }

        private Dictionary<ModelIdentifier, object> operations = new Dictionary<ModelIdentifier, object>();

        public IObservable<OperationState<T>> Get<T>(ModelIdentifier id, CancellationToken cancellation = default(CancellationToken)) where T : class
        {
            object operation = null;
            if (operations.TryGetValue(id, out operation)) {
                return operation as IObservable<OperationState<T>>;
            }

            var newOperation = GetModel<T>(id);
            operations[id] = newOperation;
            newOperation.Connect();
            return newOperation;
        }

        private IConnectableObservable<OperationState<T>> GetModel<T>(ModelIdentifier id) where T : class
        {
            var operation = _loader.Load(id);
            return operation
                .Select(modelsState => {
                    T result = modelsState.Result as T;
                    return new OperationState<T>(result, result != null ? 100 : modelsState.Progress, modelsState.Error);
                })
                .Replay();
        }
    }
}

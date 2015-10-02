using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;

namespace Futurice.DataAccess
{
    public class ModelRepository
    {
        private readonly ModelLoader _loader;

        public ModelRepository(ModelLoader loader)
        {
            _loader = loader;
        }

        public ModelGetOperation<T> Get<T>(ModelId id) where T : class
        {
            
            return new ModelGetOperation<T>((_) => {
                var operation = _loader.Load(id);
                var states = operation.Begin();
                
                return states.Select(modelsState => new OperationState<T>(modelsState.Result as T, modelsState.Progress, modelsState.Error));
            }, id);
        }
    }
}

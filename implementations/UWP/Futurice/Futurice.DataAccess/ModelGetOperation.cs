using System;

namespace Futurice.DataAccess
{
    public class ModelGetOperation<T> : Operation<T>
    {
        public ModelGetOperation(Func<ModelIdentifier, IObservable<OperationState<T>>> begin, ModelIdentifier id) : base(() => begin(id))
        {
            Id = id;
        }
        
        public ModelIdentifier Id { get; private set; }

    }
}
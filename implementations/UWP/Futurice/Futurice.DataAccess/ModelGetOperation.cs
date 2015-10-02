using System;

namespace Futurice.DataAccess
{
    public class ModelGetOperation<T> : Operation<T>
    {
        public ModelGetOperation(Func<ModelId, IObservable<OperationState<T>>> begin, ModelId id) : base(() => begin(id))
        {
            Id = id;
        }
        
        public ModelId Id { get; private set; }
    }
}
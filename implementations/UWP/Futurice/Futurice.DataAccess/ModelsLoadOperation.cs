using System;

namespace Futurice.DataAccess
{
    public class ModelsLoadOperation : Operation<object>
    {
        public ModelsLoadOperation(Func<IObservable<OperationState<object>>> begin) : base(begin) { }
    }
}
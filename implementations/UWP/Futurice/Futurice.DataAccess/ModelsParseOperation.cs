using System;

namespace Futurice.DataAccess
{
    public class ModelsParseOperation : Operation<object>
    {
        public ModelsParseOperation(Func<IObservable<OperationState<object>>> begin) : base(begin) { }

    }
}
using System;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public abstract class Parser
    {
        public abstract IObservable<OperationState<object>> Parse(IBuffer stream);
    }
}
using System;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public interface IParser
    {
        IObservable<OperationState<object>> Parse(IBuffer stream);
    }
}
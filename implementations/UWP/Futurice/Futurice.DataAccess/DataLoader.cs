using System;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public interface DataLoader
    {
        IObservable<IOperationState<IBuffer>> Load(ModelIdentifier id);
    }
}

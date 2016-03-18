using System;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public interface DataLoader
    {
        IObservable<OperationState<IBuffer>> Load(ModelIdentifier id);
    }
}

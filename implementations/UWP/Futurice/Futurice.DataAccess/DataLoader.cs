using System;
using System.Threading;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public interface IDataLoader
    {
        void Load(ModelIdentifier id, IObserver<IOperationState<IBuffer>> target, CancellationToken ct = default(CancellationToken));
    }
}

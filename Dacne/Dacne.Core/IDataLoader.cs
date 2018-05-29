using System;
using System.IO;
using System.Threading;

namespace Dacne.Core
{
    public interface IDataLoader
    {
        void Load(ModelIdentifierBase id, IObserver<IOperationState<Stream>> target, CancellationToken ct = default);
    }
}

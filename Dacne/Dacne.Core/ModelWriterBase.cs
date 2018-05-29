using System;
using System.Reactive.Subjects;
using System.Threading;

namespace Futurice.DataAccess
{
    public abstract class ModelWriterBase
    {
        protected abstract void WriteImplementation(ModelIdentifierBase id, UpdateContainer update, ModelSource target, IObserver<IOperationState<object>> operation, CancellationToken ct = default);

        public IObservable<IOperationState<object>> Write(ModelIdentifierBase id, UpdateContainer update, ModelSource target, CancellationToken ct = default)
        {
            var replayStates = new ReplaySubject<IOperationState<object>>();

            WriteImplementation(id, update, target, replayStates, ct);

            return replayStates;
        }
    }

}

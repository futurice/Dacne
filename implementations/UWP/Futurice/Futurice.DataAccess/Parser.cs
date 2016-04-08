using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public abstract class SimpleParser : Parser
    {
        protected abstract object ParseImplementation(IBuffer data, ModelIdentifier id);

        protected override void ParseImplementation(IBuffer data, ModelIdentifier id, IObserver<IOperationState<object>> target)
        {
            target.OnNext(new OperationState<object>(ParseImplementation(data, id), 100, null, false, ModelSource.Server));
        }

    }

    public abstract class Parser
    {
        protected abstract void ParseImplementation(IBuffer data, ModelIdentifier id, IObserver<IOperationState<object>> target);

        public IObservable<IOperationState<object>> Parse(IBuffer data, ModelIdentifier id)
        {
            var subject = new Subject<IOperationState<object>>();
            
            Task.Run(() => {
                try
                {
                    ParseImplementation(data, id, subject);
                }
                catch (Exception e)
                {
                    subject.OnNext(new OperationState<object>(null, 100, new OperationError(e), false, ModelSource.Server));
                }
                subject.OnCompleted();
            });

            return subject;
        }

    }
}
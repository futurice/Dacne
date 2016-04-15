using System;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public abstract class SimpleParser : Parser
    {
        protected abstract object ParseImplementation(ModelIdentifier id, IBuffer data);

        protected override void ParseImplementation(ModelIdentifier id, IBuffer data, IObserver<IOperationState<object>> target)
        {
            target.OnNextResult(ParseImplementation(id, data), id, 100, ModelSource.Server);
            target.OnCompleted();
        }

    }

    public abstract class Parser
    {
        protected abstract void ParseImplementation(ModelIdentifier id, IBuffer data, IObserver<IOperationState<object>> target);

        public void Parse(ModelIdentifier id, IBuffer data, IObserver<IOperationState<object>> target)
        {
            Task.Run(() => {
                try
                {
                    ParseImplementation(id, data, target);
                }
                catch (Exception e)
                {
                    target.OnNextError(new OperationError(e), 100, ModelSource.Server);
                    target.OnCompleted();
                }
            });
        }

    }
}
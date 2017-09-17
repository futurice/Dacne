using System;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public abstract class SimpleParser : Parser
    {
        protected abstract object ParseImplementation(ModelIdentifierBase id, IBuffer data);

        protected override void ParseImplementation(ModelIdentifierBase id, IBuffer data, IObserver<IOperationState<object>> target)
        {
            target.OnCompleteResult(ParseImplementation(id, data), id, 100, ModelSource.Server);
            target.OnCompleted();
        }

    }

    public abstract class Parser
    {
        protected abstract void ParseImplementation(ModelIdentifierBase id, IBuffer data, IObserver<IOperationState<object>> target);

        public void Parse(ModelIdentifierBase id, IBuffer data, IObserver<IOperationState<object>> target)
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
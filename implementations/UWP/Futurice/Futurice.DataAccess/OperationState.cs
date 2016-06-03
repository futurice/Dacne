
namespace Futurice.DataAccess
{
    public interface IOperationState<out T> : IOperationStateBase where T : class
    {
        T Result { get; }
    }

    public class OperationState<T> : OperationStateBase, IOperationState<T> where T : class
    {
        public OperationState(T result = null, double progress = 0, OperationError error = null, bool isCancelled = false, ModelSource source = ModelSource.Unknown, ModelIdentifier id = null, double resultProgress = 0) : base(error, progress, isCancelled, source, id, resultProgress)
        {
            Result = result;
        }

        public T Result { get; private set; }
    }
    
    public abstract class OperationStateBase : IOperationStateBase
    {

        public OperationStateBase(OperationError error, double progress, bool isCancelled, ModelSource source = ModelSource.Unknown, ModelIdentifier id = null, double resultProgress = 0)
        {
            Error = error;
            Progress = progress;
            IsCancelled = isCancelled;
            ResultSource = source;
            ResultIdentifier = id;
            ResultProgress = resultProgress;
        }

        public OperationError Error { get; private set; }
        public double Progress { get; private set; }
        public bool IsCancelled { get; private set; }
        public ModelSource ResultSource { get; private set; }
        public ModelIdentifier ResultIdentifier { get; private set; }
        public double ResultProgress { get; set; }
    }
}
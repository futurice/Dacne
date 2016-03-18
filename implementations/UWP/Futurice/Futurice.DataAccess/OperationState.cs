
namespace Futurice.DataAccess
{
    public class OperationState<T> : OperationStateBase where T : class
    {
        public OperationState(T result = null, double progress = 0, OperationError error = null, bool isCancelled = false, ModelSource source = ModelSource.Unknown, ModelIdentifier id = null) : base(error, progress, isCancelled)
        {
            Result = result;
            ResultSource = source;
            ResultIdentifier = id;
        }

        public T Result { get; private set; }

        public ModelSource ResultSource { get; private set; }

        public ModelIdentifier ResultIdentifier { get; private set; }
    }
    
    public abstract class OperationStateBase
    {

        public OperationStateBase(OperationError error, double progress, bool isCancelled)
        {
            Error = error;
            Progress = progress;
            IsCancelled = isCancelled;
        }

        public OperationError Error { get; private set; }
        public double Progress { get; private set; }
        public bool IsCancelled { get; private set; }
    }
}
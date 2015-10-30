
namespace Futurice.DataAccess
{
    public class OperationState<T>
    {
        public OperationState(T result, double progress = 0, OperationError error = null, bool isCancelled = false)
        {
            Result = result;
            Progress = progress;
            Error = error;
            IsCancelled = isCancelled;
        }

        public OperationError Error { get; private set; }
        public double Progress { get; private set; }
        public T Result { get; private set; }
        public bool IsCancelled { get; private set; }
    }
}
namespace Futurice.DataAccess
{
    public interface IOperationStateBase
    {
        OperationError Error { get; }
        bool IsCancelled { get; }
        double Progress { get; }
        ModelIdentifier ResultIdentifier { get; }
        ModelSource ResultSource { get; }
    }
}
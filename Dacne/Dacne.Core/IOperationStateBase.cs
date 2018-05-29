namespace Dacne.Core
{
    public interface IOperationStateBase
    {
        OperationError Error { get; }
        bool IsCancelled { get; }
        double Progress { get; }
        ModelIdentifierBase ResultIdentifier { get; }
        ModelSource ResultSource { get; }
        double ResultProgress { get; set; }
    }
}
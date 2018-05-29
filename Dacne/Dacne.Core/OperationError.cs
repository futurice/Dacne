using System;

namespace Dacne.Core
{

    public class OperationError
    {
        public readonly Exception Exception;
        private readonly string Message;

        public OperationError(Exception exception = null, string message = null)
        {
            Exception = exception;
            Message = message;
        }

        public override string ToString()
        {
            return Message ?? "" + Exception?.ToString() ?? ""; 
        }
    }
}
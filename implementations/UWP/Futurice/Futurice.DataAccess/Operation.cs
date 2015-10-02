using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futurice.DataAccess
{
    public class Operation<T>
    {
        private readonly Func<IObservable<OperationState<T>>> _begin;

        public Operation(Func<IObservable<OperationState<T>>> begin)
        {
            _begin = begin;
        }

        public IObservable<OperationState<T>> Begin()
        { 
            return _begin();
        }
    }
}

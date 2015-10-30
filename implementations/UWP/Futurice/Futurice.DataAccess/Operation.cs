using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using System.Reactive.Linq;

namespace Futurice.DataAccess
{
    public class Operation<T>
    {
        public void Cancel()
        {

        }

        private IObservable<OperationState<T>> _states;
        private readonly Func<IObservable<OperationState<T>>> _begin;

        public Operation(Func<IObservable<OperationState<T>>> begin)
        {
            _begin = begin;
        }

        public IObservable<OperationState<T>> Begin()
        { 
            _states = _begin();
            return _states;
        }
        
    }
}

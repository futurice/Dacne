using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futurice.DataAccess
{

    public static class ObservableOperationStateExtensions
    {
        public static IObservable<OperationState<T>> OnResult<T>(this IObservable<OperationState<T>> self, Action<T> onResult)
        {
            self.Where(state => state.Result != null).Do(state => onResult(state.Result));
            return self;
        }

        public static IDisposable SubscribeStateChange<T>(this IObservable<OperationState<T>> self, Action<T> onResult = null, Action<double> onProgress = null, Action<OperationError> onError = null) where T : class
        {
            T result = null;
            double progress = 0;
            OperationError error = null;
            return self
                .Subscribe(state => {
                    TryFire(onProgress, state.Progress, ref progress);
                    TryFire(onError, state.Error, ref error);
                    TryFire(onResult, state.Result, ref result);
                }
            );
        }

        private static void TryFire<T>(Action<T> onChanged, T newValue, ref T oldValue)
        {
            if (onChanged != null && !Object.Equals(newValue, oldValue)) {
                oldValue = newValue;
                onChanged(oldValue);
            }
        }

        private static bool HasChanged<T>(ref T oldValue, T newValue)
        {
            if (!Object.Equals(newValue, oldValue)) {
                oldValue = newValue;
                return true;
            }

            return false;
        }

        public static IObservable<OperationState<TResult>> WhereChanged<TResult, TProperty>(this IObservable<OperationState<TResult>> self, Func<OperationState<TResult>, TProperty> selector)
        {
            TProperty def = default(TProperty);
            TProperty oldValue = def;
            return self.Where(state => {
                var newValue = selector(state);
                return !Object.Equals(newValue, def) && HasChanged(ref oldValue, newValue);
            });
        }

        public static IObservable<OperationState<TResult>> WhereResultChanged<TResult>(this IObservable<OperationState<TResult>> self)
        {
            return self.WhereChanged(state => state.Result);
        }
        public static IObservable<OperationState<TResult>> WhereErrorChanged<TResult>(this IObservable<OperationState<TResult>> self)
        {
            return self.WhereChanged(state => state.Error);
        }
        public static IObservable<OperationState<TResult>> WhereProgressChanged<TResult>(this IObservable<OperationState<TResult>> self)
        {
            return self.WhereChanged(state => state.Progress);
        }
    }
}

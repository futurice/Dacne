using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Futurice.DataAccess
{

    public static class ObservableOperationStateExtensions
    {
        public static IObservable<IOperationState<TResult>> OnResult<TResult>(this IObservable<IOperationState<TResult>> self, Action<TResult> onResult) where TResult : class
        {
            self.Where(state => state.Result != null).Do(state => onResult(state.Result));
            return self;
        }

        public static IDisposable SubscribeStateChange<TResult>(this IObservable<IOperationState<TResult>> self, Action<TResult> onResult = null, Action<double> onProgress = null, Action<OperationError> onError = null) where TResult : class
        {
            TResult result = null;
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

        public static IObservable<IOperationState<TResult>> WhereChanged<TResult, TProperty>(this IObservable<IOperationState<TResult>> self, Func<IOperationState<TResult>, TProperty> selector) where TResult : class
        {
            TProperty def = default(TProperty);
            TProperty oldValue = def;
            return self.Where(state => {
                var newValue = selector(state);
                return !Object.Equals(newValue, def) && HasChanged(ref oldValue, newValue);
            });
        }

        public static IObservable<IOperationState<TResult>> WhereResultChanged<TResult>(this IObservable<IOperationState<TResult>> self) where TResult : class
        {
            return self.WhereChanged(state => state.Result);
        }
        public static IObservable<IOperationState<TResult>> WhereErrorChanged<TResult>(this IObservable<IOperationState<TResult>> self) where TResult : class
        {
            return self.WhereChanged(state => state.Error);
        }
        public static IObservable<IOperationState<TResult>> WhereProgressChanged<TResult>(this IObservable<IOperationState<TResult>> self) where TResult : class
        {
            return self.WhereChanged(state => state.Progress);
        }

        public static IObservable<IOperationState<TResult>> WithFallback<TResult>(
            this IObservable<IOperationState<TResult>> forOperation,
            Func<IObservable<IOperationState<TResult>>> fallback) where TResult : class
        {
            IObservable<IOperationState<TResult>> doFallback = forOperation
                .WhereErrorChanged()
                .Take(1)
                .SelectMany(it => fallback());

            return forOperation
                .TakeWhile(it => it.Error == null)
                .Merge(doFallback);
        }

        public static IObserver<IOperationState<TResult>> OnNextProgress<TResult>(this IObserver<IOperationState<TResult>> self, double progress) where TResult : class
        {
            self.OnNext(new OperationState<TResult>(progress: progress));
            return self;
        }

        public static IObserver<IOperationState<TResult>> OnNextResult<TResult>(this IObserver<IOperationState<TResult>> self, TResult result, ModelIdentifier id, double progress = 100) where TResult : class
        {
            self.OnNext(new OperationState<TResult>(result: result, id: id, progress: progress));
            return self;
        }

        public static IObserver<IOperationState<TResult>> OnNextError<TResult>(this IObserver<IOperationState<TResult>> self, OperationError error, double progress = 100) where TResult : class
        {
            self.OnNext(new OperationState<TResult>(error: error, progress: progress));
            return self;
        }
    }
}

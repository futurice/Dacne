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
            TResult oldValue = default(TResult);
            return self.Do(state =>
            {
                if (HasChanged(ref oldValue, state.Result))
                {
                    onResult(state.Result);
                }
            });
        }

        public static IDisposable SubscribeStateChange<TResult>(this IObservable<IOperationState<TResult>> self, Action<TResult> onResult = null, Action<double> onProgress = null, Action<OperationError> onError = null, Action<IOperationState<TResult>> onCompleted = null) where TResult : class
        {
            TResult result = null;
            ModelSource source = ModelSource.Unknown;
            ModelIdentifier id = null;
            double progress = 0;
            OperationError error = null;
            bool isCancelled = false;
            return self
                .Subscribe(state => {
                    TryFire(onProgress, ref progress, state.Progress);
                    TryFire(onError, ref error, state.Error);

                    if (TryFire(onResult, ref result, state.Result))
                    {
                        id = state.ResultIdentifier;
                        source = state.ResultSource;
                    }

                    isCancelled = state.IsCancelled;
                },
                () => onCompleted?.Invoke(new OperationState<TResult>(result, progress, error, isCancelled, source, id))
            );
        }

        private static bool TryFire<T>(Action<T> onChanged, ref T oldValue, T newValue)
        {
            if (HasChanged(ref oldValue, newValue)) {
                onChanged?.Invoke(newValue);
                return true;
            }

            return false;
        }

        private static bool HasChanged<T>(ref T oldValue, T newValue)
        {
            if (!Object.Equals(newValue, default(T)) && !Object.Equals(newValue, oldValue)) {
                oldValue = newValue;
                return true;
            }

            return false;
        }

        public static IObservable<IOperationState<TResult>> WhereChanged<TResult, TProperty>(this IObservable<IOperationState<TResult>> self, Func<IOperationState<TResult>, TProperty> selector) where TResult : class
        {
            TProperty oldValue = default(TProperty);
            return self.Where(state => {
                var newValue = selector(state);
                return HasChanged(ref oldValue, newValue);
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

        public static IObserver<IOperationState<TResult>> OnNextProgress<TResult>(this IObserver<IOperationState<TResult>> self, double progress, ModelSource source = ModelSource.Unknown) where TResult : class
        {
            self.OnNext(new OperationState<TResult>(progress: progress, source: source));
            return self;
        }

        public static IObserver<IOperationState<TResult>> OnCompleteResult<TResult>(this IObserver<IOperationState<TResult>> self, TResult result, ModelIdentifier id, double progress, ModelSource source = ModelSource.Unknown) where TResult : class
        {
            self.OnNext(new OperationState<TResult>(result: result, id: id, progress: progress, source: source, resultProgress: 100));
            return self;
        }

        public static IObserver<IOperationState<TResult>> OnIncompleteResult<TResult>(this IObserver<IOperationState<TResult>> self, TResult result, ModelIdentifier id, double progress, double resultProgress, ModelSource source = ModelSource.Unknown) where TResult : class
        {
            self.OnNext(new OperationState<TResult>(result: result, id: id, progress: progress, source: source, resultProgress: resultProgress));
            return self;
        }

        public static IObserver<IOperationState<TResult>> OnNextError<TResult>(this IObserver<IOperationState<TResult>> self, OperationError error, double progress, ModelSource source = ModelSource.Unknown) where TResult : class
        {
            self.OnNext(new OperationState<TResult>(error: error, progress: progress, source: source));
            return self;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public abstract class ModelWriter
    {
        private readonly ICache _cache;

        public ModelWriter(ICache defaultCache = null)
        {
            _cache = defaultCache;
        }

        protected abstract void WriteImplementation(ModelIdentifier id, IObserver<IOperationState<IBuffer>> target, CancellationToken ct = default(CancellationToken));

        protected abstract void ParseImplementation(ModelIdentifier id, IBuffer data, IObserver<IOperationState<object>> target, CancellationToken ct = default(CancellationToken));

        protected virtual ICache GetCache(ModelIdentifier id)
        {
            return _cache;
        }


        private void WriteData(ModelIdentifier id, ModelSource source, Subject<IOperationState<IBuffer>> target, CancellationToken ct = default(CancellationToken))
        {
            ICache cache = GetCache(id);

            switch (source)
            {
                case ModelSource.Server:
                    target.OnResult(it => cache?.Save(id, it));
                    WriteImplementation(id, target, ct);
                    break;

                case ModelSource.Disk:
                    if (cache == null)
                    {
                        target.OnNextError(new OperationError(message: "Disk cache not set!"), 0, ModelSource.Disk);
                        target.OnError(new InvalidOperationException("Disk cache not set!"));
                        //target.OnCompleted();
                    }
                    else
                    {
                        cache.Write(id, target, ct);
                    }
                    break;

                default:
                    throw new Exception("Unknown source: " + source);
            }
        }

        public IObservable<IOperationState<object>> Write(ModelIdentifier id, ModelSource source, double WriteOperationProgressShare = 0.8, CancellationToken ct = default(CancellationToken))
        {
            var WriteOperationStates = new Subject<IOperationState<IBuffer>>();
            var parseOperationStates = new Subject<IOperationState<object>>();

            // Writeing and parsing might be running in parallel so we need some logic to combine the state updates
            var combinedReplayStates = new ReplaySubject<IOperationState<object>>();
            Observable.CombineLatest(
                    WriteOperationStates
                        .OnResult(buffer => ParseImplementation(id, buffer, parseOperationStates, ct))
                        .StartWith(null as IOperationState<IBuffer>),

                    parseOperationStates
                        .StartWith(null as IOperationState<object>),

                    (Write, parse) => Tuple.Create(Write, parse)
                )
                .Buffer(2, 1)
                .Where(pairs => pairs.Count == 2) // Last buffer
                .Select(pairs =>
                {
                    var olds = pairs[0];
                    var oldWrite = olds?.Item1;
                    var oldParse = olds?.Item2;

                    var news = pairs[1];
                    var newWrite = news.Item1;
                    var newParse = news.Item2;

                    var WriteError = newWrite.Error ?? oldWrite?.Error;
                    var parseError = newParse?.Error ?? oldParse?.Error;
                    var latestError = oldWrite != newWrite
                                        ? WriteError ?? parseError
                                        : parseError ?? WriteError;

                    return new OperationState<object>(
                        newParse?.Result,
                        newParse?.Progress == 100
                            ? 100
                            : newWrite.Progress * WriteOperationProgressShare + ((newParse?.Progress ?? 0) * (1.0 - WriteOperationProgressShare)),
                        latestError,
                        false, // TODO: Cancelled?
                        source,
                        newParse?.ResultIdentifier,
                        newParse?.ResultProgress ?? 0);
                })
                .Subscribe(combinedReplayStates);

            WriteData(id, source, WriteOperationStates, ct);

            return combinedReplayStates;
        }
    }
}

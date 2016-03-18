using System;
using System.Reactive.Subjects;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace Futurice.DataAccess
{
    public class NetworkRequestHander
    {
        public IObservable<OperationState<IBuffer>> Get(Uri uri)
        {
            var subject = new Subject<OperationState<IBuffer>>();
            var client = new HttpClient();

            client.GetBufferAsync(uri).AsTask().ContinueWith(result =>
            {
                subject.OnNext(new OperationState<IBuffer>(result.Result, 100, null, false, ModelSource.Server));
                subject.OnCompleted();
            });

            return subject;
        }
    }
}

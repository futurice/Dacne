using System;
using System.Diagnostics;
using System.Reactive.Subjects;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace Futurice.DataAccess
{
    public class NetworkRequestHander
    {
        public void Get(Uri uri, IObserver<IOperationState<IBuffer>> target)
        {
            var client = new HttpClient();

            Debug.WriteLine("Request started: " + uri.ToString());
            client.GetBufferAsync(uri).AsTask().ContinueWith(result =>
            {
                target.OnNextResult(result.Result, null, 100, ModelSource.Server);
                target.OnCompleted();
                Debug.WriteLine("Request completed: " + uri.ToString());
            });
        }
    }
}

using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;

namespace Futurice.DataAccess
{
    public class NetworkRequestHander
    {
        public void Get(Uri uri, IObserver<IOperationState<Stream>> target)
        {
            var client = new HttpClient();

            Debug.WriteLine("Request started: " + uri.ToString());
            client.GetStreamAsync(uri).ContinueWith(result =>
            {
                target.OnCompleteResult(result.Result, null, 100, ModelSource.Server);
                target.OnCompleted();
                Debug.WriteLine("Request completed: " + uri.ToString());
            });
        }
    }
}

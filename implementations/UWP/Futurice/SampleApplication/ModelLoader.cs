using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Futurice.DataAccess;
using Windows.Storage.Streams;

namespace SampleApplication
{
    public class ModelLoader : Futurice.DataAccess.ModelLoader
    {

        protected override IObservable<OperationState<IBuffer>> LoadImplementation(ModelIdentifier id, ModelSource source)
        {
            // Check that this model is supposed to be loaded from the bbc
            return new BbcDataLoader(new Uri("http://www.bbc.com/api")).Load(id, source);
        }

        protected override IObservable<OperationState<object>> ParseImplementation(ModelIdentifier id, IBuffer data)
        {
            // Check that this model is bbc data
            return Observable.Return(
                new OperationState<object>(
                    new NewsArticle() { Title = Encoding.UTF8.GetString(data.ToArray()) }, 100)
                );
        }
    }

    public static DataLoader
    {
        public IObservable<OperationState<IBuffer>> Load(ModelIdentifier id, ModelSource source)
        {
            if (source == ModelSource.Disk) {
                return Observable.Generate(0, p => p <= 100, p => ++p,
                    p => new OperationState<IBuffer>(p == 100 ? Encoding.UTF8.GetBytes("Article from disk").AsBuffer() : null, p),
                    p => TimeSpan.FromMilliseconds(1));
            } else {
                return Observable.Generate(0, p => p <= 100, p => ++p,
                    p => new OperationState<IBuffer>(p == 100 ? Encoding.UTF8.GetBytes("Article from server").AsBuffer() : null, p),
                    p => TimeSpan.FromMilliseconds(50));
            }
        }
    }
}

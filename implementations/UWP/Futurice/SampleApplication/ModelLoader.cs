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
using System.Collections.Concurrent;

namespace SampleApplication
{
    public class TestCache : Cache
    {
        private ConcurrentDictionary<ModelIdentifier, IBuffer> _cache =
            new ConcurrentDictionary<ModelIdentifier, IBuffer>();

        public IObservable<OperationState<IBuffer>> Load(ModelIdentifier id)
        {
            IBuffer value;

            return Observable.Return(
                _cache.TryGetValue(id, out value) ?
                new OperationState<IBuffer>(value, 100, null, false, ModelSource.Disk) :
                new OperationState<IBuffer>(null, 0, new OperationError(), false, ModelSource.Disk));
        }

        public void Save(ModelIdentifier id, IBuffer data)
        {
            _cache.AddOrUpdate(id, data, (_, __) => data);
        }
    }

    public class ModelLoader : Futurice.DataAccess.ModelLoader
    {
        protected override IObservable<OperationState<IBuffer>> LoadImplementation(ModelIdentifier id)
        {
            // Check that this model is supposed to be loaded from the bbc
            return new NetworkRequestHander().Get(new Uri("http://feeds.bbci.co.uk/news/rss.xml"));
        }

        protected override IObservable<OperationState<object>> ParseImplementation(ModelIdentifier id, IBuffer data)
        {
            return new DataContractXmlParser(typeof(Rss)).Parse(data);
        }
    }

    public class BbcDataLoader
    {
        private Uri _baseUri;

        public BbcDataLoader(Uri baseUri)
        {
            _baseUri = baseUri;
        }

        public IObservable<OperationState<IBuffer>> Load(ModelIdentifier id)
        {
            return Observable.Generate(0, p => p <= 100, p => ++p,
                p => new OperationState<IBuffer>(p == 100 ? Encoding.UTF8.GetBytes("Article from server").AsBuffer() : null, p),
                p => TimeSpan.FromMilliseconds(50));

            /*
            if (source == ModelSource.Disk) {
                return Observable.Generate(0, p => p <= 100, p => ++p,
                    p => new OperationState<IBuffer>(p == 100 ? Encoding.UTF8.GetBytes("Article from disk").AsBuffer() : null, p),
                    p => TimeSpan.FromMilliseconds(1));
            } else {
            }*/
        }
    }
}

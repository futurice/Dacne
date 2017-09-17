using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Futurice.DataAccess;
using Windows.Storage.Streams;
using System.Threading;

namespace SampleApplication
{
    public class ModelLoader : Futurice.DataAccess.ModelLoaderBase
    {
        protected override void LoadImplementation(ModelIdentifierBase id, IObserver<IOperationState<IBuffer>> target, CancellationToken ct)
        {
            // Check that this model is supposed to be loaded from the bbc
            new NetworkRequestHander().Get(new Uri("http://feeds.bbci.co.uk/news/rss.xml"), target);
        }

        protected override void ParseImplementation(ModelIdentifierBase id, IBuffer data, IObserver<IOperationState<object>> target, CancellationToken ct)
        {
            // Use the parser for this model
            new BbcParser().Parse(id, data, target);
        }
        
        public static BbcArticleIdentifier GetBbcArticleIdentifier(int item, params string[] sections)
        {
            return BbcParser.CreateItemId(item, sections);
        }

        public static ModelIdentifier<IEnumerable<NewsArticle>> GetBbcArticlesIdentifier()
        {
            return BbcParser.AllArticlesId;
        }
    }

    public class BbcDataLoader
    {
        private Uri _baseUri;

        public BbcDataLoader(Uri baseUri)
        {
            _baseUri = baseUri;
        }

        public IObservable<IOperationState<IBuffer>> Load(ModelIdentifierBase id)
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

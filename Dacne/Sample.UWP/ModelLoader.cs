using System;
using System.Collections.Generic;
using System.Text;
using Dacne.Core;
using System.Threading;
using System.IO;
using System.Reactive.Linq;

namespace SampleApplication
{
    public class ModelLoader : Dacne.Core.ModelLoaderBase
    {
        protected override void LoadImplementation(ModelIdentifierBase id, IObserver<IOperationState<Stream>> target, CancellationToken ct)
        {
            // Check that this model is supposed to be loaded from the bbc
            new NetworkRequestHander().Get(new Uri("http://feeds.bbci.co.uk/news/rss.xml"), target);
        }

        protected override void ParseImplementation(ModelIdentifierBase id, Stream data, IObserver<IOperationState<object>> target, CancellationToken ct)
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

        public IObservable<IOperationState<Stream>> Load(ModelIdentifierBase id)
        {
            return Observable.Generate(0, p => p <= 100, p => ++p,
                p => new OperationState<Stream>(
                    p == 100 
                        ? new MemoryStream(
                            Encoding.UTF8.GetBytes("Article from server"), 
                            false
                        ) 
                        : null, 
                    p
                ),
                p => TimeSpan.FromMilliseconds(50));

            /*
            if (source == ModelSource.Disk) {
                return Observable.Generate(0, p => p <= 100, p => ++p,
                    p => new OperationState<Stream>(p == 100 ? Encoding.UTF8.GetBytes("Article from disk").AsBuffer() : null, p),
                    p => TimeSpan.FromMilliseconds(1));
            } else {
            }*/
        }
    }
}

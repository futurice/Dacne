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
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Threading;

namespace SampleApplication
{
    public class ModelLoader : Futurice.DataAccess.ModelLoader
    {
        protected override void LoadImplementation(ModelIdentifier id, IObserver<IOperationState<IBuffer>> target, CancellationToken ct)
        {
            // Check that this model is supposed to be loaded from the bbc
            new NetworkRequestHander().Get(new Uri("http://feeds.bbci.co.uk/news/rss.xml"), target);
        }

        protected override void ParseImplementation(ModelIdentifier id, IBuffer data, IObserver<IOperationState<object>> target, CancellationToken ct)
        {
            // Use the parser for this model
            new BbcParser().Parse(id, data, target);
        }
        
        public static ModelIdentifier<NewsArticle> GetBbcArticleIdentifier(int item, params string[] sections)
        {
            return BbcParser.CreateItemId(item, sections);
        }

        public static ModelIdentifier<IEnumerable<NewsArticle>> GetBbcArticlesIdentifier()
        {
            return BbcParser.AllArticlesId;
        }
    }

    public class BbcParser : Parser
    {
        protected override void ParseImplementation(ModelIdentifier id, IBuffer data, IObserver<IOperationState<object>> target)
        {
            var progress = 1.0;
            target.OnNextProgress(progress);

            var doc = XDocument.Load(data.AsStream());

            progress = 20;

            // TODO: Try to get existing list from repository
            var list = new List<NewsArticle>();
            target.OnIncompleteResult(list, AllArticlesId, progress, 10);

            var items = doc.Descendants("item").ToList();
            var itemProgress = (100.0 - progress) / (items.Count() + 1);
            foreach (var item in items)
            {
                var link = item.Element("link").Value;
                var article = new NewsArticle
                {
                    Url = new Uri(link),
                    Title = item.Element("title").Value
                };

                var idString = link.Substring(link.LastIndexOf('/') + 1);
                var thisId = new BbcArticleIdentifier(idString);

                progress = thisId.Equals(id) ? 100 : progress + itemProgress;

                target.OnCompleteResult(article, thisId, progress);
                list.Add(article);
            }

            progress += itemProgress;
            target.OnCompleteResult(list, AllArticlesId, progress);

            if (progress < 100)
            {
                target.OnNextProgress(100);
            }

            target.OnCompleted();
        }

        public static BbcArticleIdentifier CreateItemId(int item, params string[] sections)
        {
            var str = String.Join("-", sections.Select(v => v.ToLower())) + "-" + item.ToString();
            return new BbcArticleIdentifier(str);
        }

        public static readonly AllBbcArticlesIdentifier AllArticlesId = new AllBbcArticlesIdentifier();
    }

    public class BbcArticleIdentifier : SimpleModelIdentifier<NewsArticle>
    {
        public BbcArticleIdentifier(string id) : base(id) { }
    }

    public class AllBbcArticlesIdentifier : ModelIdentifier<IEnumerable<NewsArticle>>
    {

    }

    public class BbcDataLoader
    {
        private Uri _baseUri;

        public BbcDataLoader(Uri baseUri)
        {
            _baseUri = baseUri;
        }

        public IObservable<IOperationState<IBuffer>> Load(ModelIdentifier id)
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

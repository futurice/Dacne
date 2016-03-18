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
            // Use the parser for this model
            return new BbcParser().Parse(data, id);
        }
        
        public static ModelIdentifier GetBbcArticleId(int item, params string[] sections)
        {
            return BbcParser.CreateItemId(item, sections);
        }
    }

    public class BbcParser : Parser
    {
        private static readonly Regex ID_FROM_LINK = new Regex("[^/]*(?=#)");

        protected override void ParseImplementation(IBuffer data, ModelIdentifier id, ISubject<OperationState<object>> target)
        {
            var progress = 1.0;
            target.OnNextProgress(progress);

            var doc = XDocument.Load(data.AsStream());

            progress = 20;
            target.OnNextProgress(progress);

            var items = doc.Descendants("item").ToList();
            var itemProgress = (100.0 - progress) / items.Count();  
            foreach (var item in items)
            {
                var link = item.Element("link").Value;
                var article = new NewsArticle   
                {
                    Url = new Uri(link),
                    Title = item.Element("title").Value
                };

                var idString = ID_FROM_LINK.Match(link).Value;
                var thisId = new ModelIdentifier(idString);

                progress = thisId.Equals(id) ? 100 : progress + itemProgress;


                target.OnNextResult(article, thisId, progress);
            }
            
            if (progress < 100)
            {
                target.OnNextProgress(100);
            }
        }

        public static ModelIdentifier CreateItemId(int item, params string[] sections)
        {
            var str = String.Join("-", sections.Select(v => v.ToLower())) + "-" + item.ToString();
            return new ModelIdentifier(str);
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

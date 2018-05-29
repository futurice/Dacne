using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Futurice.DataAccess;
using Windows.Storage.Streams;
using System.Xml.Linq;

namespace SampleApplication
{
    public class BbcParser : Parser
    {
        protected override void ParseImplementation(ModelIdentifierBase id, Stream data, IObserver<IOperationState<object>> target)
        {
            var progress = 1.0;
            target.OnNextProgress(progress);

            var doc = XDocument.Load(data);

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
}

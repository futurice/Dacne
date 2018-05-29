using Dacne.Core;
using System;

namespace SampleApplication
{
    public class NewsArticle : IUpdateableModel<NewsArticle>
    {

        public Uri Url { get; internal set; }
        public string Title { get; set; }

        public NewsArticle CloneForUpdate()
        {
            return new NewsArticle
            {
                Url = Url,
                Title = Title,
            };
        }
    }

    public interface IUpdateableNewsArticle
    {
        string Title { get; set; }
    }
}

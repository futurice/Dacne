using Futurice.DataAccess;
using System;
using System.Collections.Generic;

namespace SampleApplication
{

    // Should these be called ModelProxies -> BbcArticleProxy, or ModelTokens -> BbcArticleToken ?
    public class BbcArticleIdentifier : SimpleModelIdentifier<NewsArticle>
    {
        public BbcArticleIdentifier(string id) : base(id) { }

        public (ModelIdentifierBase id, UpdateEntry update) ChangeTitle(Func<string, string> update, object updateToken = null)
        {
            return (
                this,
                new UpdateEntry(
                    updateToken,
                    old => {
                        var article = (NewsArticle)old;
                        article.Title = update(article.Title);
                        return old;
                    }
                )
            );
        }

        public (ModelIdentifierBase id, UpdateEntry update) SetTitle(string newTitle, object updateToken = null)
        {
            return (
                this,
                new UpdateEntry(
                    updateToken,
                    old => ((NewsArticle)old).Title = newTitle
                )
            );
        }

        //public (ModelIdentifier id, UpdateEntry update) Update(Func<IUpdateableNewsArticle, IUpdateableNewsArticle> update, object updateToken = null)
        //{
        //    return (this, new UpdateEntry(updateToken, old => update((NewsArticle)old)));
        //}
    }

    public class AllBbcArticlesIdentifier : ModelIdentifier<IEnumerable<NewsArticle>>
    {

    }
}

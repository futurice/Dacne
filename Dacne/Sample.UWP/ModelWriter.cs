using System;
using Dacne.Core;
using System.Threading;
using System.Diagnostics;

namespace SampleApplication
{
    public class ModelWriter : Dacne.Core.ModelWriterBase
    {
        protected override void WriteImplementation(ModelIdentifierBase id, UpdateContainer update, ModelSource target, IObserver<IOperationState<object>> operation, CancellationToken ct = default)
        {
            if (id is BbcArticleIdentifier articleId)
            {
                var original = (NewsArticle)update.Original;
                var updated = (NewsArticle)update.Updated;

                if (updated.Title != original.Title)
                {
                    Debug.WriteLine("Title updated from '" + original.Title + "' to '" + updated.Title);
                }

                operation.OnCompleteResult(id, id, 100);
            }
        }
    }
}

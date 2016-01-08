using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Futurice.DataAccess;

namespace SampleApplication
{
    public class ModelRepository : Futurice.DataAccess.ModelRepository
    {
        public ModelRepository(ModelLoader loader) : base(loader) { }

        private readonly Dictionary<ModelIdentifier, NewsArticle> _articles = new Dictionary<ModelIdentifier, NewsArticle>();

        protected override T GetFromMemory<T>(ModelIdentifier id)
        {
            var type = typeof(T);

            if (type == typeof(NewsArticle)) {
                NewsArticle value = null;
                if (_articles.TryGetValue(id, out value)) {
                    return value as T;
                }
            }

            return null;
        }
    }
}

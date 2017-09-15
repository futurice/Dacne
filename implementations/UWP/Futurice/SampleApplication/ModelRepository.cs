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
        public ModelRepository(ModelLoader loader) : base(loader, cache: new SimpleMemoryCache()) { }
    }

    public class MemoryCache : IMemoryCache
    {

        private readonly Dictionary<ModelIdentifier, NewsArticle> _articles = new Dictionary<ModelIdentifier, NewsArticle>();

        public T Get<T>(ModelIdentifier id) where T : class
        {
            var type = typeof(T);

            if (type == typeof(NewsArticle))
            {
                NewsArticle value = null;
                if (_articles.TryGetValue(id, out value))
                {
                    return value as T;
                }
            }

            return null;
        }

        public void Set<T>(ModelIdentifier id, T model) where T : class
        {
            //_articles[id] = model;
        }
    }
}

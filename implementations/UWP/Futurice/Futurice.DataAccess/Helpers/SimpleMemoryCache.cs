using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Futurice.DataAccess
{
    public class SimpleMemoryCache : IMemoryCache
    {
        private readonly ConcurrentDictionary<ModelIdentifierBase, object> _cache = new ConcurrentDictionary<ModelIdentifierBase, object>();

        public T Get<T>(ModelIdentifierBase id) where T : class
        {
            object item = null;
            if (_cache.TryGetValue(id, out item))
            {
                return (T)item;
            }

            return null;
        }

        public void Set<T>(ModelIdentifierBase id, T model) where T : class
        {
            _cache.AddOrUpdate(id, model, (_, __) => model);
        }
    }
}

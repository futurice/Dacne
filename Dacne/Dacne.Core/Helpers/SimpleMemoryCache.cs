﻿using System.Collections.Concurrent;

namespace Dacne.Core
{
    public class SimpleMemoryCache : IMemoryCache
    {
        private readonly ConcurrentDictionary<ModelIdentifierBase, object> _cache = new ConcurrentDictionary<ModelIdentifierBase, object>();

        public T Get<T>(ModelIdentifierBase id) where T : class
        {
            if (_cache.TryGetValue(id, out object item))
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

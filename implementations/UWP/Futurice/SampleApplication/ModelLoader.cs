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

namespace SampleApplication
{
    public class ModelLoader : Futurice.DataAccess.ModelLoader
    {

        protected override IObservable<OperationState<IBuffer>> LoadImplementation(ModelIdentifier id, SourcePreference source)
        {
            // Check that this model is supposed to be loaded from the bbc
            if (source == SourcePreference.Cache || source == SourcePreference.CacheWithServerFallback) {                
                return Observable.Generate(0, p => p <= 100, p => ++p,
                    p => new OperationState<IBuffer>(p == 100 ? Encoding.UTF8.GetBytes("Article from disk").AsBuffer() : null, p),
                    p => TimeSpan.FromMilliseconds(1));
            } else {
                return Observable.Generate(0, p => p <= 100, p => ++p,
                    p => new OperationState<IBuffer>(p == 100 ? Encoding.UTF8.GetBytes("Article from server").AsBuffer() : null, p),
                    p => TimeSpan.FromMilliseconds(50));
            }
        }

        protected override IObservable<OperationState<object>> ParseImplementation(ModelIdentifier id, IBuffer data)
        {
            // Check that this model is bbc data
            return Observable.Return(
                new OperationState<object>(
                    new NewsArticle() { Title = Encoding.UTF8.GetString(data.ToArray()) }, 100)
                );
        }
    }

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

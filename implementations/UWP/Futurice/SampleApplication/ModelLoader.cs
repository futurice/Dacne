using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Futurice.DataAccess;
using Windows.Storage.Streams;

namespace SampleApplication
{
    public class ModelLoader : Futurice.DataAccess.ModelLoader
    {

        protected override IBuffer LoadImplementation(ModelIdentifier id)
        {
            // Check that this model is supposed to be loaded from the bbc
            return new Windows.Storage.Streams.Buffer(1000);
        }

        protected override IObservable<OperationState<object>> ParseImplementation(ModelIdentifier id, IBuffer data)
        {
            // Check that this model is bbc data
            return Observable.Return(
                new OperationState<object>(
                    new NewsArticle() { Title = "Test article title" }, 1)
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

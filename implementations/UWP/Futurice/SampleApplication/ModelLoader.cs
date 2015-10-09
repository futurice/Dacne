using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using Futurice.DataAccess;
using Windows.Storage.Streams;

namespace SampleApplication
{
    class ModelLoader : Futurice.DataAccess.ModelLoader
    {

        private DataLoader _bbcLoader = new BbcDataLoader();
        private IParser _bbcParser = new BbcDataParser();

        protected override DataLoader PickLoader(ModelIdentifier id)
        {
            // Check that this model is supposed to be loaded from the bbc
            return _bbcLoader;
        }

        protected override IParser PickParser(ModelIdentifier id)
        {
            // Check that this model is bbc data
            return _bbcParser;
        }
    }

    class BbcDataLoader : DataLoader
    {
        public override IBuffer Load(ModelIdentifier id)
        {
            return new Windows.Storage.Streams.Buffer(1000);
        }
    }

    class BbcDataParser : IParser
    {
        public ModelsParseOperation Parse(IBuffer stream)
        {
            return new ModelsParseOperation(() => new BehaviorSubject<OperationState<object>>(new OperationState<object>(new NewsArticle() { Title = "Test article title" }, 1)));
        }
    }
}

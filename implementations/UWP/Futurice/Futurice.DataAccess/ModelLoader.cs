using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public abstract class ModelLoader
    {

        protected abstract DataLoader PickLoader(ModelIdentifier id);

        protected abstract IParser PickParser(ModelIdentifier id);

        public ModelsLoadOperation Load(ModelIdentifier id)
        {
            return new ModelsLoadOperation(() => {
                var loader = PickLoader(id);
                var stream = loader.Load(id);
                var parser = PickParser(id);

                return parser.Parse(stream).Begin();
            });            
        }
    }
}

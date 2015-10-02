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

        protected abstract DataLoader PickLoader(ModelId id);

        protected abstract IParser PickParser(ModelId id);

        public ModelsLoadOperation Load(ModelId id)
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

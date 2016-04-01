using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public class NetworkDataLoader : DataLoader
    {
        private readonly Func<ModelIdentifier, Uri> getUriFunc;

        public NetworkDataLoader(Func<ModelIdentifier, Uri> getUriFunc)
        {
            this.getUriFunc = getUriFunc;
        }

        public IObservable<IOperationState<IBuffer>> Load(ModelIdentifier id)
        {
            var uri = getUriFunc(id);
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public abstract class DataLoader
    {
        public DataLoader(Uri baseUri)
        {

        }

        protected abstract IObservable<OperationState<IBuffer>> LoadImplementation(ModelIdentifier id, ModelSource source);

    }

}

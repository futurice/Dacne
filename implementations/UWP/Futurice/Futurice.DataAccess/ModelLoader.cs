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

        protected abstract IBuffer LoadImplementation(ModelIdentifier id);

        protected abstract IObservable<OperationState<object>> ParseImplementation(ModelIdentifier id, IBuffer data);

        public IObservable<OperationState<object>> Load(ModelIdentifier id)
        {            
            var data = LoadImplementation(id);
            return ParseImplementation(id, data);                        
        }
    }


    public delegate IBuffer LoadFunction(ModelIdentifier id);
    public delegate ModelsParseOperation ParseFunction(IBuffer stream);

}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public abstract class ModelLoader
    {

        protected abstract IObservable<OperationState<IBuffer>> LoadImplementation(ModelIdentifier id);

        protected abstract IObservable<OperationState<object>> ParseImplementation(ModelIdentifier id, IBuffer data);

        public IObservable<OperationState<object>> Load(ModelIdentifier id)
        {
            var loadStates = LoadImplementation(id);

            return Observable.Merge(
                loadStates
                    .WhereProgressChanged()
                    .Select(loadState => new OperationState<object>(null, loadState.Progress, loadState.Error, loadState.IsCancelled)),

                loadStates
                    .WhereResultChanged()
                    .SelectMany(state => ParseImplementation(id, state.Result))
            );
                   
        }
    }


    public delegate IBuffer LoadFunction(ModelIdentifier id);
    public delegate ModelsParseOperation ParseFunction(IBuffer stream);

}

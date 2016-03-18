using System;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public class DataContractXmlParser : Parser
    {
        private readonly Type type;

        public DataContractXmlParser(Type type)
        {
            this.type = type;
        }

        public override IObservable<OperationState<object>> Parse(IBuffer data)
        {
            var subject = new Subject<OperationState<object>>();

            Task.Run(() =>
            {
                try
                {
                    var parser = new DataContractSerializer(type);
                    var obj = parser.ReadObject(data.AsStream());
                    subject.OnNext(new OperationState<object>(obj, 100, null, false, ModelSource.Server));
                    subject.OnCompleted();
                }
            });

            return subject;
        }
    }
}

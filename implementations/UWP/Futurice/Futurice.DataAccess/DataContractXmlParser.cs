using System;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public class DataContractXmlParser : SimpleParser
    {
        private readonly Type type;

        public DataContractXmlParser(Type type)
        {
            this.type = type;
        }

        protected override object ParseImplementation(IBuffer data, ModelIdentifier id)
        {
            using (var reader = XmlReader.Create(data.AsStream(), new XmlReaderSettings { IgnoreProcessingInstructions = true, DtdProcessing = DtdProcessing.Ignore }))
            {
                var parser = new DataContractSerializer(type);
                var obj = parser.ReadObject(reader);
                return obj;
            }
        }
    }
}

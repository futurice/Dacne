using System;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

namespace Futurice.DataAccess
{
    public class DataContractXmlParser : SimpleParser
    {
        private readonly Type type;

        public DataContractXmlParser(Type type)
        {
            this.type = type;
        }

        protected override object ParseImplementation(ModelIdentifierBase id, Stream data)
        {
            using (var reader = XmlReader.Create(data, new XmlReaderSettings { IgnoreProcessingInstructions = true, DtdProcessing = DtdProcessing.Ignore }))
            {
                var parser = new DataContractSerializer(type);
                var obj = parser.ReadObject(reader);
                return obj;
            }
        }
    }
}

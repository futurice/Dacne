
using System.IO;

namespace Dacne.Core
{
    public interface ICache : IDataLoader
    {
        void Save(ModelIdentifierBase id, Stream data);
    }
}

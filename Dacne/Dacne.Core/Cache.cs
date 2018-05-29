
using System.IO;

namespace Futurice.DataAccess
{
    public interface ICache : IDataLoader
    {
        void Save(ModelIdentifierBase id, Stream data);
    }
}

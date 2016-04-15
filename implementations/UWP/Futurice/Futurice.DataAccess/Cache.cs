using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public interface ICache : IDataLoader
    {
        void Save(ModelIdentifier id, IBuffer data);
    }
}

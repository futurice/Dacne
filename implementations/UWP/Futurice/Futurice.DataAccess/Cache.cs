using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public interface ICache : IDataLoader
    {
        void Save(ModelIdentifierBase id, IBuffer data);
    }
}

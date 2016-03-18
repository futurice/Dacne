using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public interface Cache : DataLoader
    {
        void Save(ModelIdentifier id, IBuffer data);
    }
}

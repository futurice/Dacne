using System;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public abstract class DataLoader
    {
        public abstract IBuffer Load(ModelId id);
    }
}
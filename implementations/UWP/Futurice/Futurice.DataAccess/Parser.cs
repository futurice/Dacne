using System;
using Windows.Storage.Streams;

namespace Futurice.DataAccess
{
    public interface IParser
    {
        ModelsParseOperation Parse(IBuffer stream);
    }
}
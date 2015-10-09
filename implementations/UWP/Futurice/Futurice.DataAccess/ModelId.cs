namespace Futurice.DataAccess
{
    public class ModelIdentifier
    {
        public ModelIdentifier(string id)
        {
            Id = id;
        }

        public string Id { get; private set; }

        public int Completness { get; }
    }
}
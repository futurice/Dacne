namespace Futurice.DataAccess
{

    public interface IUpdateableModel<T>
    {
        T CloneForUpdate();
    }

}

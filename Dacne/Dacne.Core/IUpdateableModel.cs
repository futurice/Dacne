namespace Dacne.Core
{

    public interface IUpdateableModel<T>
    {
        T CloneForUpdate();
    }

}

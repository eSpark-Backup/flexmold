namespace FlexMold.StartupHelper
{
    public interface IAbstarctFactory<T>
    {
        T Create();
    }
}
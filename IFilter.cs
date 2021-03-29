namespace NETType
{
    public interface IFilter<T>
    {
        T Filter(string filter);
    }
}
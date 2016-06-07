namespace Gribble
{
    public interface ITableNamingConvention
    {
        string GetName<T>();
    }

    public class DefaultTableNamingConvention : ITableNamingConvention
    {
        public string GetName<T>()
        {
            return typeof(T).Name;
        }
    }
}

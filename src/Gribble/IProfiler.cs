namespace Gribble
{
    public interface IProfiler
    {
        void Write(string format, params object[] args);
    }
}

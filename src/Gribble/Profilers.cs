using System;

namespace Gribble
{
    public interface IProfiler
    {
        void Write(string format, params object[] args);
    }

    public class ConsoleProfiler : IProfiler
    {
        public void Write(string format, params object[] args)
        {
            Console.WriteLine("Gribble ({0:hh:mm:ss.fffffff}): {1}", DateTime.Now, string.Format(format, args));
        }
    }

    public class NullProfiler : IProfiler
    {
        public void Write(string format, params object[] args) { }
    }
}

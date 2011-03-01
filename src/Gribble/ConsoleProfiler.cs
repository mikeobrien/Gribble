using System;

namespace Gribble
{
    public class ConsoleProfiler : IProfiler
    {
        public void Write(string format, params object[] args)
        {
            Console.WriteLine("DHibernate ({0:hh:mm:ss.fffffff}): {1}", DateTime.Now, string.Format(format, args));
        }
    }
}

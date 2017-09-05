using System.Threading;

namespace Gribble.Extensions
{
    public static class Unique
    {
        private static long _index;

        public static long Next()
        {
            return Interlocked.Increment(ref _index);
        }
    }
}

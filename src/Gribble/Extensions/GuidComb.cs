using System;
using System.Runtime.InteropServices;

namespace Gribble.Extensions
{
    public static class GuidComb
    {
        [DllImport("rpcrt4.dll", SetLastError = true)]
        private static extern int UuidCreateSequential(out Guid guid);

        public static Guid Create()
        {
            Guid guid;
            var result = UuidCreateSequential(out guid);
            if (result == 0) return guid;
            throw new Exception($"Error generating guid ({result}).");
        }
    }
}

using System;
using Gribble.Extensions;

namespace Gribble.Model
{
    public class Constant
    {
        public string Alias = string.Format("C{0}", Unique.Next());
        public object Value;
    }
}

using System;
using Gribble.Extensions;

namespace Gribble.Model
{
    public class SelectProjection
    {
        public Projection Projection;
        public string Alias = string.Format("@F{0}", Unique.Next());

        public static SelectProjection Create(Projection projection)
        {
            return new SelectProjection {Projection = projection};
        }
    }
}

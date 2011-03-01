using System;

namespace Gribble.Statements
{
    public class SelectProjection
    {
        private static readonly Random Random = new Random();

        public Projection Projection;
        public string Alias = string.Format("@F{0}", Random.Next());

        public static SelectProjection Create(Projection projection)
        {
            return new SelectProjection {Projection = projection};
        }
    }
}

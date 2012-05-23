using System.Collections.Generic;

namespace Gribble.Model
{
    public class Duplicates
    {
        public Duplicates()
        {
            OrderBy = new List<OrderBy>();
        }

        public Projection Distinct;
        public List<OrderBy> OrderBy;
    }
}
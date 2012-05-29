using System.Collections.Generic;

namespace Gribble.Model
{
    public class Sync
    {
        public Projection SourceKey;
        public Projection TargetKey;
        public List<Field> ExcludedFields;
        public Select Source;
        public Select Target;
    }
}
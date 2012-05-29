using System;
using Gribble.Mapping;

namespace Gribble.Model
{
    public class Field
    {
        private static readonly Random Random = new Random();

        public string Alias = string.Format("F{0}", Random.Next());
        public string Name;
        public bool HasKey;
        public string Key;
        public string TableAlias;
        public bool HasTableAlias { get { return !string.IsNullOrEmpty(TableAlias); } }

        public string Map(IEntityMapping mapping)
        {
            return HasKey ? mapping.DynamicProperty.GetColumnName(Key) : mapping.StaticProperty.GetColumnName(Name);
        }
    }
}

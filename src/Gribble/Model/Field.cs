using System;
using Gribble.Extensions;
using Gribble.Mapping;

namespace Gribble.Model
{
    public class Field
    {
        public string Alias = $"F{Unique.Next()}";
        public string Name;
        public bool HasKey;
        public string Key;
        public string TableAlias;
        public bool HasTableAlias => !string.IsNullOrEmpty(TableAlias);

        public string Map(IEntityMapping mapping)
        {
            return HasKey 
                ? mapping.DynamicProperty.GetColumnName(Key) 
                : mapping.StaticProperty.GetColumnName(Name);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Gribble.Mapping
{
    public interface IClassMap
    {
        Type Type { get; }
        KeyPropertyMapping KeyProperty { get; set; }
        DynamicMap DynamicProperty { get; }
        List<PropertyMapping> Properties { get; }
    }
}

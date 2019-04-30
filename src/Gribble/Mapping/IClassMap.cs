using System;
using System.Collections.Generic;
using System.Reflection;

namespace Gribble.Mapping
{
    public interface IClassMap
    {
        IDictionary<string, string> PropertyColumMapping { get; }
        IDictionary<string, PropertyInfo> ColumPropertyMapping { get; }
        IDictionary<string, PropertyInfo> PropertyNameMapping { get; }
        List<PropertyInfo> Properties { get; }

        PropertyInfo DynamicProperty { get; }
        bool HasDynamicProperty { get; }

        PrimaryKeyType KeyType { get; }
        PrimaryKeyGeneration KeyGeneration { get; }
        string KeyColumn { get; }
        PropertyInfo KeyProperty { get; }

        Type Type { get; }
    }
}

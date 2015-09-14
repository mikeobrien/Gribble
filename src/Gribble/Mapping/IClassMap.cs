using System;
using System.Collections.Generic;

namespace Gribble.Mapping
{
    public interface IClassMap
    {
        IDictionary<string, string> PropertyColumMapping { get; }
        IDictionary<string, string> ColumPropertyMapping { get; }

        string DynamicProperty { get; }
        bool HasDynamicProperty { get; }

        PrimaryKeyType KeyType { get; }
        PrimaryKeyGeneration KeyGeneration { get; }
        string KeyColumn { get; }
        string KeyProperty { get; }

        Type Type { get; }
    }
}

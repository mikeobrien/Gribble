using System.Collections.Generic;
using System.Reflection;

namespace Gribble.Mapping
{
    public interface IEntityMapping
    {
        IEntityKey Key { get; }
        IDynamicPropertyMap DynamicProperty { get; }
        IStaticPropertyMap StaticProperty { get; }
        IColumnMap Column { get; }
    }

    public interface IEntityKey
    {
        bool HasKey { get; }
        PrimaryKeyType KeyType { get; }
        PrimaryKeyGeneration KeyGeneration { get; }
        string ColumnName { get; }
        PropertyInfo Property { get; }
    }

    public interface IDynamicPropertyMap
    {
        bool HasProperty { get; }
        PropertyInfo Property { get; }
        bool HasColumnMapping(string propertyName);
        bool IsReadonly(string propertyName);
        string GetColumnName(string propertyName);
    }

    public interface IStaticPropertyMap
    {
        bool HasColumnMapping(string propertyName);
        string GetColumnName(string propertyName);
        PropertyInfo GetProperty(string propertyName);
        List<PropertyMapping> Mapping { get; }
        Dictionary<string, PropertyInfo> StaticDynamicMapping { get; }
    }

    public interface IColumnMap
    {
        bool HasMapping(string columnName);
        string GetName(string columnName);
        bool HasStaticPropertyMapping(string columnName);
        PropertyInfo GetStaticProperty(string columnName);
        PropertyInfo TryGetStaticProperty(string columnName);
        bool HasDynamicMapping(string columnName);
        string GetDynamicName(string columnName);
    }
}

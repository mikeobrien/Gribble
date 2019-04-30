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
        PrimaryKeyType KeyType { get; }
        PrimaryKeyGeneration KeyGeneration { get; }
        string GetColumnName();
        PropertyInfo GetProperty();
    }

    public interface IDynamicPropertyMap
    {
        bool HasProperty();
        PropertyInfo GetProperty();
        bool HasColumnMapping(string propertyName);
        string GetColumnName(string propertyName);
    }

    public interface IStaticPropertyMap
    {
        bool HasColumnMapping(string propertyName);
        string GetColumnName(string propertyName);
        PropertyInfo GetProperty(string propertyName);
        List<PropertyInfo> Properties { get; }
        Dictionary<string, PropertyInfo> StaticDynamicMapping { get; }
    }

    public interface IColumnMap
    {
        bool HasPropertyMapping(string columnName);
        string GetPropertyName(string columnName);
        bool HasStaticPropertyMapping(string columnName);
        PropertyInfo GetStaticProperty(string columnName);
        PropertyInfo TryGetStaticProperty(string columnName);
        bool HasDynamicPropertyMapping(string columnName);
        string GetDynamicPropertyName(string columnName);
    }
}

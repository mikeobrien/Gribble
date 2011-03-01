using System;

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
        Guid GenerateGuidKey();
        string GetColumnName();
        string GetPropertyName();
    }

    public interface IDynamicPropertyMap
    {
        bool HasProperty();
        string GetPropertyName();
        bool HasColumnMapping(string propertyName);
        string GetColumnName(string propertyName);
    }

    public interface IStaticPropertyMap
    {
        bool HasColumnMapping(string propertyName);
        string GetColumnName(string propertyName);
    }

    public interface IColumnMap
    {
        bool HasPropertyMapping(string columnName);
        string GetPropertyName(string columnName);
        bool HasStaticPropertyMapping(string columnName);
        string GetStaticPropertyName(string columnName);
        bool HasDynamicPropertyMapping(string columnName);
        string GetDynamicPropertyName(string columnName);
    }
}

﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Gribble.Extensions;
using Gribble.TransactSql;

namespace Gribble.Model
{
    public class Column
    {
        public enum KeyType { None, PrimaryKey, ClusteredPrimaryKey }

        public Column(
            string name, 
            Type type = null,
            SqlDbType? sqlType = null,
            KeyType key = KeyType.None,
            bool isIdentity = false, 
            short length = 0, 
            byte? precision = null,
            byte? scale = null,
            bool isNullable = false, 
            bool isAutoGenerated = false,
            object defaultValue = null,
            string computation = null,
            bool? computationPersisted = null)
        {
            Name = name;
            if (type == null && sqlType == null && string.IsNullOrEmpty(computation)) 
                throw new Exception("No column data type or computation specified.");
            Type = type ?? (sqlType.HasValue ? sqlType.Value.GetClrType(isNullable) : null);
            SqlType = sqlType ?? (type != null ? type.GetSqlType() : (SqlDbType?)null);
            Key = key;
            IsIdentity = isIdentity;
            Length = length;
            Precision = precision;
            Scale = scale;
            IsAutoGenerated = isAutoGenerated;
            IsNullable = isNullable;
            DefaultValue = defaultValue;
            Computation = computation;
            ComputationPersisted = computationPersisted;
        }

        public class OptionBuilder
        {
            private readonly Column _column;
            public OptionBuilder(Column column) { _column = column; }
            public Column Create() { return _column; }
            public OptionBuilder Length(short length) { _column.Length = length; return this; }
            public OptionBuilder Computation(string statement, bool? persisted = false) { _column.Computation = statement;
                _column.ComputationPersisted = persisted; return this; }
            public OptionBuilder Precision(byte precision) { _column.Precision = precision; return this; }
            public OptionBuilder Scale(byte scale) { _column.Scale = scale; return this; }
            public OptionBuilder Nullable() { _column.IsNullable = true; return this; }
            public OptionBuilder Identity() { _column.IsIdentity = true; return this; }
            public OptionBuilder AutoGenerated() { _column.IsAutoGenerated = true; return this; }
            public OptionBuilder Key(KeyType key) { _column.Key = key; return this; }
            public static implicit operator Column(OptionBuilder builder) { return builder._column; }
        }

        public static OptionBuilder Create(string name, SqlDbType type)
        {
            return new OptionBuilder(new Column(name, sqlType: type));
        }

        public static OptionBuilder Create(string name, SqlDbType type, object defaultValue)
        {
            return new OptionBuilder(new Column(name, sqlType: type, defaultValue: defaultValue));
        }

        public static OptionBuilder Create<T>(string name)
        {
            return new OptionBuilder(new Column(name, typeof(T)));
        }

        public static OptionBuilder Create<T>(string name, T defaultValue)
        {
            return new OptionBuilder(new Column(name, typeof (T), defaultValue: defaultValue));
        }

        public static OptionBuilder Create(string name, Type type)
        {
            return new OptionBuilder(new Column(name, type));
        }

        public static OptionBuilder Create(string name, Type type, object defaultValue)
        {
            return new OptionBuilder(new Column(name, type, defaultValue: defaultValue));
        }

        public string Name { get;  }
        public Type Type { get; }
        public SqlDbType? SqlType { get; }
        public bool IsIdentity { get; private set; }
        public KeyType Key { get; private set; }
        public short Length { get; private set; }
        public byte? Precision { get; private set; }
        public byte? Scale { get; private set; }
        public bool IsNullable { get; private set; }
        public bool IsComputed => !string.IsNullOrEmpty(Computation);
        public string Computation { get; private set; }
        public bool? ComputationPersisted { get; private set; }
        public bool IsAutoGenerated { get; private set; }
        public object DefaultValue { get; }

        public bool IsEquivalent(Column column)
        {
            return SqlType == column.SqlType &&
                IsIdentity == column.IsIdentity &&
                Key == column.Key &&
                Length == column.Length && 
                Precision == column.Precision &&
                Scale == column.Scale &&
                IsNullable == column.IsNullable &&
                IsComputed == column.IsComputed &&
                Computation == column.Computation &&
                ComputationPersisted == column.ComputationPersisted &&
                IsAutoGenerated == column.IsAutoGenerated &&
                (DefaultValue ?? "").ToString() == (column.DefaultValue ?? "").ToString();
        }
        
        public override bool Equals(object obj)
        {
            return obj is Column compare && compare.Name.Equals(Name, 
                StringComparison.OrdinalIgnoreCase) && compare.SqlType == SqlType;
        }

        public override int GetHashCode()
        {
            return (Name ?? "").GetHashCode() | (SqlType ?? 0).GetHashCode();
        }

        public override string ToString()
        {
            return $"Name: {Name}, Sql Type: {SqlType}, Clr Type: {Type}, " +
                   $"Identity: {IsIdentity}, Key: {Key}, Length: {Length}, " +
                   $"Precision: {Precision}, Scale: {Scale}, Nullable: {IsNullable}, " +
                   $"Computed: {IsComputed}, Computation: {Computation}, " +
                   $"Computation Persisted: {ComputationPersisted}, " +
                   $"Auto Generated: {IsAutoGenerated}, Default Value: {DefaultValue}";
        }
    }

    public static class ColumnExtensions
    {
        public static Column GetColumn(this IEnumerable<Column> columns, string name)
        {
            return columns.FirstOrDefault(x => x.Name.EqualsIgnoreCase(name));
        }
    }
}

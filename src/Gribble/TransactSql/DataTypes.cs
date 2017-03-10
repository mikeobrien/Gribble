using System;
using System.Data;
using System.Linq;
using Gribble.Extensions;

namespace Gribble.TransactSql
{
    public static class DataTypes
    {
        public class SqlDataType
        {
            public SqlDataType(string sqlName, byte sqlId, SqlDbType sqlType, Type clrType, Type clrNullableType)
            {
                SqlName = sqlName;
                SqlId = sqlId;
                SqlType = sqlType;
                ClrType = clrType;
                ClrNullableType = clrNullableType;
            }
            public string SqlName { get; }
            public byte SqlId { get; }
            public SqlDbType SqlType { get; }
            public Type ClrType { get; }
            public Type ClrNullableType { get; }
        }

        public static readonly SqlDataType BigInt = new SqlDataType("bigint", 127, SqlDbType.BigInt, typeof(long), typeof(long?));
        public static readonly SqlDataType Binary = new SqlDataType("binary", 173, SqlDbType.Binary, typeof(byte[]), typeof(byte[]));
        public static readonly SqlDataType Bit = new SqlDataType("bit", 104, SqlDbType.Bit, typeof(bool), typeof(bool?));
        public static readonly SqlDataType Char = new SqlDataType("char", 175, SqlDbType.Char, typeof(string), typeof(string));
        public static readonly SqlDataType Date = new SqlDataType("date", 40, SqlDbType.Date, typeof(DateTime), typeof(DateTime?));
        public static readonly SqlDataType Datetime = new SqlDataType("datetime", 61, SqlDbType.DateTime, typeof(DateTime), typeof(DateTime?));
        public static readonly SqlDataType Datetime2 = new SqlDataType("datetime2", 42, SqlDbType.DateTime2, typeof(DateTime), typeof(DateTime?));
        public static readonly SqlDataType DatetimeOffset = new SqlDataType("datetimeoffset", 43, SqlDbType.DateTimeOffset, typeof(DateTimeOffset), typeof(DateTimeOffset?));
        public static readonly SqlDataType Decimal = new SqlDataType("decimal", 106, SqlDbType.Decimal, typeof(decimal), typeof(decimal?));
        public static readonly SqlDataType Float = new SqlDataType("float", 62, SqlDbType.Float, typeof(double), typeof(double?));
        public static readonly SqlDataType Int = new SqlDataType("int", 56, SqlDbType.Int, typeof(int), typeof(int?));
        public static readonly SqlDataType Money = new SqlDataType("money", 60, SqlDbType.Money, typeof(decimal), typeof(decimal?));
        public static readonly SqlDataType NChar = new SqlDataType("nchar", 239, SqlDbType.NChar, typeof(string), typeof(string));
        public static readonly SqlDataType Numeric = new SqlDataType("numeric", 108, SqlDbType.Decimal, typeof(decimal), typeof(decimal?));
        public static readonly SqlDataType NVarChar = new SqlDataType("nvarchar", 231, SqlDbType.NVarChar, typeof(string), typeof(string));
        public static readonly SqlDataType NText = new SqlDataType("ntext", 99, SqlDbType.NText, typeof(string), typeof(string));
        public static readonly SqlDataType Real = new SqlDataType("real", 59, SqlDbType.Real, typeof(float), typeof(float?));
        public static readonly SqlDataType SmallDatetime = new SqlDataType("smalldatetime", 58, SqlDbType.SmallDateTime, typeof(DateTime), typeof(DateTime?));
        public static readonly SqlDataType SmallInt = new SqlDataType("smallint", 52, SqlDbType.SmallInt, typeof(short), typeof(short?));
        public static readonly SqlDataType SmallMoney = new SqlDataType("smallmoney", 122, SqlDbType.SmallMoney, typeof(decimal), typeof(decimal?));
        public static readonly SqlDataType Text = new SqlDataType("text", 35, SqlDbType.Text, typeof(string), typeof(string));
        public static readonly SqlDataType Time = new SqlDataType("time", 41, SqlDbType.Time, typeof(TimeSpan), typeof(TimeSpan?));
        public static readonly SqlDataType Timestamp = new SqlDataType("timestamp", 189, SqlDbType.Timestamp, typeof(DateTime), typeof(DateTime?));
        public static readonly SqlDataType TinyInt = new SqlDataType("tinyint", 48, SqlDbType.TinyInt, typeof(byte), typeof(byte?));
        public static readonly SqlDataType Uniqueidentifier = new SqlDataType("uniqueidentifier", 36, SqlDbType.UniqueIdentifier, typeof(Guid), typeof(Guid?));
        public static readonly SqlDataType VarBinary = new SqlDataType("varbinary", 165, SqlDbType.VarBinary, typeof(byte[]), typeof(byte[]));
        public static readonly SqlDataType VarChar = new SqlDataType("varchar", 167, SqlDbType.VarChar, typeof(string), typeof(string));
        public static readonly SqlDataType Variant = new SqlDataType("sql_variant", 98, SqlDbType.Variant, typeof(object), typeof(object));

        public static readonly SqlDataType[] TypesWithLength = new[] { Char, VarChar, NChar, NVarChar, Binary, VarBinary };
        public static readonly SqlDataType[] TypesWithScaleAndPrecision = new[] { Decimal, Numeric };

        // All mappings
        private static readonly SqlDataType[] SqlTypes = new [] {
            BigInt, Binary, Bit, Char, Date, Datetime, Datetime2, DatetimeOffset, Decimal, Float, Int,
            Money, NChar, Numeric, NVarChar, NText, Real, SmallDatetime, SmallInt, SmallMoney,
            Text, Time, Timestamp, TinyInt, Uniqueidentifier, VarBinary, VarChar, Variant };

        public static Type GetClrType(this byte sqlId, bool nullable)
        {
            var type = SqlTypes.FirstOrDefault(x => x.SqlId == sqlId);
            if (type != null) return nullable ? type.ClrNullableType : type.ClrType;
            throw new Exception($"No CLR data type found to match SQL data type id '{sqlId}'.");
        }

        public static Type GetClrType(this string sqlName, bool nullable)
        {
            var type = SqlTypes.FirstOrDefault(x => x.SqlName == sqlName);
            if (type != null) return nullable ? type.ClrNullableType : type.ClrType;
            throw new Exception($"No CLR data type found to match SQL data type '{sqlName}'.");
        }

        public static SqlDbType GetSqlType(this byte sqlId)
        {
            var type = SqlTypes.FirstOrDefault(x => x.SqlId == sqlId);
            if (type != null) return type.SqlType;
            throw new Exception($"No SQL data type found to match data type id '{sqlId}'.");
        }

        // These are the distinct clr to sql type mapping
        private static readonly SqlDataType[] ClrTypeMapping = new[] {
            BigInt, Bit, Datetime, DatetimeOffset, Decimal, Float, Int, NVarChar, 
            Real, SmallInt, Time, TinyInt, Uniqueidentifier, VarBinary, Variant};

        public static SqlDbType GetSqlType(this Type clrType)
        {
            return clrType.GetSqlDataType().SqlType;
        }

        public static string GetSqlTypeName(this Type clrType)
        {
            return clrType.GetSqlDataType().SqlName;
        }

        private static SqlDataType GetSqlDataType(this Type clrType)
        {
            var underlyingType = clrType.GetUnderlyingType();
            var type = ClrTypeMapping.FirstOrDefault(x => 
                x.ClrType.IsSameTypeAs(underlyingType));
            if (type != null) return type;
            throw new Exception($"No SQL data type found to match CLR data type '{underlyingType.FullName}'.");
        }

        // These are distinct sql type mappings
        private static readonly SqlDataType[] SqlTypeMapping = new[] {
            BigInt, Binary, Bit, Char, Date, Datetime, Datetime2, DatetimeOffset, Decimal, Float, Int,
            Money, NChar, NVarChar, NText, Real, SmallDatetime, SmallInt, SmallMoney,
            Text, Time, Timestamp, TinyInt, Uniqueidentifier, VarBinary, VarChar, Variant };

        public static Type GetClrType(this SqlDbType sqlType, bool nullable)
        {
            var type = SqlTypeMapping.FirstOrDefault(x => x.SqlType == sqlType);
            if (type != null) return nullable ? type.ClrNullableType : type.ClrType;
            throw new Exception($"No CLR data type found to match SQL data type '{sqlType}'.");
        }

        public static string GetSqlTypeName(this SqlDbType sqlType)
        {
            var type = SqlTypeMapping.FirstOrDefault(x => x.SqlType == sqlType);
            if (type != null) return type.SqlName;
            throw new Exception($"No data type found to match SQL data type '{sqlType}'.");
        }

        public static byte GetSqlTypeId(this SqlDbType sqlType)
        {
            var type = SqlTypeMapping.FirstOrDefault(x => x.SqlType == sqlType);
            if (type != null) return type.SqlId;
            throw new Exception($"No data type found to match SQL data type '{sqlType}'.");
        }
    }
}

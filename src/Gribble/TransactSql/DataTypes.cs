using System;
using System.Data;
using System.Linq;

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
            public string SqlName { get; private set; }
            public byte SqlId { get; private set; }
            public SqlDbType SqlType { get; private set; }
            public Type ClrType { get; private set; }
            public Type ClrNullableType { get; private set; }
        }

        public readonly static SqlDataType BigInt = new SqlDataType("bigint", 127, SqlDbType.BigInt, typeof(long), typeof(long?));
        public readonly static SqlDataType Binary = new SqlDataType("binary", 173, SqlDbType.Binary, typeof(byte[]), typeof(byte[]));
        public readonly static SqlDataType Bit = new SqlDataType("bit", 104, SqlDbType.Bit, typeof(bool), typeof(bool?));
        public readonly static SqlDataType Char = new SqlDataType("char", 175, SqlDbType.Char, typeof(string), typeof(string));
        public readonly static SqlDataType Date = new SqlDataType("date", 40, SqlDbType.Date, typeof(DateTime), typeof(DateTime?));
        public readonly static SqlDataType Datetime = new SqlDataType("datetime", 61, SqlDbType.DateTime, typeof(DateTime), typeof(DateTime?));
        public readonly static SqlDataType Datetime2 = new SqlDataType("datetime2", 42, SqlDbType.DateTime2, typeof(DateTime), typeof(DateTime?));
        public readonly static SqlDataType DatetimeOffset = new SqlDataType("datetimeoffset", 43, SqlDbType.DateTimeOffset, typeof(DateTimeOffset), typeof(DateTimeOffset?));
        public readonly static SqlDataType Decimal = new SqlDataType("decimal", 106, SqlDbType.Decimal, typeof(decimal), typeof(decimal?));
        public readonly static SqlDataType Float = new SqlDataType("float", 62, SqlDbType.Float, typeof(double), typeof(double?));
        public readonly static SqlDataType Int = new SqlDataType("int", 56, SqlDbType.Int, typeof(int), typeof(int?));
        public readonly static SqlDataType Money = new SqlDataType("money", 60, SqlDbType.Money, typeof(decimal), typeof(decimal?));
        public readonly static SqlDataType NChar = new SqlDataType("nchar", 239, SqlDbType.NChar, typeof(string), typeof(string));
        public readonly static SqlDataType Numeric = new SqlDataType("numeric", 108, SqlDbType.Decimal, typeof(decimal), typeof(decimal?));
        public readonly static SqlDataType NVarChar = new SqlDataType("nvarchar", 231, SqlDbType.NVarChar, typeof(string), typeof(string));
        public readonly static SqlDataType NText = new SqlDataType("ntext", 99, SqlDbType.NText, typeof(string), typeof(string));
        public readonly static SqlDataType Real = new SqlDataType("real", 59, SqlDbType.Real, typeof(Single), typeof(Single?));
        public readonly static SqlDataType SmallDatetime = new SqlDataType("smalldatetime", 58, SqlDbType.SmallDateTime, typeof(DateTime), typeof(DateTime?));
        public readonly static SqlDataType SmallInt = new SqlDataType("smallint", 52, SqlDbType.SmallInt, typeof(short), typeof(short?));
        public readonly static SqlDataType SmallMoney = new SqlDataType("smallmoney", 122, SqlDbType.SmallMoney, typeof(decimal), typeof(decimal?));
        public readonly static SqlDataType Text = new SqlDataType("text", 35, SqlDbType.Text, typeof(string), typeof(string));
        public readonly static SqlDataType Time = new SqlDataType("time", 41, SqlDbType.Time, typeof(TimeSpan), typeof(TimeSpan?));
        public readonly static SqlDataType Timestamp = new SqlDataType("timestamp", 189, SqlDbType.Timestamp, typeof(DateTime), typeof(DateTime?));
        public readonly static SqlDataType TinyInt = new SqlDataType("tinyint", 48, SqlDbType.TinyInt, typeof(byte), typeof(byte?));
        public readonly static SqlDataType Uniqueidentifier = new SqlDataType("uniqueidentifier", 36, SqlDbType.UniqueIdentifier, typeof(Guid), typeof(Guid?));
        public readonly static SqlDataType VarBinary = new SqlDataType("varbinary", 165, SqlDbType.VarBinary, typeof(byte[]), typeof(byte[]));
        public readonly static SqlDataType VarChar = new SqlDataType("varchar", 167, SqlDbType.VarChar, typeof(string), typeof(string));
        public readonly static SqlDataType Variant = new SqlDataType("sql_variant", 98, SqlDbType.Variant, typeof(object), typeof(object));

        public static readonly SqlDataType[] TypesWithLength = new[] { Char, VarChar, NChar, NVarChar, Binary, VarBinary };
        public static readonly SqlDataType[] TypesWithScaleAndPrecision = new[] { Decimal, Numeric };

        // All mappings
        private readonly static SqlDataType[] SqlTypes = new [] {
            BigInt, Binary, Bit, Char, Date, Datetime, Datetime2, DatetimeOffset, Decimal, Float, Int,
            Money, NChar, Numeric, NVarChar, NText, Real, SmallDatetime, SmallInt, SmallMoney,
            Text, Time, Timestamp, TinyInt, Uniqueidentifier, VarBinary, VarChar, Variant };

        public static Type GetClrType(this byte sqlId, bool nullable)
        {
            var type = SqlTypes.FirstOrDefault(x => x.SqlId == sqlId);
            if (type != null) return nullable ? type.ClrNullableType : type.ClrType;
            throw new Exception(string.Format("No CLR data type found to match SQL data type id '{0}'.", sqlId));
        }

        public static Type GetClrType(this string sqlName, bool nullable)
        {
            var type = SqlTypes.FirstOrDefault(x => x.SqlName == sqlName);
            if (type != null) return nullable ? type.ClrNullableType : type.ClrType;
            throw new Exception(string.Format("No CLR data type found to match SQL data type '{0}'.", sqlName));
        }

        public static SqlDbType GetSqlType(this byte sqlId)
        {
            var type = SqlTypes.FirstOrDefault(x => x.SqlId == sqlId);
            if (type != null) return type.SqlType;
            throw new Exception(string.Format("No SQL data type found to match data type id'{0}'.", sqlId));
        }

        // These are the distinct clr to sql type mapping
        private static readonly SqlDataType[] ClrTypeMapping = new[] {
            BigInt, Bit, Datetime, DatetimeOffset, Decimal, Float, Int, NVarChar, 
            Real, SmallInt, Time, TinyInt, Uniqueidentifier, VarBinary, Variant};

        public static SqlDbType GetSqlType(this Type clrType)
        {
            var type = ClrTypeMapping.FirstOrDefault(x => x.ClrType == clrType || x.ClrNullableType == clrType);
            if (type != null) return type.SqlType;
            throw new Exception(string.Format("No SQL data type found to match CLR data type'{0}'.", clrType.Name));
        }

        public static string GetSqlTypeName(this Type clrType)
        {
            var type = ClrTypeMapping.FirstOrDefault(x => x.ClrType == clrType || x.ClrNullableType == clrType);
            if (type != null) return type.SqlName;
            throw new Exception(string.Format("No SQL data type found to match CLR data type'{0}'.", clrType.Name));
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
            throw new Exception(string.Format("No CLR data type found to match SQL data type '{0}'.", sqlType));
        }

        public static string GetSqlTypeName(this SqlDbType sqlType)
        {
            var type = SqlTypeMapping.FirstOrDefault(x => x.SqlType == sqlType);
            if (type != null) return type.SqlName;
            throw new Exception(string.Format("No data type found to match SQL data type '{0}'.", sqlType));
        }
    }
}

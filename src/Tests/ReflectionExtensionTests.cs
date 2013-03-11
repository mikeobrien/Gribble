using System;
using Gribble;
using NUnit.Framework;
using Should;

namespace Tests
{
    [TestFixture]
    public class ReflectionExtensionTests
    {
         [Test]
         public void should_convert_object_to_dictionary()
         {
             object values = new
             {
                 String = "Neils",
                 Boolean = (bool)true,
                 Byte = (byte)5,
                 SByte = (sbyte)6,
                 Int16 = (Int16)7,
                 UInt16 = (UInt16)8,
                 Int32 = 9,
                 UInt32 = (UInt32)10,
                 Int64 = (Int64)11,
                 UInt64 = (UInt64)12,
                 IntPtr = (IntPtr)13,
                 UIntPtr = (UIntPtr)14,
                 Char = 'a',
                 Double = (Double)15,
                 Single = (Single)16,
                 DateTime = DateTime.MaxValue,
                 Guid = Guid.Empty,
                 TimeSpan = TimeSpan.FromSeconds(15),
                 Nullable = (int?)null,
                 NullableBoolean = (bool?)true,
                 NullableByte = (byte?)5,
                 NullableSByte = (sbyte?)6,
                 NullableInt16 = (Int16?)7,
                 NullableUInt16 = (UInt16?)8,
                 NullableInt32 = (Int32?)9,
                 NullableUInt32 = (UInt32?)10,
                 NullableInt64 = (Int64?)11,
                 NullableUInt64 = (UInt64?)12,
                 NullableIntPtr = (IntPtr?)13,
                 NullableUIntPtr = (UIntPtr?)14,
                 NullableChar = (Char?)'a',
                 NullableDouble = (Double?)15,
                 NullableSingle = (Single?)16,
                 NullableDateTime = (DateTime?)DateTime.MaxValue,
                 NullableGuid = (Guid?)Guid.Empty,
                 NullableTimeSpan = (TimeSpan?)TimeSpan.FromSeconds(15)
             };

             var result = values.ToDictionary();
             result.Count.ShouldEqual(36);

             result["String"].ShouldEqual("Neils");
             result["Byte"].ShouldEqual((byte)5);
             result["SByte"].ShouldEqual((sbyte)6);
             result["Int16"].ShouldEqual((Int16)7);
             result["UInt16"].ShouldEqual((UInt16)8);
             result["Int32"].ShouldEqual(9);
             result["UInt32"].ShouldEqual((UInt32)10);
             result["Int64"].ShouldEqual((Int64)11);
             result["UInt64"].ShouldEqual((UInt64)12);
             result["IntPtr"].ShouldEqual((IntPtr)13);
             result["UIntPtr"].ShouldEqual((UIntPtr)14);
             result["Char"].ShouldEqual('a');
             result["Double"].ShouldEqual((Double)15);
             result["Single"].ShouldEqual((Single)16);
             result["DateTime"].ShouldEqual(DateTime.MaxValue);
             result["Guid"].ShouldEqual(Guid.Empty);
             result["TimeSpan"].ShouldEqual(TimeSpan.FromSeconds(15));

             result["Nullable"].ShouldBeNull();
             result["NullableByte"].ShouldEqual((byte)5);
             result["NullableSByte"].ShouldEqual((sbyte)6);
             result["NullableInt16"].ShouldEqual((Int16)7);
             result["NullableUInt16"].ShouldEqual((UInt16)8);
             result["NullableInt32"].ShouldEqual(9);
             result["NullableUInt32"].ShouldEqual((UInt32)10);
             result["NullableInt64"].ShouldEqual((Int64)11);
             result["NullableUInt64"].ShouldEqual((UInt64)12);
             result["NullableIntPtr"].ShouldEqual((IntPtr)13);
             result["NullableUIntPtr"].ShouldEqual((UIntPtr)14);
             result["NullableChar"].ShouldEqual('a');
             result["NullableDouble"].ShouldEqual((Double)15);
             result["NullableSingle"].ShouldEqual((Single)16);
             result["NullableDateTime"].ShouldEqual(DateTime.MaxValue);
             result["NullableGuid"].ShouldEqual(Guid.Empty);
             result["NullableTimeSpan"].ShouldEqual(TimeSpan.FromSeconds(15));
         }
    }
}
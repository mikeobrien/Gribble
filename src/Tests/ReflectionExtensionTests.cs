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
                 Boolean = true,
                 Byte = 5,
                 SByte = 6,
                 Int16 = 7,
                 UInt16 = 8,
                 Int32 = 9,
                 UInt32 = 10,
                 Int64 = 11,
                 UInt64 = 12,
                 IntPtr = 13,
                 UIntPtr = 14,
                 Char = 'a',
                 Double = 15,
                 Single = 16,
                 DateTime = DateTime.MaxValue,
                 Guid = Guid.Empty,
                 TimeSpan = TimeSpan.FromSeconds(15)
             };
             var result = values.ToDictionary();
             result.Count.ShouldEqual(18);
             result["String"].ShouldEqual("Neils");
             result["Boolean"].ShouldEqual(true);
             result["Byte"].ShouldEqual(5);
             result["SByte"].ShouldEqual(6);
             result["Int16"].ShouldEqual(7);
             result["UInt16"].ShouldEqual(8);
             result["Int32"].ShouldEqual(9);
             result["UInt32"].ShouldEqual(10);
             result["Int64"].ShouldEqual(11);
             result["UInt64"].ShouldEqual(12);
             result["IntPtr"].ShouldEqual(13);
             result["UIntPtr"].ShouldEqual(14);
             result["Char"].ShouldEqual('a');
             result["Double"].ShouldEqual(15);
             result["Single"].ShouldEqual(16);
             result["DateTime"].ShouldEqual(DateTime.MaxValue);
             result["Guid"].ShouldEqual(Guid.Empty);
             result["TimeSpan"].ShouldEqual(TimeSpan.FromSeconds(15));
         }
    }
}
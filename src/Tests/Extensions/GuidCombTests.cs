using System;
using Gribble.Extensions;
using NUnit.Framework;
using Should;

namespace Tests.Extensions
{
    [TestFixture]
    public class GuidCombTests
    {
        [Test]
        public void Should_generate_guid()
        {
            GuidComb.Create().ShouldNotEqual(Guid.Empty);
        }
    }
}

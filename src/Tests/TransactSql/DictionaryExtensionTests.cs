using System.Collections.Generic;
using System.Linq;
using Gribble;
using Gribble.Extensions;
using Gribble.TransactSql;
using NUnit.Framework;
using Should;

namespace Tests.TransactSql
{
    [TestFixture]
    public class DictionaryExtensionTests
    {
        [Test]
        public void Add_Random_Parameter_Test()
        {
            var dictionary = new Dictionary<string, object>();
            Enumerable.Range(1, 10).ToList().ForEach(x => dictionary.AddWithRandomlyNamedKey(x));
            dictionary.Select(x => x.Key).Distinct().Count().ShouldEqual(10);
            dictionary.Select(x => x.Value).ToList().ForEach(x => x.ShouldBeType(typeof(int)));
            dictionary.Select(x => x.Value).ToList().ForEach(x => x.ShouldBeGreaterThan(0));
        }
    }
}

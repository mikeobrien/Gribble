using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Gribble;
using Should;

namespace Tests
{
    [TestFixture]
    public class EnumerableTests
    {
        public class Entity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime Birthdate { get; set; }
            public DateTime? Created { get; set; }
            public int Age { get; set; }
            public float Price { get; set; }
            public double Distance { get; set; }
            public byte Flag { get; set; }
            public bool Active { get; set; }
            public decimal Length { get; set; }
            public long Miles { get; set; }
            public Dictionary<string, object> Values { get; set; }
        }

        private static readonly IEnumerable<string> Items = CreateItemsList();

        private static IEnumerable<string> CreateItemsList()
        {
            return new List<string> { "One", "Two", "Three", "Four", "Five", "One", "Two", "Three", "Four", "Five" };
        }

        private static readonly IEnumerable<Entity> Entities = CreateEntitiesList();

        private static IEnumerable<Entity> CreateEntitiesList()
        {
            return new List<Entity> 
            { 
                new Entity { Id = Guid.NewGuid(), Name = "Tom", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 33, Price = 88.99F, Distance = 34.5, Flag = 3, Active = true },
                new Entity { Id = Guid.NewGuid(), Name = "Tom", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 66, Price = 88.99F, Distance = 34.5, Flag = 12, Active = true },
                new Entity { Id = Guid.NewGuid(), Name = "Tom", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 66, Price = 88.99F, Distance = 34.5, Flag = 12, Active = true },
                new Entity { Id = Guid.NewGuid(), Name = "Dick", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 44, Price = 88.99F, Distance = 34.5, Flag = 6, Active = true },
                new Entity { Id = Guid.NewGuid(), Name = "Dick", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 77, Price = 88.99F, Distance = 34.5, Flag = 15, Active = true },
                new Entity { Id = Guid.NewGuid(), Name = "Dick", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 77, Price = 88.99F, Distance = 34.5, Flag = 15, Active = true },
                new Entity { Id = Guid.NewGuid(), Name = "Dick", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 44, Price = 88.99F, Distance = 34.5, Flag = 6, Active = true },
                new Entity { Id = Guid.NewGuid(), Name = "Harry", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 55, Price = 88.99F, Distance = 34.5, Flag = 9, Active = true },
                new Entity { Id = Guid.NewGuid(), Name = "Harry", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 55, Price = 88.99F, Distance = 34.5, Flag = 9, Active = true }
            };
        }

        [Test]
        public void Randomize_Test()
        {
            var items = CreateItemsList();
            items.Randomize().Zip(items, (x, y) => x == y).All(x => x).ShouldEqual(false);
        }

        [Test]
        public void TakePercent_Test()
        {
            CreateItemsList().TakePercent(70).Count().ShouldEqual(7);
        }

        [Test]
        public void SelectInto_Test()
        {
            var items = (IList<string>)CreateItemsList();
            Items.Take(6).CopyTo(items).Count().ShouldEqual(16);
            items.Count().ShouldEqual(16);
        }

        [Test]
        public void Distinct_Test()
        {
            CreateItemsList().Distinct(x => x).Count().ShouldEqual(5);
        }

        [Test]
        public void Intersect_Test()
        {
            var compare = CreateEntitiesList().Where(x => (x.Name == "Tom" || x.Name == "Dick") && (x.Age == 33 || x.Age == 77)).ToList();
            var results = CreateEntitiesList().Intersect(compare, x => x.Name, x => x.Age).ToList();
            results.Count.ShouldEqual(3);
            results.Where(x => x.Name == "Tom" && x.Age == 33).Count().ShouldEqual(1);
            results.Where(x => x.Name == "Dick" && x.Age == 77).Count().ShouldEqual(2);
        }

        [Test]
        public void Except_Test()
        {
            var compare = CreateEntitiesList().Where(x => (x.Name == "Tom" || x.Name == "Dick") && (x.Age == 33 || x.Age == 77)).ToList();
            var results = CreateEntitiesList().Except(compare, x => x.Name, x => x.Age).ToList();
            results.Count.ShouldEqual(6);
            results.Where(x => x.Name == "Tom" && x.Age == 66).Count().ShouldEqual(2);
            results.Where(x => x.Name == "Dick" && x.Age == 44).Count().ShouldEqual(2);
            results.Where(x => x.Name == "Harry" && x.Age == 55).Count().ShouldEqual(2);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using Gribble.Model;
using NUnit.Framework;
using Gribble;
using Should;

namespace Tests
{
    [TestFixture]
    public class QueryableTests
    {
        public class Entity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
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

        private static readonly IQueryable<string> Items = CreateItemsList();

        private static IQueryable<string> CreateItemsList()
        {
            return new List<string> { "One", "Two", "Three", "Four", "Five", "One", "Two", "Three", "Four", "Five" }.AsQueryable();
        }

        private static IQueryable<Entity> CreateEntitiesList()
        {
            return new List<Entity> 
            { 
                new Entity { Id = Guid.NewGuid(), Name = "Tom", Email = "tom@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 33, Price = 88.99F, Distance = 34.5, Flag = 3, Active = false },
                new Entity { Id = Guid.NewGuid(), Name = "Tom", Email = "tom@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 66, Price = 88.99F, Distance = 34.5, Flag = 12, Active = true },
                new Entity { Id = Guid.NewGuid(), Name = "Tom", Email = "thomas@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 66, Price = 88.99F, Distance = 34.5, Flag = 12, Active = true },
                new Entity { Id = Guid.NewGuid(), Name = "Dick", Email = "dick@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 44, Price = 88.99F, Distance = 34.5, Flag = 6, Active = true },
                new Entity { Id = Guid.NewGuid(), Name = "Dick", Email = "richard@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 77, Price = 88.99F, Distance = 34.5, Flag = 15, Active = true },
                new Entity { Id = Guid.NewGuid(), Name = "Dick", Email = "rich@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 77, Price = 88.99F, Distance = 34.5, Flag = 15, Active = true },
                new Entity { Id = Guid.NewGuid(), Name = "Dick", Email = "rick@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 44, Price = 88.99F, Distance = 34.5, Flag = 6, Active = true },
                new Entity { Id = Guid.NewGuid(), Name = "Harry", Email = "harry@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 65, Price = 88.99F, Distance = 34.5, Flag = 9, Active = true },
                new Entity { Id = Guid.NewGuid(), Name = "Harry", Email = "harry@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 55, Price = 88.99F, Distance = 34.5, Flag = 9, Active = false }
            }.AsQueryable();
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
        public void Copy_To_Test()
        {
            var items = CreateItemsList();
            Items.Take(6).CopyTo(items).Count().ShouldEqual(16);
            items.ShouldNotBeSameAs(Items);
            items.Count().ShouldEqual(16);
        }

        [Test]
        public void Copy_To_New_Test()
        {
            var items = Items.Take(6).CopyTo(string.Empty);
            items.ShouldNotBeSameAs(Items);
            items.Count().ShouldEqual(6);
        }

        [Test]
        public void Distinct_Test()
        {
            CreateItemsList().Distinct(x => x).Count().ShouldEqual(5);
        }

        [Test]
        public void should_return_duplicates()
        {
            var duplicates = CreateEntitiesList().Duplicates(x => x.Email).ToList();
            duplicates.Count.ShouldEqual(2);
            var result = duplicates.FirstOrDefault(x => x.Email == "tom@yada.com");
            result.ShouldNotBeNull();
            result = duplicates.FirstOrDefault(x => x.Email == "harry@yada.com");
            result.ShouldNotBeNull();
        }

        [Test]
        public void should_return_duplicates_of_precidence()
        {
            var duplicates = CreateEntitiesList().Duplicates(x => x.Email, x => x.Active).ToList();
            duplicates.Count.ShouldEqual(2);
            var result = duplicates.FirstOrDefault(x => x.Email == "tom@yada.com");
            result.ShouldNotBeNull();
            result.Active.ShouldBeTrue();
            result = duplicates.FirstOrDefault(x => x.Email == "harry@yada.com");
            result.ShouldNotBeNull();
            result.Active.ShouldBeTrue();
        }

        [Test]
        public void should_return_duplicates_of_order()
        {
            var duplicates = CreateEntitiesList().Duplicates(x => x.Email, x => x.Age, Order.Ascending).ToList();
            duplicates.Count.ShouldEqual(2);
            var result = duplicates.FirstOrDefault(x => x.Email == "tom@yada.com");
            result.ShouldNotBeNull();
            result.Age.ShouldEqual(66);
            result = duplicates.FirstOrDefault(x => x.Email == "harry@yada.com");
            result.Age.ShouldEqual(65);
        }

        [Test]
        public void should_return_duplicates_of_order_descending()
        {
            var duplicates = CreateEntitiesList().Duplicates(x => x.Email, x => x.Age, Order.Descending).ToList();
            duplicates.Count.ShouldEqual(2);
            var result = duplicates.FirstOrDefault(x => x.Email == "tom@yada.com");
            result.ShouldNotBeNull();
            result.Age.ShouldEqual(33);
            result = duplicates.FirstOrDefault(x => x.Email == "harry@yada.com");
            result.Age.ShouldEqual(55);
        }

        [Test]
        public void Intersect_Test()
        {
            var compare = CreateEntitiesList().Where(x => (x.Name == "Tom" || x.Name == "Dick") && (x.Age == 33 || x.Age == 77)).ToList();
            var results = CreateEntitiesList().Intersect(compare, x => x.Name, x => x.Age).ToList();
            results.Count.ShouldEqual(3);
            results.Count(x => x.Name == "Tom" && x.Age == 33).ShouldEqual(1);
            results.Count(x => x.Name == "Dick" && x.Age == 77).ShouldEqual(2);
        }

        [Test]
        public void Except_Test()
        {
            var compare = CreateEntitiesList().Where(x => (x.Name == "Tom" || x.Name == "Dick") && (x.Age == 33 || x.Age == 77)).ToList();
            var results = CreateEntitiesList().Except(compare, x => x.Name, x => x.Age).ToList();
            results.Count.ShouldEqual(6);
            results.Count(x => x.Name == "Tom" && x.Age == 66).ShouldEqual(2);
            results.Count(x => x.Name == "Dick" && x.Age == 44).ShouldEqual(2);
            results.Count(x => x.Name == "Harry" && x.Age == 55).ShouldEqual(1);
            results.Count(x => x.Name == "Harry" && x.Age == 65).ShouldEqual(1);
        }
    }
}

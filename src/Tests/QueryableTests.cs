using System;
using System.Collections.Generic;
using System.Linq;
using Gribble.Model;
using NUnit.Framework;
using Gribble;
using Should;
using Queryable = Gribble.Queryable;

namespace Tests
{
    [TestFixture]
    public class QueryableTests
    {
        public class Entity
        {
            public int Id { get; set; }
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
            public int Group { get; set; }
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
                new Entity { Id = 1, Name = "Tom", Email = "tom@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 33, Price = 88.99F, Distance = 34.5, Flag = 3, Active = false, Group = 1 },
                new Entity { Id = 2, Name = "Tom", Email = "tom@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 66, Price = 88.99F, Distance = 34.5, Flag = 12, Active = true},
                new Entity { Id = 3, Name = "Tom", Email = "thomas@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 66, Price = 88.99F, Distance = 34.5, Flag = 12, Active = true },
                new Entity { Id = 4, Name = "Dick", Email = "dick@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 44, Price = 88.99F, Distance = 34.5, Flag = 6, Active = true },
                new Entity { Id = 5, Name = "Dick", Email = "richard@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 77, Price = 88.99F, Distance = 34.5, Flag = 15, Active = true },
                new Entity { Id = 6, Name = "Dick", Email = "rich@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 77, Price = 88.99F, Distance = 34.5, Flag = 15, Active = true },
                new Entity { Id = 7, Name = "Dick", Email = "rick@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 44, Price = 88.99F, Distance = 34.5, Flag = 6, Active = true},
                new Entity { Id = 8, Name = "Harry", Email = "harry@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 65, Price = 88.99F, Distance = 34.5, Flag = 9, Active = true, Group = 1 },
                new Entity { Id = 9, Name = "Harry", Email = "harry@yada.com", Birthdate = DateTime.Now, Created = DateTime.Now, Age = 55, Price = 88.99F, Distance = 34.5, Flag = 8, Active = false }
            }.AsQueryable();
        }

        [Test]
        public void should_return_randomized_results()
        {
            var items = CreateItemsList();
            items.Randomize().Zip(items, (x, y) => x == y).All(x => x).ShouldEqual(false);
        }

        [Test]
        public void should_return_a_percentage_of_results()
        {
            CreateItemsList().TakePercent(70).Count().ShouldEqual(7);
        }

        [Test]
        public void should_return_distinct()
        {
            CreateEntitiesList().Distinct(x => x.Name).Count().ShouldEqual(3);
        }

        [Test]
        public void should_return_distinct_ordered_ascending()
        {
            var results = CreateEntitiesList().Distinct(x => x.Name, x => x.Age, Order.Ascending).ToList();
            results.Count.ShouldEqual(3);
            results.Any(x => x.Name == "Tom" && x.Age == 33).ShouldBeTrue();
            results.Any(x => x.Name == "Dick" && x.Age == 44).ShouldBeTrue();
            results.Any(x => x.Name == "Harry" && x.Age == 55).ShouldBeTrue();
        }

        [Test]
        public void should_return_distinct_ordered_descending()
        {
            var results = CreateEntitiesList().Distinct(x => x.Name, x => x.Age, Order.Descending).ToList();
            results.Count.ShouldEqual(3);
            results.Any(x => x.Name == "Tom" && x.Age == 66).ShouldBeTrue();
            results.Any(x => x.Name == "Dick" && x.Age == 77).ShouldBeTrue();
            results.Any(x => x.Name == "Harry" && x.Age == 65).ShouldBeTrue();
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
            var duplicates = CreateEntitiesList().Duplicates(x => x.Email, x => x.Active == true, Order.Ascending).ToList();
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
        public void should_return_duplicates_of_double_order()
        {
            var duplicates = CreateEntitiesList().Duplicates(x => x.Email, x => x.Distance, Order.Ascending, x => x.Flag, Order.Descending).ToList();
            duplicates.Count.ShouldEqual(2);
            var result = duplicates.FirstOrDefault(x => x.Email == "tom@yada.com");
            result.ShouldNotBeNull();
            result.Flag.ShouldEqual((byte)3);
            result = duplicates.FirstOrDefault(x => x.Email == "harry@yada.com");
            result.Flag.ShouldEqual((byte)8);
        }

        [Test]
        public void should_return_duplicates_of_double_order_descending()
        {
            var duplicates = CreateEntitiesList().Duplicates(x => x.Email, x => x.Distance, Order.Descending, x => x.Flag, Order.Ascending).ToList();
            duplicates.Count.ShouldEqual(2);
            var result = duplicates.FirstOrDefault(x => x.Email == "tom@yada.com");
            result.ShouldNotBeNull();
            result.Flag.ShouldEqual((byte)12);
            result = duplicates.FirstOrDefault(x => x.Email == "harry@yada.com");
            result.Flag.ShouldEqual((byte)9);
        }

        [Test]
        public void should_copy_to_a_list()
        {
            var target = CreateEntitiesList();
            var source = CreateEntitiesList();
            source.Take(6).CopyTo(target).Count().ShouldEqual(15);
            target.ShouldNotBeSameAs(Items);
            target.Count().ShouldEqual(15);
        }

        [Test]
        public void should_sync_list_with_included_fields()
        {
            var results = CreateEntitiesList();
            results.Where(x => x.Group != 1).SyncWith(CreateEntitiesList().Where(x => x.Group == 1), x => x.Name, 
                SyncFields.Include, x => x.Age, x => x.Flag).ToList();
            results.Count().ShouldEqual(9);
            var original = CreateEntitiesList();
            results.Where(x => x.Group == 1).Join(original, x => x.Id, x => x.Id, (x, y) => new { Target = x, Source = y }).All(x =>
                x.Target.Name == x.Source.Name &&
                x.Target.Email == x.Source.Email &&
                x.Target.Age == x.Source.Age &&
                x.Target.Flag == x.Source.Flag &&
                x.Target.Active == x.Source.Active).ShouldBeTrue();
            results.Where(x => x.Group != 1).Join(original, x => x.Id, x => x.Id, (x, y) => new { Target = x, Source = y }).All(x =>
                x.Target.Name == x.Source.Name &&
                x.Target.Email == x.Source.Email &&
                x.Target.Active == x.Source.Active).ShouldBeTrue();
            results.Where(x => x.Group != 1).Join(results.Where(x => x.Group == 1), x => x.Name, x => x.Name, (x, y) => new { Target = x, Source = y }).All(x =>
                x.Target.Age == x.Source.Age &&
                x.Target.Flag == x.Source.Flag).ShouldBeTrue();
        }

        [Test]
        public void should_sync_list_with_excluded_fields()
        {
            var results = CreateEntitiesList();
            results.Where(x => x.Group != 1).SyncWith(CreateEntitiesList().Where(x => x.Group == 1), x => x.Name, 
                SyncFields.Exclude, x => x.Id, x => x.Age, x => x.Flag, x => x.Group).ToList();
            results.Count().ShouldEqual(9);
            var original = CreateEntitiesList();
            results.Distinct(x => x.Id).Count().ShouldEqual(9);
            results.Where(x => x.Group == 1).Join(original, x => x.Id, x => x.Id, (x, y) => new { Target = x, Source = y }).All(x =>
                x.Target.Name == x.Source.Name &&
                x.Target.Email == x.Source.Email &&
                x.Target.Age == x.Source.Age &&
                x.Target.Flag == x.Source.Flag &&
                x.Target.Active == x.Source.Active).ShouldBeTrue();
            results.Where(x => x.Group != 1).Join(original, x => x.Id, x => x.Id, (x, y) => new { Target = x, Source = y }).All(x =>
                x.Target.Name == x.Source.Name &&
                x.Target.Age == x.Source.Age &&
                x.Target.Flag == x.Source.Flag).ShouldBeTrue();
            results.Where(x => x.Group != 1).Join(results.Where(x => x.Group == 1), x => x.Name, x => x.Name, (x, y) => new { Target = x, Source = y }).All(x =>
                x.Target.Email == x.Source.Email &&
                x.Target.Active == x.Source.Active).ShouldBeTrue();
        }

        [Test]
        public void should_return_intersected_results()
        {
            var compare = CreateEntitiesList().Where(x => (x.Name == "Tom" || x.Name == "Dick") && (x.Age == 33 || x.Age == 77)).ToList();
            var results = CreateEntitiesList().Intersect(compare, x => x.Name, x => x.Age).ToList();
            results.Count.ShouldEqual(3);
            results.Count(x => x.Name == "Tom" && x.Age == 33).ShouldEqual(1);
            results.Count(x => x.Name == "Dick" && x.Age == 77).ShouldEqual(2);
        }

        [Test]
        public void should_return_except_results()
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

using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gribble.Expressions;
using Gribble.Mapping;
using Gribble.TransactSql;
using NUnit.Framework;
using Should;

namespace Tests.TransactSql
{
    [TestFixture]
    public class WhereWriterTests
    {
        public class Entity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public DateTime Birthdate { get; set; }
            public int Age { get; set; }
            public float Price { get; set; }
            public double Distance { get; set; }
            public byte? Flag { get; set; }
            public bool Active { get; set; }
            public decimal Length { get; set; }
            public long Miles { get; set; }
            public Dictionary<string, object> Values {get; set;}
        }

        public class EntityMap : ClassMap<Entity>
        {
            public EntityMap()
            {
                Id(x => x.Id).Column("id");
                Map(x => x.Name).Column("name");
                Map(x => x.Birthdate).Column("birthdate");
                Map(x => x.Age).Column("age");
                Map(x => x.Price).Column("price");
                Map(x => x.Distance).Column("distance");
                Map(x => x.Flag).Column("flag");
                Map(x => x.Active).Column("active");
                Map(x => x.Length).Column("length");
                Map(x => x.Miles).Column("miles");
                Map(x => x.Values).Dynamic();
            }
        }

        private static readonly EntityMapping Map = new EntityMapping(new EntityMap());

        [Test]
        public void And_Test()
        {
            Expression<Func<Entity,bool>> expression = x => x.Age == 10 && x.Flag != null;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(10);
            statement.Text.ShouldEqual(string.Format("(([age] = @{0}) AND (CAST([flag] AS int) IS NOT NULL))", statement.Parameters.First().Key));
        }

        [Test]
        public void Or_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Age == 10 || x.Flag != null;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(10);
            statement.Text.ShouldEqual(string.Format("(([age] = @{0}) OR (CAST([flag] AS int) IS NOT NULL))", statement.Parameters.First().Key));
        }

        [Test]
        public void Null_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Flag == null;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("(CAST([flag] AS int) IS NULL)");
        }

        [Test]
        public void Not_Null_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Flag != null;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("(CAST([flag] AS int) IS NOT NULL)");
        }

        [Test]
        public void Not_Expression_Test()
        {
            Expression<Func<Entity, bool>> expression = x => !(x.Age == 33);
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual(33);
            statement.Parameters.Skip(1).First().Value.ShouldEqual(false);
            statement.Text.ShouldEqual(string.Format("(CASE WHEN ([age] = @{0}) THEN 1 ELSE 0 END = @{1})", 
                                                                    statement.Parameters.First().Key,
                                                                    statement.Parameters.Skip(1).First().Key));
        }

        [Test]
        public void Equals_Boolean_Expression_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Active == (x.Age == 33);
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);

            statement.Parameters.Count().ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(33);
            statement.Text.ShouldEqual(string.Format("([active] = CASE WHEN ([age] = @{0}) THEN 1 ELSE 0 END)", statement.Parameters.First().Key));
        }

        [Test]
        public void Boolean_Expression_Equals_Boolean_Expression_Test()
        {
            Expression<Func<Entity, bool>> expression = x => (x.Distance == 30.0) == (x.Age == 33);
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);

            statement.Parameters.Count().ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual(30.0);
            statement.Parameters.Skip(1).First().Value.ShouldEqual(33);
            statement.Text.ShouldEqual(string.Format("(CASE WHEN ([distance] = @{0}) THEN 1 ELSE 0 END = CASE WHEN ([age] = @{1}) THEN 1 ELSE 0 END)",
                                                            statement.Parameters.First().Key,
                                                            statement.Parameters.Skip(1).First().Key));
        }

        [Test]
        public void Equals_Boolean_Not_Expression_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Active == !(x.Age == 33);
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);

            statement.Parameters.Count().ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual(33);
            statement.Parameters.Skip(1).First().Value.ShouldEqual(false);
            statement.Text.ShouldEqual(string.Format("([active] = CASE WHEN (CASE WHEN ([age] = @{0}) THEN 1 ELSE 0 END = @{1}) THEN 1 ELSE 0 END)", 
                                                            statement.Parameters.First().Key,
                                                            statement.Parameters.Skip(1).First().Key));
        }

        [Test]
        public void Not_Bool_Test()
        {
            Expression<Func<Entity, bool>> expression = x => !x.Active && !(bool)x.Values["optout"];
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);

            statement.Parameters.Count().ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual(false);
            statement.Parameters.Skip(1).First().Value.ShouldEqual(false);
            statement.Text.ShouldEqual(string.Format("(([active] = @{0}) AND (CAST([optout] AS bit) = @{1}))",
                                                statement.Parameters.First().Key,
                                                statement.Parameters.Skip(1).First().Key));
        }

        [Test]
        public void True_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Active == true;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);

            statement.Parameters.Count().ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(true);
            statement.Text.ShouldEqual(string.Format("([active] = @{0})", statement.Parameters.First().Key));
        }

        [Test]
        public void Unary_True_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Active;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(true);
            statement.Text.ShouldEqual(string.Format("([active] = @{0})", statement.Parameters.First().Key));
        }

        [Test]
        public void Equal_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Age == 33;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(33);
            statement.Text.ShouldEqual(string.Format("([age] = @{0})", statement.Parameters.First().Key));
        }

        [Test]
        public void Not_Equal_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Age != 33;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(33);
            statement.Text.ShouldEqual(string.Format("([age] <> @{0})", statement.Parameters.First().Key));
        }

        [Test]
        public void Greater_Than_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Age > 33;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(33);
            statement.Text.ShouldEqual(string.Format("([age] > @{0})", statement.Parameters.First().Key));
        }

        [Test]
        public void Less_Than_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Age < 33;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(33);
            statement.Text.ShouldEqual(string.Format("([age] < @{0})", statement.Parameters.First().Key));
        }

        [Test]
        public void Greater_Than_Or_Equal_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Age >= 33;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(33);
            statement.Text.ShouldEqual(string.Format("([age] >= @{0})", statement.Parameters.First().Key));
        }

        [Test]
        public void Less_Than_Or_Equal_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Age <= 33;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(33);
            statement.Text.ShouldEqual(string.Format("([age] <= @{0})", statement.Parameters.First().Key));
        }

        [Test]
        public void Add_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Age + 20 == 55;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual(20);
            statement.Parameters.Skip(1).First().Value.ShouldEqual(55);
            statement.Text.ShouldEqual(string.Format("(([age] + @{0}) = @{1})", statement.Parameters.First().Key, statement.Parameters.Skip(1).First().Key));
        }

        [Test]
        public void Subtract_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Age - 20 == 55;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual(20);
            statement.Parameters.Skip(1).First().Value.ShouldEqual(55);
            statement.Text.ShouldEqual(string.Format("(([age] - @{0}) = @{1})", statement.Parameters.First().Key, statement.Parameters.Skip(1).First().Key));
        }

        [Test]
        public void Multiply_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Age * 20 == 55;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual(20);
            statement.Parameters.Skip(1).First().Value.ShouldEqual(55);
            statement.Text.ShouldEqual(string.Format("(([age] * @{0}) = @{1})", statement.Parameters.First().Key, statement.Parameters.Skip(1).First().Key));
        }

        [Test]
        public void Divide_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Age / 20 == 55;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual(20);
            statement.Parameters.Skip(1).First().Value.ShouldEqual(55);
            statement.Text.ShouldEqual(string.Format("(([age] / @{0}) = @{1})", statement.Parameters.First().Key, statement.Parameters.Skip(1).First().Key));
        }

        [Test]
        public void Modulo_Test()
        {
            Expression<Func<Entity, bool>> expression = x => x.Age % 20 == 55;
            var statement = WhereWriter<Entity>.CreateStatement(WhereVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual(20);
            statement.Parameters.Skip(1).First().Value.ShouldEqual(55);
            statement.Text.ShouldEqual(string.Format("(([age] % @{0}) = @{1})", statement.Parameters.First().Key, statement.Parameters.Skip(1).First().Key));
        }
    }
}

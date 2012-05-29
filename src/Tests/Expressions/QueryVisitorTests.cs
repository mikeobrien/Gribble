using System;
using System.Collections.Generic;
using System.Linq;
using Gribble;
using Gribble.Expressions;
using Gribble.Model;
using NUnit.Framework;
using Should;

namespace Tests.Expressions
{
    [TestFixture]
    public class QueryVisitorTests
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

        private const string TableName1 = "XLIST_1";
        private const string TableName2 = "XLIST_2";
        private const string TableName3 = "XLIST_3";
        private const string TableName4 = "XLIST_4";

        [Test]
        public void Tablename_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            
            select.From.Table.Name.ShouldEqual(TableName1);
        }

        [Test]
        public void Take_Count_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Take(5);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.HasTop.ShouldEqual(true);
            select.TopType.ShouldEqual(Select.TopValueType.Count);
            select.Top.ShouldEqual(5);
        }

        [Test]
        public void Take_Percent_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.TakePercent(5);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.HasTop.ShouldEqual(true);
            select.TopType.ShouldEqual(Select.TopValueType.Percent);
            select.Top.ShouldEqual(5);
        }

        [Test]
        public void Skip_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Skip(15);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.HasStart.ShouldEqual(true);
            select.Start.ShouldEqual(16);
        }

        [Test]
        public void First_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.First();
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.First.ShouldEqual(true);
        }

        [Test]
        public void First_Or_Default_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.FirstOrDefault();
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.FirstOrDefault.ShouldEqual(true);
        }

        [Test]
        public void Count_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Count();
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.Count.ShouldEqual(true);
        }

        [Test]
        public void Randomize_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Randomize();
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.Randomize.ShouldEqual(true);
        }

        [Test]
        public void Union_Chained_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);

            query.Union(MockQueryable<Entity>.Create(TableName2)).
                  Union(MockQueryable<Entity>.Create(TableName3)).
                  Union(MockQueryable<Entity>.Create(TableName4));
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.HasConditions.ShouldEqual(false);

            select.From.Type.ShouldEqual(Data.DataType.Query);
            select.From.HasQueries.ShouldEqual(true);
            select.From.Queries.Count.ShouldEqual(4);

            select.From.Queries[0].From.Table.ShouldNotBeNull();
            select.From.Queries[0].From.Table.Name.ShouldEqual(TableName4);

            select.From.Queries[1].From.Table.ShouldNotBeNull();
            select.From.Queries[1].From.Table.Name.ShouldEqual(TableName3);

            select.From.Queries[2].From.Table.ShouldNotBeNull();
            select.From.Queries[2].From.Table.Name.ShouldEqual(TableName2);

            select.From.Queries[3].From.Table.ShouldNotBeNull();
            select.From.Queries[3].From.Table.Name.ShouldEqual(TableName1);
        }

        [Test]
        public void Union_Chained_Post_Operations_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);

            query.Union(MockQueryable<Entity>.Create(TableName2)).
                  Union(MockQueryable<Entity>.Create(TableName3)).
                  Union(MockQueryable<Entity>.Create(TableName4)).Take(5);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();

            select.Top.ShouldEqual(5);

            select.From.Type.ShouldEqual(Data.DataType.Query);
            select.From.HasQueries.ShouldEqual(true);
            select.From.Queries.Count.ShouldEqual(4);

            select.From.Queries[0].From.Table.ShouldNotBeNull();
            select.From.Queries[0].From.Table.Name.ShouldEqual(TableName4);

            select.From.Queries[1].From.Table.ShouldNotBeNull();
            select.From.Queries[1].From.Table.Name.ShouldEqual(TableName3);

            select.From.Queries[2].From.Table.ShouldNotBeNull();
            select.From.Queries[2].From.Table.Name.ShouldEqual(TableName2);

            select.From.Queries[3].From.Table.ShouldNotBeNull();
            select.From.Queries[3].From.Table.Name.ShouldEqual(TableName1);
        }

        [Test]
        public void Union_Nested_A_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);

            query.Where(x => x.Age == 33).Union(MockQueryable<Entity>.Create(TableName2).
                                                    Union(MockQueryable<Entity>.Create(TableName3).Where(x => x.Active))).
                  Union(MockQueryable<Entity>.Create(TableName4).Take(5)).Take(6);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.Top.ShouldEqual(6);
            select.From.Type.ShouldEqual(Data.DataType.Query);
            select.From.HasQueries.ShouldEqual(true);
            select.From.Queries.Count.ShouldEqual(3);

            select.From.Queries[0].From.Table.ShouldNotBeNull();
            select.From.Queries[0].From.Type.ShouldEqual(Data.DataType.Table);
            select.From.Queries[0].From.Table.Name.ShouldEqual(TableName4);
            select.From.Queries[0].Top.ShouldEqual(5);

            select.From.Queries[1].From.Type.ShouldEqual(Data.DataType.Query);
            select.From.Queries[1].From.HasQueries.ShouldEqual(true);
            select.From.Queries[1].From.Queries.Count.ShouldEqual(2);

            select.From.Queries[1].From.Queries[0].From.Table.ShouldNotBeNull();
            select.From.Queries[1].From.Queries[0].From.Type.ShouldEqual(Data.DataType.Table);
            select.From.Queries[1].From.Queries[0].From.Table.Name.ShouldEqual(TableName3);
            select.From.Queries[1].From.Queries[0].Where.Type.ShouldEqual(Operator.OperatorType.Equal);

            select.From.Queries[1].From.Queries[1].HasConditions.ShouldEqual(false);
            select.From.Queries[1].From.Queries[1].From.Table.ShouldNotBeNull();
            select.From.Queries[1].From.Queries[1].From.Type.ShouldEqual(Data.DataType.Table);
            select.From.Queries[1].From.Queries[1].From.Table.Name.ShouldEqual(TableName2);

            select.From.Queries[2].From.Table.ShouldNotBeNull();
            select.From.Queries[2].From.Type.ShouldEqual(Data.DataType.Table);
            select.From.Queries[2].From.Table.Name.ShouldEqual(TableName1);
            select.From.Queries[2].Where.Type.ShouldEqual(Operator.OperatorType.Equal);
        }

        [Test]
        public void Union_Nested_B_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Union(MockQueryable<Entity>.Create(TableName2).Take(1).Union(MockQueryable<Entity>.Create(TableName3)).Take(3)).Take(5);

            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.Top.ShouldEqual(5);
            select.From.Type.ShouldEqual(Data.DataType.Query);
            select.From.HasQueries.ShouldEqual(true);
            select.From.Queries.Count.ShouldEqual(2);

            select.From.Queries[0].From.Type.ShouldEqual(Data.DataType.Query);
            select.From.Queries[0].From.HasQueries.ShouldEqual(true);
            select.From.Queries[0].From.Queries.Count.ShouldEqual(2);
            select.From.Queries[0].Top.ShouldEqual(3);

            select.From.Queries[0].From.Queries[0].HasConditions.ShouldEqual(false);
            select.From.Queries[0].From.Queries[0].From.Table.ShouldNotBeNull();
            select.From.Queries[0].From.Queries[0].From.Type.ShouldEqual(Data.DataType.Table);
            select.From.Queries[0].From.Queries[0].From.Table.Name.ShouldEqual(TableName3);

            select.From.Queries[0].From.Queries[1].From.Table.ShouldNotBeNull();
            select.From.Queries[0].From.Queries[1].From.Type.ShouldEqual(Data.DataType.Table);
            select.From.Queries[0].From.Queries[1].From.Table.Name.ShouldEqual(TableName2);
            select.From.Queries[0].From.Queries[1].Top.ShouldEqual(1);

            select.From.Queries[1].HasConditions.ShouldEqual(false);
            select.From.Queries[1].From.Table.ShouldNotBeNull();
            select.From.Queries[1].From.Type.ShouldEqual(Data.DataType.Table);
            select.From.Queries[1].From.Table.Name.ShouldEqual(TableName1);
        }

        [Test]
        public void Where_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Where(x => x.Age == 33 && (bool)x.Values["opt_out"] || x.Name.StartsWith("yada"));
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.Where.ShouldNotBeNull();
            select.HasWhere.ShouldEqual(true);
        }

        [Test]
        public void First_Where_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.First(x => x.Age == 33 && (bool)x.Values["opt_out"] || x.Name.StartsWith("yada"));
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.First.ShouldEqual(true);
            select.Where.ShouldNotBeNull();
            select.HasWhere.ShouldEqual(true);
        }

        [Test]
        public void First_Or_Default_Where_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.FirstOrDefault(x => x.Age == 33 && (bool)x.Values["opt_out"] || x.Name.StartsWith("yada"));
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.FirstOrDefault.ShouldEqual(true);
            select.Where.ShouldNotBeNull();
            select.HasWhere.ShouldEqual(true);
        }

        [Test]
        public void Multiple_Where_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Where(x => x.Age == 33).
                  First(x => x.Age != 5);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.First.ShouldEqual(true);
            select.Where.ShouldNotBeNull();
            select.HasWhere.ShouldEqual(true);
            select.Where.Type.ShouldEqual(Operator.OperatorType.And);
            select.Where.LeftOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            select.Where.LeftOperand.Operator.Type.ShouldEqual(Operator.OperatorType.NotEqual);
            select.Where.RightOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            select.Where.RightOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);
        }

        [Test]
        public void No_Where_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Take(10);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.HasWhere.ShouldEqual(false);
        }

        [Test]
        public void OrderBy_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.OrderBy(x => x.Name.Substring(0, 5)).OrderBy(x => x.Age);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.OrderBy.ShouldNotBeNull();
            select.HasOrderBy.ShouldEqual(true);
            select.OrderBy.Count.ShouldEqual(2);
            select.OrderBy.All(x => x.Order == Order.Ascending).ShouldEqual(true);
        }

        [Test]
        public void OrderBy_Descending_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.OrderByDescending(x => x.Name.Substring(0, 5)).OrderByDescending(x => x.Age);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.OrderBy.ShouldNotBeNull();
            select.HasOrderBy.ShouldEqual(true);
            select.OrderBy.Count.ShouldEqual(2);
            select.OrderBy.All(x => x.Order == Order.Descending).ShouldEqual(true);
        }

        [Test]
        public void Copy_To_Query_Test()
        {
            var query1 = MockQueryable<Entity>.Create(TableName1);
            query1.CopyTo(MockQueryable<Entity>.Create(TableName2));
            var query2 = QueryVisitor<Entity>.CreateModel(query1.Expression, x => ((MockQueryable<Entity>)x).Name);

            query2.ShouldNotBeNull();
            query2.CopyTo.Into.ShouldNotBeNull();
            query2.CopyTo.Into.Name.ShouldEqual(TableName2);
            query2.CopyTo.Query.From.Type.ShouldEqual(Data.DataType.Table);
            query2.CopyTo.Query.From.Table.ShouldNotBeNull();
            query2.CopyTo.Query.From.Table.Name.ShouldEqual(TableName1);
        }

        [Test]
        public void Distinct_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);
            query.Distinct(x => x.Name.Substring(0, 5)).Distinct(x => x.Age);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.Distinct.ShouldNotBeNull();
            select.HasDistinct.ShouldEqual(true);
            select.Distinct.Count.ShouldEqual(2);

            select.Distinct[0].Projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            select.Distinct[0].Projection.Function.ShouldNotBeNull();
            select.Distinct[0].Projection.Function.Type.ShouldEqual(Function.FunctionType.SubstringFixed);

            select.Distinct[0].Projection.Function.SubstringFixed.Text.ShouldNotBeNull();
            select.Distinct[0].Projection.Function.SubstringFixed.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.Distinct[0].Projection.Function.SubstringFixed.Text.Field.ShouldNotBeNull();
            select.Distinct[0].Projection.Function.SubstringFixed.Text.Field.Name.ShouldEqual("Name");

            select.Distinct[0].Projection.Function.SubstringFixed.Start.ShouldNotBeNull();
            select.Distinct[0].Projection.Function.SubstringFixed.Start.Type.ShouldEqual(Projection.ProjectionType.Constant);
            select.Distinct[0].Projection.Function.SubstringFixed.Start.Constant.ShouldNotBeNull();
            select.Distinct[0].Projection.Function.SubstringFixed.Start.Constant.Value.ShouldEqual(0);

            select.Distinct[0].Projection.Function.SubstringFixed.Length.ShouldNotBeNull();
            select.Distinct[0].Projection.Function.SubstringFixed.Length.Type.ShouldEqual(Projection.ProjectionType.Constant);
            select.Distinct[0].Projection.Function.SubstringFixed.Length.Constant.ShouldNotBeNull();
            select.Distinct[0].Projection.Function.SubstringFixed.Length.Constant.Value.ShouldEqual(5);

            select.Distinct[1].ShouldNotBeNull();
            select.Distinct[1].Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.Distinct[1].Projection.Field.ShouldNotBeNull();
            select.Distinct[1].Projection.Field.Name.ShouldEqual("Age");
        }

        [Test]
        public void Intersect_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);

            query.Where(x => x.Age == 33).Intersect(MockQueryable<Entity>.Create(TableName2).
                                                    Intersect(MockQueryable<Entity>.Create(TableName3).Where(x => x.Active), x => x.Id), x => x.Name, x => x.Length).
                  Intersect(MockQueryable<Entity>.Create(TableName4).Take(5), x => x.Age);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.From.Type.ShouldEqual(Data.DataType.Table);
            select.From.Table.Name.ShouldEqual(TableName1);
            select.HasIntersections.ShouldEqual(true);
            select.SetOperatons.Count.ShouldEqual(2);

            select.SetOperatons[0].Type.ShouldEqual(SetOperation.OperationType.Intersect);
            select.SetOperatons[0].Select.From.Table.ShouldNotBeNull();
            select.SetOperatons[0].Select.From.Table.Name.ShouldEqual(TableName4);
            select.SetOperatons[0].Select.HasWhere.ShouldEqual(true);
            select.SetOperatons[0].Select.Where.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[0].Select.Where.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[0].Select.Where.LeftOperand.Projection.Field.Name.ShouldEqual("Age");
            select.SetOperatons[0].Select.Where.LeftOperand.Projection.Field.TableAlias.ShouldEqual(null);
            select.SetOperatons[0].Select.Where.Type.ShouldEqual(Operator.OperatorType.Equal);
            select.SetOperatons[0].Select.Where.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[0].Select.Where.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[0].Select.Where.RightOperand.Projection.Field.Name.ShouldEqual("Age");
            select.SetOperatons[0].Select.Where.RightOperand.Projection.Field.TableAlias.ShouldEqual(select.From.Alias);

            select.SetOperatons[1].Type.ShouldEqual(SetOperation.OperationType.Intersect);
            select.SetOperatons[1].Select.From.Table.ShouldNotBeNull();
            select.SetOperatons[1].Select.From.Table.Name.ShouldEqual(TableName2);
            select.SetOperatons[1].Select.HasWhere.ShouldEqual(true);
            select.SetOperatons[1].Select.Where.LeftOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.LeftOperand.Projection.Field.Name.ShouldEqual("Name");
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.LeftOperand.Projection.Field.TableAlias.ShouldEqual(null);
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.RightOperand.Projection.Field.Name.ShouldEqual("Name");
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.RightOperand.Projection.Field.TableAlias.ShouldEqual(select.From.Alias);
            select.SetOperatons[1].Select.Where.Type.ShouldEqual(Operator.OperatorType.And);
            select.SetOperatons[1].Select.Where.RightOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            select.SetOperatons[1].Select.Where.RightOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.Where.RightOperand.Operator.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[1].Select.Where.RightOperand.Operator.LeftOperand.Projection.Field.Name.ShouldEqual("Length");
            select.SetOperatons[1].Select.Where.RightOperand.Operator.LeftOperand.Projection.Field.TableAlias.ShouldEqual(null);
            select.SetOperatons[1].Select.Where.RightOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);
            select.SetOperatons[1].Select.Where.RightOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.Where.RightOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[1].Select.Where.RightOperand.Operator.RightOperand.Projection.Field.Name.ShouldEqual("Length");
            select.SetOperatons[1].Select.Where.RightOperand.Operator.RightOperand.Projection.Field.TableAlias.ShouldEqual(select.From.Alias);

            select.SetOperatons[1].Select.HasIntersections.ShouldEqual(true);
            select.SetOperatons[1].Select.SetOperatons.Count.ShouldEqual(1);

            select.SetOperatons[1].Select.SetOperatons[0].Type.ShouldEqual(SetOperation.OperationType.Intersect);
            select.SetOperatons[1].Select.SetOperatons[0].Select.From.Table.ShouldNotBeNull();
            select.SetOperatons[1].Select.SetOperatons[0].Select.From.Table.Name.ShouldEqual(TableName3);

            select.SetOperatons[1].Select.SetOperatons[0].Select.HasWhere.ShouldEqual(true);

            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.LeftOperand.Projection.Field.Name.ShouldEqual("Active");
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.LeftOperand.Projection.Field.TableAlias.ShouldEqual(null);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.RightOperand.Projection.Constant.Value.ShouldEqual(true);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.Type.ShouldEqual(Operator.OperatorType.And);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.LeftOperand.Projection.Field.Name.ShouldEqual("Id");
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.LeftOperand.Projection.Field.TableAlias.ShouldEqual(null);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.RightOperand.Projection.Field.Name.ShouldEqual("Id");
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.RightOperand.Projection.Field.TableAlias.ShouldEqual(select.SetOperatons[1].Select.From.Alias);
        }

        [Test]
        public void Except_Test()
        {
            var query = MockQueryable<Entity>.Create(TableName1);

            query.Where(x => x.Age == 33).Except(MockQueryable<Entity>.Create(TableName2).
                                                    Except(MockQueryable<Entity>.Create(TableName3).Where(x => x.Active), x => x.Id), x => x.Name, x => x.Length).
                  Except(MockQueryable<Entity>.Create(TableName4).Take(5), x => x.Age);
            var select = QueryVisitor<Entity>.CreateModel(query.Expression, x => ((MockQueryable<Entity>)x).Name).Select;

            select.ShouldNotBeNull();
            select.From.Type.ShouldEqual(Data.DataType.Table);
            select.From.Table.Name.ShouldEqual(TableName1);
            select.HasCompliments.ShouldEqual(true);
            select.SetOperatons.Count.ShouldEqual(2);

            select.SetOperatons[0].Type.ShouldEqual(SetOperation.OperationType.Compliment);
            select.SetOperatons[0].Select.From.Table.ShouldNotBeNull();
            select.SetOperatons[0].Select.From.Table.Name.ShouldEqual(TableName4);
            select.SetOperatons[0].Select.HasWhere.ShouldEqual(true);
            select.SetOperatons[0].Select.Where.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[0].Select.Where.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[0].Select.Where.LeftOperand.Projection.Field.Name.ShouldEqual("Age");
            select.SetOperatons[0].Select.Where.LeftOperand.Projection.Field.TableAlias.ShouldEqual(null);
            select.SetOperatons[0].Select.Where.Type.ShouldEqual(Operator.OperatorType.Equal);
            select.SetOperatons[0].Select.Where.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[0].Select.Where.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[0].Select.Where.RightOperand.Projection.Field.Name.ShouldEqual("Age");
            select.SetOperatons[0].Select.Where.RightOperand.Projection.Field.TableAlias.ShouldEqual(select.From.Alias);

            select.SetOperatons[1].Type.ShouldEqual(SetOperation.OperationType.Compliment);
            select.SetOperatons[1].Select.From.Table.ShouldNotBeNull();
            select.SetOperatons[1].Select.From.Table.Name.ShouldEqual(TableName2);
            select.SetOperatons[1].Select.HasWhere.ShouldEqual(true);
            select.SetOperatons[1].Select.Where.LeftOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.LeftOperand.Projection.Field.Name.ShouldEqual("Name");
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.LeftOperand.Projection.Field.TableAlias.ShouldEqual(null);
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.RightOperand.Projection.Field.Name.ShouldEqual("Name");
            select.SetOperatons[1].Select.Where.LeftOperand.Operator.RightOperand.Projection.Field.TableAlias.ShouldEqual(select.From.Alias);
            select.SetOperatons[1].Select.Where.Type.ShouldEqual(Operator.OperatorType.And);
            select.SetOperatons[1].Select.Where.RightOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            select.SetOperatons[1].Select.Where.RightOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.Where.RightOperand.Operator.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[1].Select.Where.RightOperand.Operator.LeftOperand.Projection.Field.Name.ShouldEqual("Length");
            select.SetOperatons[1].Select.Where.RightOperand.Operator.LeftOperand.Projection.Field.TableAlias.ShouldEqual(null);
            select.SetOperatons[1].Select.Where.RightOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);
            select.SetOperatons[1].Select.Where.RightOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.Where.RightOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[1].Select.Where.RightOperand.Operator.RightOperand.Projection.Field.Name.ShouldEqual("Length");
            select.SetOperatons[1].Select.Where.RightOperand.Operator.RightOperand.Projection.Field.TableAlias.ShouldEqual(select.From.Alias);

            select.SetOperatons[1].Select.HasCompliments.ShouldEqual(true);
            select.SetOperatons[1].Select.SetOperatons.Count.ShouldEqual(1);

            select.SetOperatons[1].Select.SetOperatons[0].Type.ShouldEqual(SetOperation.OperationType.Compliment);
            select.SetOperatons[1].Select.SetOperatons[0].Select.From.Table.ShouldNotBeNull();
            select.SetOperatons[1].Select.SetOperatons[0].Select.From.Table.Name.ShouldEqual(TableName3);

            select.SetOperatons[1].Select.SetOperatons[0].Select.HasWhere.ShouldEqual(true);

            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.LeftOperand.Projection.Field.Name.ShouldEqual("Active");
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.LeftOperand.Projection.Field.TableAlias.ShouldEqual(null);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.LeftOperand.Operator.RightOperand.Projection.Constant.Value.ShouldEqual(true);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.Type.ShouldEqual(Operator.OperatorType.And);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.LeftOperand.Projection.Field.Name.ShouldEqual("Id");
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.LeftOperand.Projection.Field.TableAlias.ShouldEqual(null);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.RightOperand.Projection.Field.Name.ShouldEqual("Id");
            select.SetOperatons[1].Select.SetOperatons[0].Select.Where.RightOperand.Operator.RightOperand.Projection.Field.TableAlias.ShouldEqual(select.SetOperatons[1].Select.From.Alias);
        }
    }
}

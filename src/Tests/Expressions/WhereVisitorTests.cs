using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gribble.Expressions;
using Gribble.Statements;
using NUnit.Framework;
using Should;

namespace Tests.Expressions
{
    [TestFixture]
    public class WhereVisitorTests
    {
        public class Entity
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string NickName { get; set; }
            public DateTime Birthdate { get; set; }
            public DateTime? Created { get; set; }
            public int Age { get; set; }
            public float Price { get; set; }
            public double Distance { get; set; }
            public byte Flag { get; set; }
            public bool Enabled { get; set; }
            public bool Active { get; set; }
            public decimal Length { get; set; }
            public long Miles { get; set; }
            public Dictionary<string, object> Values { get; set; }
            public Dictionary<string, bool> Flags { get; set; }
        }

        [Test]
        public void Equals_Constant_Test()
        {
            Expression<Func<Entity, bool>> where = x => x.Name == "Jeff";

            var whereClause = WhereVisitor<Entity>.CreateModel(where.Body);

            whereClause.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);

            whereClause.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
        }

        [Test]
        public void Equals_Boolean_Expression_Test()
        {
            Expression<Func<Entity, bool>> where = x => x.Active;
            var whereClause = WhereVisitor<Entity>.CreateModel(where.Body);

            whereClause.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);

            whereClause.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.RightOperand.Projection.Constant.Value.ShouldEqual(true);
        }

        [Test]
        public void Double_Boolean_Expression_Test()
        {
            Expression<Func<Entity, bool>> where = x => x.Active || x.Enabled;
            var whereClause = WhereVisitor<Entity>.CreateModel(where.Body);

            whereClause.LeftOperand.Type.ShouldEqual(Operand.OperandType.Operator);

            whereClause.LeftOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);

            whereClause.LeftOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.LeftOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.LeftOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.LeftOperand.Operator.RightOperand.Projection.Constant.Value.ShouldEqual(true);

            whereClause.Type.ShouldEqual(Operator.OperatorType.Or);

            whereClause.RightOperand.Type.ShouldEqual(Operand.OperandType.Operator);

            whereClause.RightOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);

            whereClause.RightOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.RightOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.RightOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.RightOperand.Operator.RightOperand.Projection.Constant.Value.ShouldEqual(true);
        }

        [Test]
        public void Boolean_Not_Expression_Test()
        {
            Expression<Func<Entity, bool>> where = x => !x.Active;
            var whereClause = WhereVisitor<Entity>.CreateModel(where.Body);

            whereClause.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);

            whereClause.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.RightOperand.Projection.Constant.Value.ShouldEqual(false);
        }

        [Test]
        public void Equals_Entity_Property_Test()
        {
            var entity = new Entity {Name = "yada"};
            Expression<Func<Entity, bool>> where = x => x.Name == entity.Name;

            var whereClause = WhereVisitor<Entity>.CreateModel(where.Body);

            whereClause.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);

            whereClause.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
        }

        [Test]
        public void Equals_Boolean_Not_Equals_Test()
        {
            Expression<Func<Entity, bool>> where = x => x.Active == false;

            var whereClause = WhereVisitor<Entity>.CreateModel(where.Body);

            whereClause.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);

            whereClause.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
        }

        [Test]
        public void Equals_Boolean_Not_Expression_Equals_Test()
        {
            Expression<Func<Entity, bool>> where = x => !x.Active == false;
            var whereClause = WhereVisitor<Entity>.CreateModel(where.Body);

            whereClause.LeftOperand.Type.ShouldEqual(Operand.OperandType.Operator);

            whereClause.LeftOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);

            whereClause.LeftOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.LeftOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.LeftOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.LeftOperand.Operator.RightOperand.Projection.Constant.Value.ShouldEqual(false);

            whereClause.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
        }

        [Test]
        public void Boolean_Dictionary_Expression_Test()
        {
            Expression<Func<Entity, bool>> where = x => x.Flags["Disabled"];
            var whereClause = WhereVisitor<Entity>.CreateModel(where.Body);

            whereClause.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);

            whereClause.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.RightOperand.Projection.Constant.Value.ShouldEqual(true);
        }

        [Test]
        public void Boolean_Object_Dictionary_Expression_Test()
        {
            Expression<Func<Entity, bool>> where = x => (bool)x.Values["Disabled"];
            var whereClause = WhereVisitor<Entity>.CreateModel(where.Body);

            whereClause.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);

            whereClause.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.RightOperand.Projection.Constant.Value.ShouldEqual(true);
        }

        [Test]
        public void Not_Boolean_Object_Dictionary_Expression_Test()
        {
            Expression<Func<Entity, bool>> where = x => !(bool)x.Values["Disabled"];
            var whereClause = WhereVisitor<Entity>.CreateModel(where.Body);

            whereClause.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);

            whereClause.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.RightOperand.Projection.Constant.Value.ShouldEqual(false);
        }

        [Test]
        public void Method_Boolean_Expression_Test()
        {
            Expression<Func<Entity, bool>> where = x => x.Name.Contains("t");
            var whereClause = WhereVisitor<Entity>.CreateModel(where.Body);

            whereClause.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);

            whereClause.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.RightOperand.Projection.Constant.Value.ShouldEqual(true);
        }

        [Test]
        public void Method_Boolean_Not_Expression_Test()
        {
            Expression<Func<Entity, bool>> where = x => !x.Name.Contains("t");
            var whereClause = WhereVisitor<Entity>.CreateModel(where.Body);

            whereClause.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);

            whereClause.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.RightOperand.Projection.Constant.Value.ShouldEqual(false);
        }

        [Test]
        public void Coalesce_Test()
        {
            Expression<Func<Entity, bool>> where = x => (x.Name ?? x.NickName) == "Jeff";

            var whereClause = WhereVisitor<Entity>.CreateModel(where.Body);

            whereClause.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);

            whereClause.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
        }

        [Test]
        public void Complex_Test()
        {
            var number1 = 45;
            var someKey = "OptOut";
            Expression<Func<Entity, bool>> where = x => ((!((x.Name ?? x.NickName) == "Jeff") && x.Age > AddValues(number1, "yada".Length)) || x.Name.Contains(x.NickName) && !(bool)x.Values[someKey]) == x.Flags["Bounce"] || x.Name.Substring(x.NickName.IndexOf("yada"), x.NickName.Length) == "yada";

            //                  Coalesce(x.Name, x.NickName) 
            //                  == 
            //                  "Jeff"
            //             == 
            //                  false 
            //          && 
            //              x.Price > 49
            //      || 
            //              Contains(Name, NickName)
            //              == 
            //              true 
            //         && 
            //              Convert(someKey, bool) == false
            //      == 
            //      Bounce 
            // || 
            //         Substring(
            //            Name,
            //            IndexOf(
            //                NickName, 
            //                "yada"), 
            //            Length(NickName)) 
            //         == 
            //              "yada";

            var whereClause = WhereVisitor<Entity>.CreateModel(where.Body);

            whereClause.LeftOperand.Type.ShouldEqual(Operand.OperandType.Operator);

            whereClause.LeftOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Operator);

            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Projection.Function.Type.ShouldEqual(Function.FunctionType.Coalesce);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Projection.Function.Coalesce.First.Type.ShouldEqual(Projection.ProjectionType.Field);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Projection.Function.Coalesce.First.Field.Name.ShouldEqual("Name");
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Projection.Function.Coalesce.First.Field.HasKey.ShouldEqual(false);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Projection.Function.Coalesce.Second.Type.ShouldEqual(Projection.ProjectionType.Field);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Projection.Function.Coalesce.Second.Field.Name.ShouldEqual("NickName");
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Projection.Function.Coalesce.Second.Field.HasKey.ShouldEqual(false);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Projection.Constant.Value.ShouldEqual("Jeff");

            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Projection.Constant.Value.ShouldEqual(false);

            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.Type.ShouldEqual(Operator.OperatorType.And);

            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Projection.Field.Name.ShouldEqual("Age");
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Projection.Field.HasKey.ShouldEqual(false);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.Type.ShouldEqual(Operator.OperatorType.GreaterThan);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Projection.Constant.Value.ShouldEqual(49);

            whereClause.LeftOperand.Operator.LeftOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Or);

            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Operator.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Operator.LeftOperand.Projection.Function.Type.ShouldEqual(Function.FunctionType.Contains);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Operator.LeftOperand.Projection.Function.Contains.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Operator.LeftOperand.Projection.Function.Contains.Text.Field.Name.ShouldEqual("Name");
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Operator.LeftOperand.Projection.Function.Contains.Value.Type.ShouldEqual(Projection.ProjectionType.Field);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Operator.LeftOperand.Projection.Function.Contains.Value.Field.Name.ShouldEqual("NickName");
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.LeftOperand.Operator.RightOperand.Projection.Constant.Value.ShouldEqual(true);

            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.Type.ShouldEqual(Operator.OperatorType.And);

            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Operator.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Operator.LeftOperand.Projection.Function.Type.ShouldEqual(Function.FunctionType.Convert);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Operator.LeftOperand.Projection.Function.Convert.Value.Type.ShouldEqual(Projection.ProjectionType.Field);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Operator.LeftOperand.Projection.Function.Convert.Value.Field.Name.ShouldEqual("Values");
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Operator.LeftOperand.Projection.Function.Convert.Value.Field.HasKey.ShouldEqual(true);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Operator.LeftOperand.Projection.Function.Convert.Value.Field.Key.ShouldEqual("OptOut");
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Operator.LeftOperand.Projection.Function.Convert.Type.ShouldEqual(typeof(bool));
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.LeftOperand.Operator.LeftOperand.Operator.RightOperand.Operator.RightOperand.Operator.RightOperand.Projection.Constant.Value.ShouldEqual(false);

            whereClause.LeftOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);

            whereClause.LeftOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.LeftOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            whereClause.LeftOperand.Operator.RightOperand.Projection.Field.Name.ShouldEqual("Flags");
            whereClause.LeftOperand.Operator.RightOperand.Projection.Field.HasKey.ShouldEqual(true);
            whereClause.LeftOperand.Operator.RightOperand.Projection.Field.Key.ShouldEqual("Bounce");

            whereClause.Type.ShouldEqual(Operator.OperatorType.Or);

            whereClause.RightOperand.Type.ShouldEqual(Operand.OperandType.Operator);
            whereClause.RightOperand.Operator.LeftOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.RightOperand.Operator.LeftOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.Type.ShouldEqual(Function.FunctionType.SubstringFixed);
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.SubstringFixed.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.SubstringFixed.Text.Field.Name.ShouldEqual("Name");
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.SubstringFixed.Text.Field.HasKey.ShouldEqual(false);
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.SubstringFixed.Start.Type.ShouldEqual(Projection.ProjectionType.Function);
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.SubstringFixed.Start.Function.Type.ShouldEqual(Function.FunctionType.IndexOf);
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.SubstringFixed.Start.Function.IndexOf.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.SubstringFixed.Start.Function.IndexOf.Text.Field.Name.ShouldEqual("NickName");
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.SubstringFixed.Start.Function.IndexOf.Text.Field.HasKey.ShouldEqual(false);
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.SubstringFixed.Start.Function.IndexOf.Value.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.SubstringFixed.Start.Function.IndexOf.Value.Constant.Value.ShouldEqual("yada");
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.SubstringFixed.Length.Type.ShouldEqual(Projection.ProjectionType.Function);
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.SubstringFixed.Length.Function.Type.ShouldEqual(Function.FunctionType.Length);
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.SubstringFixed.Length.Function.Length.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.SubstringFixed.Length.Function.Length.Text.Field.Name.ShouldEqual("NickName");
            whereClause.RightOperand.Operator.LeftOperand.Projection.Function.SubstringFixed.Length.Function.Length.Text.Field.HasKey.ShouldEqual(false);
            whereClause.RightOperand.Operator.Type.ShouldEqual(Operator.OperatorType.Equal);
            whereClause.RightOperand.Operator.RightOperand.Type.ShouldEqual(Operand.OperandType.Projection);
            whereClause.RightOperand.Operator.RightOperand.Projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            whereClause.RightOperand.Operator.RightOperand.Projection.Constant.Value.ShouldEqual("yada");
        }

        private static int AddValues(int a, int b) { return a + b; }
    }
}

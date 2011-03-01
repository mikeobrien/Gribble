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
    public class ProjectionVisitorTests
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

        public class Widget
        {
            public string Yada { get { return "oh hai"; } }
        }

        [Test]
        public void Constant_Test()
        {
            Expression<Func<Entity, string>> where = x => "Jeff";

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Constant.ShouldNotBeNull();
            projection.Constant.Value.ShouldEqual("Jeff");
        }

        [Test]
        public void Property_Test()
        {
            Expression<Func<Entity, DateTime>> where = x => x.Birthdate;

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Field.ShouldNotBeNull();
            projection.Field.Name.ShouldEqual("Birthdate");
            projection.Field.HasKey.ShouldEqual(false);
        }

        [Test]
        public void Dictionary_Property_Test()
        {
            Expression<Func<Entity, object>> where = x => x.Values["OptOut"];

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Field.ShouldNotBeNull();
            projection.Field.Name.ShouldEqual("Values");
            projection.Field.HasKey.ShouldEqual(true);
            projection.Field.Key.ShouldEqual("OptOut");
        }

        [Test]
        public void Conversion_Test()
        {
            Expression<Func<Entity, double>> where = x => (double)x.Age;

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.Convert);
            projection.Function.Convert.ShouldNotBeNull();
            projection.Function.Convert.Type.ShouldEqual(typeof(double));
            projection.Function.Convert.Value.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.Convert.Value.Field.Name.ShouldEqual("Age");
        }

        [Test]
        public void Coalesce_Test()
        {
            Expression<Func<Entity, string>> where = x => x.Name ?? x.NickName;

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.Coalesce);
            projection.Function.Coalesce.First.ShouldNotBeNull();
            projection.Function.Coalesce.First.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.Coalesce.First.Field.Name.ShouldEqual("Name");
            projection.Function.Coalesce.Second.ShouldNotBeNull();
            projection.Function.Coalesce.Second.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.Coalesce.Second.Field.Name.ShouldEqual("NickName");
        }

        [Test]
        public void String_Length_Test()
        {
            Expression<Func<Entity, int>> where = x => "Jeff".Length;

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.Length);
            projection.Function.Length.Text.ShouldNotBeNull();
            projection.Function.Length.Text.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Function.Length.Text.Constant.ShouldNotBeNull();
            projection.Function.Length.Text.Constant.Value.ShouldEqual("Jeff");
        }

        [Test]
        public void Non_Entity_Property_Access_Test()
        {
            var widget = new Widget();
            Expression<Func<Entity, string>> where = x => widget.Yada;

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Constant.ShouldNotBeNull();
            projection.Constant.Value.ShouldEqual(widget.Yada);
        }

        [Test]
        public void Starts_With_Test()
        {
            Expression<Func<Entity, bool>> where = x => x.Name.StartsWith("e");

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.StartsWith);

            projection.Function.StartsWith.Text.ShouldNotBeNull();
            projection.Function.StartsWith.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.StartsWith.Text.Field.ShouldNotBeNull();
            projection.Function.StartsWith.Text.Field.Name.ShouldEqual("Name");

            projection.Function.StartsWith.Value.ShouldNotBeNull();
            projection.Function.StartsWith.Value.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Function.StartsWith.Value.Constant.ShouldNotBeNull();
            projection.Function.StartsWith.Value.Constant.Value.ShouldEqual("e");
        }

        [Test]
        public void Contains_Test()
        {
            Expression<Func<Entity, bool>> where = x => x.Name.Contains("e");

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.Contains);

            projection.Function.Contains.Text.ShouldNotBeNull();
            projection.Function.Contains.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.Contains.Text.Field.ShouldNotBeNull();
            projection.Function.Contains.Text.Field.Name.ShouldEqual("Name");

            projection.Function.Contains.Value.ShouldNotBeNull();
            projection.Function.Contains.Value.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Function.Contains.Value.Constant.ShouldNotBeNull();
            projection.Function.Contains.Value.Constant.Value.ShouldEqual("e");
        }

        [Test]
        public void End_With_Test()
        {
            Expression<Func<Entity, bool>> where = x => x.Name.EndsWith("e");

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.EndsWith);

            projection.Function.EndsWith.Text.ShouldNotBeNull();
            projection.Function.EndsWith.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.EndsWith.Text.Field.ShouldNotBeNull();
            projection.Function.EndsWith.Text.Field.Name.ShouldEqual("Name");

            projection.Function.EndsWith.Value.ShouldNotBeNull();
            projection.Function.EndsWith.Value.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Function.EndsWith.Value.Constant.ShouldNotBeNull();
            projection.Function.EndsWith.Value.Constant.Value.ShouldEqual("e");
        }

        [Test]
        public void To_Lower_Test()
        {
            Expression<Func<Entity, string>> where = x => x.Name.ToLower();

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.ToLower);

            projection.Function.ToLower.Text.ShouldNotBeNull();
            projection.Function.ToLower.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.ToLower.Text.Field.ShouldNotBeNull();
            projection.Function.ToLower.Text.Field.Name.ShouldEqual("Name");
        }

        [Test]
        public void To_Upper_Test()
        {
            Expression<Func<Entity, string>> where = x => x.Name.ToUpper();

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.ToUpper);

            projection.Function.ToUpper.Text.ShouldNotBeNull();
            projection.Function.ToUpper.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.ToUpper.Text.Field.ShouldNotBeNull();
            projection.Function.ToUpper.Text.Field.Name.ShouldEqual("Name");
        }

        [Test]
        public void Trim_Test()
        {
            Expression<Func<Entity, string>> where = x => x.Name.Trim();

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.Trim);

            projection.Function.Trim.Text.ShouldNotBeNull();
            projection.Function.Trim.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.Trim.Text.Field.ShouldNotBeNull();
            projection.Function.Trim.Text.Field.Name.ShouldEqual("Name");
        }

        [Test]
        public void Trim_End_Test()
        {
            Expression<Func<Entity, string>> where = x => x.Name.TrimEnd();

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.TrimEnd);

            projection.Function.TrimEnd.Text.ShouldNotBeNull();
            projection.Function.TrimEnd.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.TrimEnd.Text.Field.ShouldNotBeNull();
            projection.Function.TrimEnd.Text.Field.Name.ShouldEqual("Name");
        }

        [Test]
        public void Trim_Start_Test()
        {
            Expression<Func<Entity, string>> where = x => x.Name.TrimStart();

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.TrimStart);

            projection.Function.TrimStart.Text.ShouldNotBeNull();
            projection.Function.TrimStart.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.TrimStart.Text.Field.ShouldNotBeNull();
            projection.Function.TrimStart.Text.Field.Name.ShouldEqual("Name");
        }

        [Test]
        public void To_String_Test()
        {
            Expression<Func<Entity, string>> where = x => x.Name.ToString();

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.ToString);

            projection.Function.ToString.Value.ShouldNotBeNull();
            projection.Function.ToString.Value.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.ToString.Value.Field.ShouldNotBeNull();
            projection.Function.ToString.Value.Field.Name.ShouldEqual("Name");
        }

        [Test]
        public void Sub_String_Test()
        {
            Expression<Func<Entity, string>> where = x => x.Name.Substring(0);

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.Substring);

            projection.Function.Substring.Text.ShouldNotBeNull();
            projection.Function.Substring.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.Substring.Text.Field.ShouldNotBeNull();
            projection.Function.Substring.Text.Field.Name.ShouldEqual("Name");

            projection.Function.Substring.Start.ShouldNotBeNull();
            projection.Function.Substring.Start.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Function.Substring.Start.Constant.ShouldNotBeNull();
            projection.Function.Substring.Start.Constant.Value.ShouldEqual(0);
        }

        [Test]
        public void Sub_String_Fixed_Test()
        {
            Expression<Func<Entity, string>> where = x => x.Name.Substring(0, 5);

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.SubstringFixed);

            projection.Function.SubstringFixed.Text.ShouldNotBeNull();
            projection.Function.SubstringFixed.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.SubstringFixed.Text.Field.ShouldNotBeNull();
            projection.Function.SubstringFixed.Text.Field.Name.ShouldEqual("Name");

            projection.Function.SubstringFixed.Start.ShouldNotBeNull();
            projection.Function.SubstringFixed.Start.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Function.SubstringFixed.Start.Constant.ShouldNotBeNull();
            projection.Function.SubstringFixed.Start.Constant.Value.ShouldEqual(0);

            projection.Function.SubstringFixed.Length.ShouldNotBeNull();
            projection.Function.SubstringFixed.Length.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Function.SubstringFixed.Length.Constant.ShouldNotBeNull();
            projection.Function.SubstringFixed.Length.Constant.Value.ShouldEqual(5);
        }

        [Test]
        public void Replace_Test()
        {
            Expression<Func<Entity, string>> where = x => x.Name.Replace("oh", "hai");

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.Replace);

            projection.Function.Replace.Text.ShouldNotBeNull();
            projection.Function.Replace.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.Replace.Text.Field.ShouldNotBeNull();
            projection.Function.Replace.Text.Field.Name.ShouldEqual("Name");

            projection.Function.Replace.SearchValue.ShouldNotBeNull();
            projection.Function.Replace.SearchValue.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Function.Replace.SearchValue.Constant.ShouldNotBeNull();
            projection.Function.Replace.SearchValue.Constant.Value.ShouldEqual("oh");

            projection.Function.Replace.ReplaceValue.ShouldNotBeNull();
            projection.Function.Replace.ReplaceValue.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Function.Replace.ReplaceValue.Constant.ShouldNotBeNull();
            projection.Function.Replace.ReplaceValue.Constant.Value.ShouldEqual("hai");
        }

        [Test]
        public void Insert_Test()
        {
            Expression<Func<Entity, string>> where = x => x.Name.Insert(5, "hai");

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.Insert);

            projection.Function.Insert.Text.ShouldNotBeNull();
            projection.Function.Insert.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.Insert.Text.Field.ShouldNotBeNull();
            projection.Function.Insert.Text.Field.Name.ShouldEqual("Name");

            projection.Function.Insert.Start.ShouldNotBeNull();
            projection.Function.Insert.Start.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Function.Insert.Start.Constant.ShouldNotBeNull();
            projection.Function.Insert.Start.Constant.Value.ShouldEqual(5);

            projection.Function.Insert.Value.ShouldNotBeNull();
            projection.Function.Insert.Value.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Function.Insert.Value.Constant.ShouldNotBeNull();
            projection.Function.Insert.Value.Constant.Value.ShouldEqual("hai");
        }

        [Test]
        public void Ondex_Of_Test()
        {
            Expression<Func<Entity, int>> where = x => x.Name.IndexOf("hai");

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.IndexOf);

            projection.Function.IndexOf.Text.ShouldNotBeNull();
            projection.Function.IndexOf.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.IndexOf.Text.Field.ShouldNotBeNull();
            projection.Function.IndexOf.Text.Field.Name.ShouldEqual("Name");

            projection.Function.IndexOf.Value.ShouldNotBeNull();
            projection.Function.IndexOf.Value.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Function.IndexOf.Value.Constant.ShouldNotBeNull();
            projection.Function.IndexOf.Value.Constant.Value.ShouldEqual("hai");
        }

        [Test]
        public void Ondex_Of_At_Test()
        {
            Expression<Func<Entity, int>> where = x => x.Name.IndexOf("hai", 10);

            var projection = ProjectionVisitor<Entity>.CreateModel(where.Body);

            projection.ShouldNotBeNull();
            projection.Type.ShouldEqual(Projection.ProjectionType.Function);
            projection.Function.ShouldNotBeNull();
            projection.Function.Type.ShouldEqual(Function.FunctionType.IndexOfAt);

            projection.Function.IndexOfAt.Text.ShouldNotBeNull();
            projection.Function.IndexOfAt.Text.Type.ShouldEqual(Projection.ProjectionType.Field);
            projection.Function.IndexOfAt.Text.Field.ShouldNotBeNull();
            projection.Function.IndexOfAt.Text.Field.Name.ShouldEqual("Name");

            projection.Function.IndexOfAt.Value.ShouldNotBeNull();
            projection.Function.IndexOfAt.Value.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Function.IndexOfAt.Value.Constant.ShouldNotBeNull();
            projection.Function.IndexOfAt.Value.Constant.Value.ShouldEqual("hai");

            projection.Function.IndexOfAt.Start.ShouldNotBeNull();
            projection.Function.IndexOfAt.Start.Type.ShouldEqual(Projection.ProjectionType.Constant);
            projection.Function.IndexOfAt.Start.Constant.ShouldNotBeNull();
            projection.Function.IndexOfAt.Start.Constant.Value.ShouldEqual(10);
        }
    }
}

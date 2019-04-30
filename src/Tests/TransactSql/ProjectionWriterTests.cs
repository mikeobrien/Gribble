using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Gribble;
using Gribble.Expressions;
using Gribble.Extensions;
using Gribble.Mapping;
using Gribble.TransactSql;
using NUnit.Framework;
using Should;

namespace Tests.TransactSql
{
    [TestFixture]
    public class ProjectionWriterTests
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
            public bool Active { get; set; }
            public decimal Length { get; set; }
            public long Miles { get; set; }
            public IDictionary<string, object> Values {get; set;}
        }

        public class EntityMap : ClassMap<Entity>
        {
            public EntityMap()
            {
                Id(x => x.Id).Column("id");
                Map(x => x.Name).Column("name");
                Map(x => x.NickName).Column("nickname");
                Map(x => x.Birthdate).Column("birthdate");
                Map(x => x.Created).Column("created");
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

        private static readonly EntityMapping Map = new EntityMapping(new EntityMap(), new[] { new ColumnMapping("optout", "OptOut"),
                                                                                               new ColumnMapping("count", "Count"),
                                                                                               new ColumnMapping("company", "Company"),
                                                                                               new ColumnMapping("companyname", "CompanyName"),
                                                                                               new ColumnMapping("pubcode", "PubCode"),
                                                                                               new ColumnMapping("createdate", "CreateDate")});

        [Test]
        public void Entity_Property_Test()
        {
            Expression<Func<Entity,int>> expression = x => x.Age;
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("[age]");
        }

        [Test]
        public void Entity_Property_Table_Alias_Test()
        {
            Expression<Func<Entity, int>> expression = x => x.Age;
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body, "T"), Map);
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("[T].[age]");
        }

        [Test]
        public void Entity_Nullable_Property_Test()
        {
            Expression<Func<Entity, DateTime?>> expression = x => x.Created;
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("[created]");
        }

        [Test]
        public void Entity_Dictionary_Property_Test()
        {
            Expression<Func<Entity, object>> expression = x => x.Values["Company"];
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("[company]");
        }

        [Test]
        public void Null_Test()
        {
            Expression<Func<Entity, object>> expression = x => null;
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("NULL");
        }

        [Test]
        public void String_Cast_Test()
        {
            Expression<Func<Entity, string>> expression = x => (string)x.Values["Company"];
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([company] AS nvarchar (MAX))");
        }

        [Test]
        public void Int_Cast_Test()
        {
            Expression<Func<Entity, int>> expression = x => (int)x.Values["PubCode"];
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([pubcode] AS int)");
        }

        [Test]
        public void DateTime_Cast_Test()
        {
            Expression<Func<Entity, DateTime>> expression = x => (DateTime)x.Values["CreateDate"];
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([createdate] AS datetime)");
        }

        [Test]
        public void Bool_Cast_Test()
        {
            Expression<Func<Entity, bool>> expression = x => (bool)x.Values["OptOut"];
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([optout] AS bit)");
        }

        [Test]
        public void Long_Cast_Test()
        {
            Expression<Func<Entity, long>> expression = x => (long)x.Values["Count"];
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([count] AS bigint)");
        }

        [Test]
        public void Decimal_Cast_Test()
        {
            Expression<Func<Entity, decimal>> expression = x => (decimal)x.Values["Count"];
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([count] AS decimal)");
        }

        [Test]
        public void Byte_Cast_Test()
        {
            Expression<Func<Entity, byte>> expression = x => (byte)x.Values["Count"];
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([count] AS tinyint)");
        }

        [Test]
        public void Double_Cast_Test()
        {
            Expression<Func<Entity, double>> expression = x => (double)x.Values["Count"];
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([count] AS float)");
        }

        [Test]
        public void Float_Cast_Test()
        {
            Expression<Func<Entity, float>> expression = x => (float)x.Values["Count"];
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([count] AS real)");
        }

        [Test]
        public void Guid_Cast_Test()
        {
            Expression<Func<Entity, Guid>> expression = x => (Guid)x.Values["Count"];
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([count] AS uniqueidentifier)");
        }

        [Test]
        public void Property_Length_Test()
        {
            Expression<Func<Entity, int>> expression = x => x.Name.Length;
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("LEN([name])");
        }

        [Test]
        public void Fields_Length_Test()
        {
            Expression<Func<Entity, int>> expression = x => ((string)x.Values["CompanyName"]).Length;
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("LEN(CAST([companyname] AS nvarchar (MAX)))");
        }

        [Test]
        public void ToUpper_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Name.ToUpper();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("UPPER([name])");
        }

        [Test]
        public void ToLower_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Name.ToLower();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("LOWER([name])");
        }

        [Test]
        public void Trim_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Name.Trim();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("LTRIM(RTRIM([name]))");
        }

        [Test]
        public void Trim_Left_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Name.TrimStart();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("LTRIM([name])");
        }

        [Test]
        public void Trim_Right_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Name.TrimEnd();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("RTRIM([name])");
        }

        [Test]
        public void Object_ToString_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Values["PubCode"].ToString();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([pubcode] AS nvarchar (MAX))");
        }

        [Test]
        public void Int_ToString_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Age.ToString();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([age] AS nvarchar (MAX))");
        }

        [Test]
        public void DateTime_ToString_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Birthdate.ToString();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([birthdate] AS nvarchar (MAX))");
        }

        [Test]
        public void Bool_ToString_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Active.ToString();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([active] AS nvarchar (MAX))");
        }

        [Test]
        public void Long_ToString_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Miles.ToString();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([miles] AS nvarchar (MAX))");
        }

        [Test]
        public void Decimal_ToString_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Length.ToString();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([length] AS nvarchar (MAX))");
        }

        [Test]
        public void Byte_ToString_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Flag.ToString();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([flag] AS nvarchar (MAX))");
        }

        [Test]
        public void Double_ToString_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Distance.ToString();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([distance] AS nvarchar (MAX))");
        }

        [Test]
        public void Float_ToString_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Price.ToString();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([price] AS nvarchar (MAX))");
        }

        [Test]
        public void Guid_ToString_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Id.ToString();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CAST([id] AS nvarchar (MAX))");
        }

        [Test]
        public void Hash_Md5_Test()
        {
            Expression<Func<Entity, byte[]>> expression = x => x.Name.Hash(EntityExtensions.HashAlgorithim.Md5);
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);

            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("HASHBYTES('Md5', [name])");
        }

        [Test]
        public void Hash_Sha1_Test()
        {
            Expression<Func<Entity, byte[]>> expression = x => x.Name.Hash(EntityExtensions.HashAlgorithim.Sha1);
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);

            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("HASHBYTES('Sha1', [name])");
        }

        [Test]
        public void To_Hex_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Name.Hash(EntityExtensions.HashAlgorithim.Md5).ToHex();
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);

            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("CONVERT(nvarchar (MAX), HASHBYTES('Md5', [name]), 1)");
        }

        [Test]
        public void Substring_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Name.Substring(10);
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual(10);
            statement.Text.ShouldEqual(string.Format("RIGHT([name], LEN([name]) - @{0})", statement.Parameters.First().Key));
        }

        [Test]
        public void Substring_Fixed_Length_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Name.Substring(10, 5);
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual(10);
            statement.Parameters.Skip(1).First().Value.ShouldEqual(5);
            statement.Text.ShouldEqual(string.Format("SUBSTRING([name], @{0}, @{1})", statement.Parameters.First().Key, statement.Parameters.Skip(1).First().Key));
        }

        [Test]
        public void Replace_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Name.Replace("this", "that");
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual("this");
            statement.Parameters.Skip(1).First().Value.ShouldEqual("that");
            statement.Text.ShouldEqual(string.Format("REPLACE([name], @{0}, @{1})", statement.Parameters.First().Key,    
                                                                               statement.Parameters.Skip(1).First().Key));
        }

        [Test]
        public void Insert_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Name.Insert(10, "that");
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual(10);
            statement.Parameters.Skip(1).First().Value.ShouldEqual("that");
            statement.Text.ShouldEqual(string.Format("STUFF([name], @{0}, LEN([name]) - @{0}, @{1})", statement.Parameters.First().Key,
                                                                                                statement.Parameters.Skip(1).First().Key));
        }

        [Test]
        public void Index_Of_Test()
        {
            Expression<Func<Entity, int>> expression = x => x.Name.IndexOf("hi");
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(1);
            statement.Parameters.First().Value.ShouldEqual("hi");
            statement.Text.ShouldEqual(string.Format("CHARINDEX(@{0}, [name])", statement.Parameters.First().Key));
        }

        [Test]
        public void Index_Of_Fixed_Length_Test()
        {
            Expression<Func<Entity, int>> expression = x => x.Name.IndexOf("hi", 5);
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(2);
            statement.Parameters.First().Value.ShouldEqual("hi");
            statement.Parameters.Skip(1).First().Value.ShouldEqual(5);
            statement.Text.ShouldEqual(string.Format("CHARINDEX(@{0}, [name], @{1})", statement.Parameters.First().Key,
                                                                                 statement.Parameters.Skip(1).First().Key));
        }

        [Test]
        public void Coalesce_Test()
        {
            Expression<Func<Entity, string>> expression = x => x.Name ?? x.NickName;
            var statement = ProjectionWriter<Entity>.CreateStatement(ProjectionVisitor<Entity>.CreateModel(expression.Body), Map);
            
            statement.Parameters.Count().ShouldEqual(0);
            statement.Text.ShouldEqual("COALESCE([name], [nickname])");
        }
    }
}

Gribble
=============

[![Nuget](http://img.shields.io/nuget/v/Gribble.svg?style=flat)](http://www.nuget.org/packages/Gribble/) [![TeamCity Build Status](https://img.shields.io/teamcity/http/build.mikeobrien.net/s/gribble.svg?style=flat)](http://build.mikeobrien.net/viewType.html?buildTypeId=gribble&guest=1)

<img src="https://raw.github.com/mikeobrien/Gribble/master/misc/logo.png"/> 

Gribble is a simple, Linq enabled ORM that was designed to work with dynamically created tables. It was not meant to be a replacement for a full fledged ORM like NHiberate but to handle a use case that other ORM's could not handle well. 

Here is the skinny:

* [Supports most Linq query operators plus additional query operators for copying/syncing data and querying duplicate/distinct records.](#query-operators).
* [Supports a number of additional extensions methods.](#extension-methods)
* Supports POCO's.
* [Supports column name aliases for dynamic fields.](#dynamic-mapping)
* [Simple fluent mapping API](#mapping) (shamelessly ripped off from [Fluent NHibernate](http://www.fluentnhibernate.org/)).
* Work with dynamic fields via dictionary property.
* [Create, modify and delete tables, columns and indexes.](#working-with-table-schema)
* [Execute raw SQL and map results to entites. Supports the `GO` keyword for batching.](#executing-sql-statements)
* [Execute stored procs and map results to entites.](#executing-stored-procedures)
* [NHibernate session/transaction integration.](#nhibernate-integration)
* [IoC Friendly](#ioc-configuration)
	
## Install

Gribble can be found on [nuget](https://www.nuget.org/):

    PM> Install-Package Gribble
    PM> Install-Package Gribble.NHibernate

## Overview

Gribble allows you to work with data through the `Table` class which implements `ITable<T>` and `IQueryable<T>`.

    public interface ITable<TEntity>: IOrderedQueryable<TEntity>, INamedQueryable
    {
        TEntity Get<T>(T id);
        void Insert(TEntity entity);
        void Update(TEntity entity);
        void Delete<T>(T id);
        void Delete(TEntity entity);
        void Delete(Expression<Func<TEntity, bool>> filter);
        int DeleteMany(Expression<Func<TEntity, bool>> filter);
        int DeleteMany(IQueryable<TEntity> source);
    }

Let's say we have the following dynamically created table where `Id`, `Street`, `City`, `State` and `Zip` are standard columns and all other columns are dynamic (e.g. `Code` and `Active`):

    CREATE TABLE Address_F2A74B 
    (
	    Id uniqueidentifier NOT NULL,
	    Street nvarchar(200) NOT NULL,
	    City nvarchar(200) NOT NULL,
	    State nvarchar(200) NOT NULL,
	    Zip nvarchar(200) NOT NULL,
	    Code varchar(3) NOT NULL,
	    Active bit NOT NULL
    )

For this table we create the following entity:

    public class Address 
    {
        public Guid Id { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public Dictionary<string, object> Values { get; set; }
    }
    
The `Values` property will allow us to get and set all non mapped fields (I refer to them as "dynamic" fields in this document). This property must be a `Dictionary<string, object>` where the key is the name of the field and the value is the value of the field. It is possible to create a mapping for the keys so that an alias can be used instead of the raw column name. This is discussed later.

#### Mapping

Gribble supports a few mapping options as discussed below.

##### Dynamic Entity

Out of the box you can simply use the built in `Entity<int>` or `Entity<Guid>` entity. This entity consists of an id property and a dictionary property and is automatically mapped. 

    public class Entity<TKey> 
    {
        public TKey Id { get; set; }
        public Dictionary<string, object> Values { get; set; }
    }

This is handy when you don't need to create an entity or mapping.

##### Implicit Mapping

By default Gribble uses the following conventions to implicitly map columns to public instance properties when no explicit mapping exists for an entity:

- The entity id is mapped to a column and property named `Id` of type `int` or `Guid` if it exists.
- Properties are mapped to columns of the same name.
- Dynamic values are mapped to a property that implements `IDictionary<string, object>` if it exists.

##### Explicit Mapping

Finally we can also map explicitly in the spirit of [James Gregory's FluentNHibernate](http://www.fluentnhibernate.org/):

    public class AddressMap : ClassMap<Address>
    {
        public AddressMap()
        {
            Id(x => x.Id).Column("YadaYadaId").Identity();
            Map(x => x.Street).Column("YadaYadaStreet");
            Map(x => x.City).Column("YadaYadaCity");
            Map(x => x.State);
            Map(x => x.Zip);
            Map(x => x.Values).Dynamic();
        }
    }
    
The Gribble fluent mapping works the same as FNH. If the column is omitted the property name is used as the column name. The `Id` mapping is only required when creating, modifying or deleting entities. If you will only be querying entities the `Id` mapping is not required. The `Generated()` flag tells Gribble that it will need to generate the id. In this case it will generate a Guid COMB. If the id is generated by the database, as in the case of an identity field or default value, this flag should be omitted. The `Dynamic()` flag this tells Gribble that the property will be a catch all bag for columns that are not mapped.

Note: Gribble also provides a stock entity (`Gribble.Entity<TKey>`) and class map (`Gribble.IntKeyEntityMap/GuidKeyEntityMap`) out of the box that only contains an `Id` and `Values` property. This is handy if you need to work with a table that is completely dynamic and do not want to create an entity and map. `Table` contains static factory methods, discussed next, that omit the mapping and will use the built in one (`Table.Create<TKey>(...)`).

#### Connection

We create a `Table` by passing in a connection manager, a class map and an optional profiler. You can create a `Table` with the new keyword or one of the static factory methods. There is a connection manager that takes a `System.Data.SqlConnection` or connection string and one that takes an `NHibernate.ISession` (When using NHibernate integration). 

    // Connection string and console profiler
    using (var connectionManager = new ConnectionManager("server=localhost...")) 
    {
        var table = new Table<Address>(connectionManager, "Address_F2A74B", new AddressMap(), new ConsoleProfiler());
        ...
    }

    // Existing connection
    using (var connection = new SqlConnection("server=localhost...")) 
    {
        connection.Open();
        var connectionManager = new ConnectionManager(connection);
        var table = new Table<Address>(connectionManager, "Address_F2A74B", new AddressMap());
        ...
    }

    // Implicit mapping
    using (var connection = new SqlConnection("server=localhost...")) 
    {
        connection.Open();
        var connectionManager = new ConnectionManager(connection);
        var table = new Table<Address>(connectionManager, "Address_F2A74B");
        ...
    }

    // NHibernate session
    using (var session = sessionFactory.OpenSession()) 
    {
        var connectionManager = new Gribble.NHibernate.ConnectionManager(session);
        var table = new Table<Address>(connectionManager, "Address_F2A74B", new AddressMap());
        ...
    }
    
    // Static factory method using built in entity and map. Requires key column name.
    using (var connectionManager = new ConnectionManager("server=localhost...")) 
    {
        var table = Table<Entity<Guid>>.Create<Guid>(connectionManager, "Address_F2A74B", "Id");
        ...
    }
    
#### Usage

Now that you have a `Table` you can query it:

    var results = table.Where(x => x.State == "CO" && x.Values["Active"]).ToList();

Dynamic values can be specified in the Linq query by passing in the name of the column as the key. Most of the query operators are supported including a few additional ones.

You can also get, add, modify, delete and delete many records:
    
    var address = new Address { Street = "123 Rainey Street", ... };
    table.Insert(address);
    Console.WriteLine("Created address with id: {0}", address.Id);

    address = table.Get(address.Id);
    Console.WriteLine("Street is {0}", address.Street);
    
    address.Street = "456 Rainey Street";
    table.Update(address);
    
    table.Delete(adress.Id);
    
    table.Delete(address)
    
    table.Delete(x => x.Values["Code"] == 12345);
    
    table.DeleteMany(x => x.State == "CO" && !x.Values["Active"]);
    
    table.DeleteMany(table.Duplicates(x => x.Values["Code"]));

#### Dynamic Mapping

In some cases you may want to map the raw column names to an alias at runtime. This may especially be so when allowing users to set values via an API. Lets say for example we had the following table:

    CREATE TABLE Address_F2A74B 
    (
	    Id uniqueidentifier NOT NULL,
	    Street nvarchar(200) NOT NULL,
	    City nvarchar(200) NOT NULL,
	    State nvarchar(200) NOT NULL,
	    Zip nvarchar(200) NOT NULL,
	    F2A74B_code varchar(3) NOT NULL,
	    F2A74B_active bit NOT NULL
    )

And a mapping stored somewhere like this (Like the users custom columns table) which map a column name to friendly alias:

    F2A74B_code = Code
    F2A74B_active = Active
    ...

Instead of referencing the dynamic fields like `address.Values["F2A74B_active"]` you can pass a mapping override that applies to dynamic fields and reference it like `address.Values["Active"]`:

    var dynamicColumnMapping = new [] { new ColumnMapping("F2A74B_code", "Code"), new ColumnMapping("F2A74B_active", "Active") };
    var entityMapping = new EntityMapping(new AddressMap(), dynamicColumnMapping);
    
    var table = new Table<Address>(connectionManager, "Address_F2A74B", entityMapping);
    
    var results = table.Where(x => x.State == "CO" && x.Values["Active"]).ToList();

Executing SQL Statements
------------

Gribble allows you to execute SQL statements through the `SqlStatement` class which implements `ISqlStatement`. Simple return types like numeric, `DateTime` and `Guid` are supported by all methods in addition to reference types.

    public interface ISqlStatement
    {
        int Execute(string commandText, object parameters = null); // Returns the number of records affected
        T ExecuteScalar<T>(string commandText, object parameters = null);
        TEntity ExecuteSingle<TEntity>(string commandText, object parameters = null);
        TEntity ExecuteSingleOrNone<TEntity>(string commandText, object parameters = null);
        IEnumerable<TEntity> ExecuteMany<TEntity>(string commandText, object parameters = null);
    }

We create a `SqlStatement` object by passing in a connection manager, an optional class map (Only used when returning entities) and an optional profiler. You can create a `SqlStatement` object with the new keyword or one of the static factory methods. There is a connection manager that takes a `System.Data.SqlConnection` or connection string and one that takes an `NHibernate.ISession` (When using NHibernate integration). Gribble supports the `GO` keyword and will automatically split the statement into seperate batches. It uses the last batch for return values, preceding batches are executed non query.

    // Connection string and console profiler
    using (var connectionManager = new ConnectionManager("server=localhost...")) 
    {
        var sqlStatement = SqlStatement.Create(connectionManager, profiler: new ConsoleProfiler());
        ...
    }

    // Existing connection with implicit mapping
    using (var connection = new SqlConnection("server=localhost...")) 
    {
        connection.Open();
        var connectionManager = new ConnectionManager(connection);
        var sqlStatement = SqlStatement.Create(connectionManager);
        ...
    }

    // Existing connection with explicit mapping
    using (var connection = new SqlConnection("server=localhost...")) 
    {
        connection.Open();
        var connectionManager = new ConnectionManager(connection);
        var mapping = new EntityMappingCollection(new IClassMap[] { new AddressMap() })
        var sqlStatement = SqlStatement.Create(connectionManager, mapping);
        ...
    }

    // NHibernate session
    using (var session = sessionFactory.OpenSession()) 
    {
        var connectionManager = new Gribble.NHibernate.ConnectionManager(session);
        var sqlStatement = SqlStatement.Create(connectionManager);
        ...
    }

SQL statement parameters are passed in as objects where the property name is the parameter name and the property value is the parameter value:

    var result = sqlStatement.ExecuteSingle<Entity>("SELECT * FROM Addresses WHERE Id = @Id", new { id = 5 });

Executing Stored Procedures
------------

Gribble allows you to execute stored procedures through the `StoredProcedure` class which implements `IStoredProcedure`. Simple return types like numeric, `DateTime` and `Guid` are supported by all methods in addition to reference types.

    public interface IStoredProcedure
    {
        bool Exists(string name); // Checks if the procedure exists
        TReturn Execute<TReturn>(string name, object parameters = null); // Returns the return value
        int Execute(string name, object parameters = null); // Returns the number of records affected
        T ExecuteScalar<T>(string name, object parameters = null);
        TEntity ExecuteSingle<TEntity>(string name, object parameters = null);
        TEntity ExecuteSingleOrNone<TEntity>(string name, object parameters = null);
        IEnumerable<TEntity> ExecuteMany<TEntity>(string name, object parameters = null);
    }

We create a `StoredProcedure` object by passing in a connection manager, an optional class map (Only used when returning entities) and an optional profiler. You can create a `StoredProcedure` object with the new keyword or one of the static factory methods. There is a connection manager that takes a `System.Data.SqlConnection` or connection string and one that takes an `NHibernate.ISession` (When using NHibernate integration). 

    // Connection string and console profiler
    using (var connectionManager = new ConnectionManager("server=localhost...")) 
    {
        var storedProcedure = StoredProcedure.Create(connectionManager, profiler: new ConsoleProfiler());
        ...
    }

    // Existing connection with implicit mapping
    using (var connection = new SqlConnection("server=localhost...")) 
    {
        connection.Open();
        var connectionManager = new ConnectionManager(connection);
        var storedProcedure = StoredProcedure.Create(connectionManager);
        ...
    }

    // Existing connection with explicit mapping
    using (var connection = new SqlConnection("server=localhost...")) 
    {
        connection.Open();
        var connectionManager = new ConnectionManager(connection);
        var mapping = new EntityMappingCollection(new IClassMap[] { new AddressMap() })
        var storedProcedure = StoredProcedure.Create(connectionManager, mapping);
        ...
    }

    // NHibernate session
    using (var session = sessionFactory.OpenSession()) 
    {
        var connectionManager = new Gribble.NHibernate.ConnectionManager(session);
        var storedProcedure = StoredProcedure.Create(connectionManager);
        ...
    }

Stored procedure parameters are passed in as objects where the property name is the parameter name and the property value is the parameter value:

    var result = storedProcedure.ExecuteSingle<Entity>("GetAddress", new { id = 5 });

Working with Table Schema
------------

Gribble allows you to work with table schema through the `TableSchema` class which implements `ITableSchema`.

    public interface ITableSchema
    {
        void CreateTable(string tableName, params Column[] columns);
        void CreateTable(string tableName, string modelTable);
        bool TableExists(string tableName);
        void DeleteTable(string tableName);

        IEnumerable<Column> GetColumns(string tableName);
        void AddColumn(string tableName, Column column);
        void AddColumns(string tableName, params Column[] columns);
        void RemoveColumn(string tableName, string columnName);

        IEnumerable<Index> GetIndexes(string tableName);
        void AddNonClusteredIndex(string tableName, params Index.Column[] columns);
        void AddNonClusteredIndexes(string tableName, params Index.ColumnSet[] indexColumns);
        void RemoveNonClusteredIndex(string tableName, string indexName);
    }

We create a `TableSchema` object by passing in a connection manager and an optional profiler. You can create a `TableSchema` object with the new keyword or one of the static factory methods. There is a connection manager that takes a `System.Data.SqlConnection` or connection string and one that takes an `NHibernate.ISession` (When using NHibernate integration). 

    // Connection string and console profiler
    using (var connectionManager = new ConnectionManager("server=localhost...")) 
    {
        var tableSchema = TableSchema.Create(connectionManager, profiler: new ConsoleProfiler());
        ...
    }

    // NHibernate session
    using (var session = sessionFactory.OpenSession()) 
    {
        var connectionManager = new Gribble.NHibernate.ConnectionManager(session);
        var tableSchema = TableSchema.Create(connectionManager);
        ...
    }

The TableSchema object allows you to do the following:

* Create a table: `CreateTable`
* Create a table using an existing table schema as a template: `CreateTable`
* Check if a table exists: `TableExists`
* Delete a table: `DeleteTable`
* Get table columns: `GetColumns`
* Add table columns: `AddColumn`, `AddColumns`
* Remove table columns: `RemoveColumn`
* Get table indexes: `GetIndexes`
* Add non clustered indexes: `AddNonClusteredIndex`, `AddNonClusteredIndexes`
* Remove non clustered index: `RemoveNonClusteredIndex`

NHibernate Integration
------------

Gribble integrates with NHibernate. More specifically it will use the NHibernate sql connection and enlist in an NHibernate transaction if one has been started. The integration is avaiable as a seperate assembly. The following example demonstrates how to use Gribble with NHibernate:

    using (var session = sessionFactory.OpenSession()) 
    {
        var connectionManager = new Gribble.NHibernate.ConnectionManager(session);
        var table = new Table<Address>(connectionManager, "Address_F2A74B", new AddressMap());
        var tableSchema = TableSchema.Create(connectionManager);
        ...
    }

IoC Configuration
------------
  
Gribble was designed to be IoC friendly. The following demonstrates how to configure StructureMap and Gribble with NHibernate:

    public class Registry : StructureMap.Configuration.DSL.Registry
    {
        public Registry()
        {
            // NHibernate registration
            ForSingletonOf<ISessionFactory>().
                Use(context => Fluently.Configure().
                    Database(MsSqlConfiguration.MsSql2008.ConnectionString("server=localhost...")).
                    Mappings(map => map.FluentMappings.AddFromAssembly(Assembly.GetExecutingAssembly()).Conventions.Add(AutoImport.Never())).
                    BuildConfiguration().
                    BuildSessionFactory());
            For<ISession>().Use(context => context.GetInstance<ISessionFactory>().OpenSession());
            
            // Gribble registration
            Scan(x => { x.TheCallingAssembly(); x.AddAllTypesOf<IClassMap>(); });
            ForSingletonOf<EntityMappingCollection>().Use<EntityMappingCollection>();
            For<IConnectionManager>().Use<Gribble.NHibernate.ConnectionManager>();
            For<ITableFactory>().Use<TableFactory>();
            For<ITableSchema>().Use<TableSchema>();
        }
    }
    
    public class Data
    {
        private ITableFactory _tableFactory;
        private ITableSchema _tableSchema;
        
        public Data(ITableFactory tableFactory, ITableSchema tableSchema) 
        {
            _tableFactory = tableFactory;
            _tableSchema = tableSchema;
        }
        
        public T GetRecord<T>(string tableName, object id) 
        {
            var table = _tableFactory.CreateFor<T>(tableName);
            return table.Get(id);
        }
        
        public IEnumerable<string> GetColumns(string tableName)
        {
            return _tableSchema.GetColumns(tableName);
        }
    }
    
    ObjectFactory.Initialize(x => x.AddRegistry<Registry>());
    using (var container = ObjectFactory.Container.GetNestedContainer()) 
    {
        var data = container.GetInstance<Data>();
        var result = data.GetRecord<Address>(id);
    }

Query Operators
------------

Gribble supports the following query operators:

`Any`, `Count`, `CopyTo`, `Single`, `First`, `FirstOrDefault`, `Distinct`, `Duplicates`, `Except`, `Intersect`, `OrderBy`, `OrderByDescending`, `Randomize`, `Skip`, `SyncWith`, `Take`, `TakePercent`, `Union`, `Where`

Gribble adds the following custom query operators.

Returns a random result set:

    IQueryable<TSource> Randomize<TSource>(this IQueryable<TSource> source)
    
Returns a top percentage of records:

    IQueryable<TSource> TakePercent<TSource>(this IQueryable<TSource> source, int percent)

Copys records from one table to another. Both the source and target must be an `ITable`:

    IQueryable<TSource> CopyTo<TSource>(this IQueryable<TSource> source, IQueryable<TSource> target)

Syncs the values of records based on a key. The records may be in the same table or seperate tables. You can specify the fields to include or exclude in the sync. Both the source and target must be an `ITable`:

    IQueryable<TTarget> SyncWith<TTarget, TKey>(this IQueryable<TTarget> target, IQueryable<TTarget> source, Expression<Func<TTarget, TKey>> keySelector,
        SyncFields syncFields, params Expression<Func<TTarget, object>>[] syncSelectors)

Returns a distinct result set based on the specified key:

    IQueryable<TSource> Distinct<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> selector)

Returns a distinct result set based on the specified key and sorted by a projection:

    IQueryable<TSource> Distinct<TSource, TKey, TOrder>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> selector,
        Expression<Func<TSource, TOrder>> orderSelector, Order order)

Returns duplicate records based on a key:

    IQueryable<TSource> Duplicates<TSource, TKey>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> selector)

Returns duplicate records based on a key and ordered by a projection or a predicate:

    IQueryable<TSource> Duplicates<TSource, TKey, TOrder>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> selector, 
        Expression<Func<TSource, TOrder>> orderSelector, Order order)

    IQueryable<TSource> Duplicates<TSource, TKey, TOrder1, TOrder2>(this IQueryable<TSource> source, Expression<Func<TSource, TKey>> selector, 
        Expression<Func<TSource, TOrder1>> orderSelector1, Order order1, Expression<Func<TSource, TOrder2>> orderSelector2, Order order2)

Returns the intersection of two queries based on the specified selectors:

    IQueryable<TSource> Intersect<TSource>(this IQueryable<TSource> source, IQueryable<TSource> compare, params Expression<Func<TSource, object>>[] selectors)

Returns the exception of two queries based on the specified selectors:

    IQueryable<TSource> Except<TSource>(this IQueryable<TSource> source, IQueryable<TSource> compare, params Expression<Func<TSource, object>>[] selectors)

All custom query operators are complemented with an equivalent `IEnumerable<T>` so that a memory backed collection can be substituted when testing.

Extension Methods
------------

Gribble supports the following extension methods:

`Contains`, `EndsWith`, `Hash`, `Insert`, `IndexOf`, `Replace`, `StartsWith`, `Substring`, `Trim`, `ToLower`, `ToString`, `ToUpper`, `TrimEnd`, `TrimStart`, `ToHex`

Gribble adds the following custom extension methods.

Creates either a md5 or sha1 hash of a value:

    enum HashAlgorithim { Md5, Sha1 }

    byte[] Hash(this string value, HashAlgorithim algorithm)

Converts a value to hex:

    string ToHex(this byte[] value)

Installation
------------

    PM> Install-Package gribble  
    PM> Install-Package gribble.nhibernate

Contributors
------------

| [![Guru Kathiresan](http://www.gravatar.com/avatar/af0a1c7a98f6584a90b8ed49708d6218.jpg?s=144)](https://github.com/gkathire) |
|:---:|
| [Guru Kathiresan](https://github.com/gkathire) |
	
Props
------------

Thanks to [JetBrains](http://www.jetbrains.com/) for providing OSS licenses!
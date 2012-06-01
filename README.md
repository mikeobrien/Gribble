Gribble
=============

Gribble is a simple, Linq enabled ORM that was designed to work with dynamically created tables. It was not meant to be a replacement for a full fledged ORM like NHiberate but to handle a use case that other ORM's could not handle well. 

Here is the skinny:

* Supports most Linq query operators.
* Supports POCO's.
* Simple fluent mapping API (shamelessly ripped off from FluentNHibernate).
* Only supports SQL Server.
* Create, modify and delete tables, columns and indexes.
* Execute stored procs and map results to entites.
* Additional query operators for copying/syncing data and querying duplicate/distinct records.
* Interfaced based so you can test against in memory collections.
* NHibernate session/transaction integration.
	
Distribution
------------

http://nuget.org/List/Packages/gribble  
http://nuget.org/List/Packages/gribble.nhibernate
	
Props
------------

Thanks to [JetBrains](http://www.jetbrains.com/) for providing OSS licenses!
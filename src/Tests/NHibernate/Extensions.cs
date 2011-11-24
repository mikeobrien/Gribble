using System.Data;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;

namespace Tests.NHibernate
{
    public static class Extensions
    {
        public static global::NHibernate.Cfg.Configuration AutoQuote(this global::NHibernate.Cfg.Configuration configuration)
        {
            configuration.SetProperty("hbm2ddl.keywords", "auto-quote");
            return configuration;
        }

        public static FluentConfiguration Sql2008Database(this FluentConfiguration configuration, string connectionString, IsolationLevel isolationLevel, bool showSql)
        {
            var persistenceConfigurer = MsSqlConfiguration.MsSql2008.ConnectionString(connectionString).IsolationLevel(isolationLevel);
            if (showSql) persistenceConfigurer.ShowSql().FormatSql();
            return configuration.Database(persistenceConfigurer);
        }

        public static global::NHibernate.Cfg.Configuration CommandTimeout(this global::NHibernate.Cfg.Configuration configuration, int seconds)
        {
            configuration.SetProperty("command_timeout", seconds.ToString());
            return configuration;
        }
    }
}

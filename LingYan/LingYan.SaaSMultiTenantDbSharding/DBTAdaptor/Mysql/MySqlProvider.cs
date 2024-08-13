using LingYan.DynamicShardingDBT.DBTContext;
using LingYan.DynamicShardingDBT.DBTProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Migrations;
using MySqlConnector;
using System.Data.Common;

namespace LingYan.DynamicShardingDBT.DBTAdaptor.Mysql
{
    internal class MySqlProvider : DynamicDBTProvider
    {
        public override DbProviderFactory DbProviderFactory { get; } = MySqlConnectorFactory.Instance;
        public override ModelBuilder GetModelBuilder()
        {
            return new ModelBuilder(MySqlConventionSetBuilder.Build());
        }

        public override IDynamicDBTService GetDynamicDBTService(DynamicDbContext baseDbContext)
        {
            return new MySqlDBTService(baseDbContext);
        }

        public override void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection)
        {
            Action<MySqlDbContextOptionsBuilder> mySqlOptionsAction = x => x.UseNetTopologySuite();

            dbContextOptionsBuilder.UseMySql(dbConnection, MySqlServerVersion.LatestSupportedServerVersion, mySqlOptionsAction);
            dbContextOptionsBuilder.ReplaceService<IMigrationsSqlGenerator, ShardingMySqlMigrationsSqlGenerator>();
        }
    }
}

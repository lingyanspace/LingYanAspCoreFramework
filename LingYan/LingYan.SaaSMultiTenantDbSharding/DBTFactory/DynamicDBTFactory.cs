using LingYan.DynamicShardingDBT.DBTAdaptor.Mysql;
using LingYan.DynamicShardingDBT.DBTContext;
using LingYan.DynamicShardingDBT.DBTExtension;
using LingYan.DynamicShardingDBT.DBTModel;
using LingYan.DynamicShardingDBT.DBTProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data.Common;
using System.Reflection;

namespace LingYan.DynamicShardingDBT.DBTFactory
{
    public class DynamicDBTFactory: IDynamicDBTFactory
    {
        private readonly ILoggerFactory _logf;
        private readonly IOptionsMonitor<DynamicDBTOption> _doptMonitor;
        private readonly IServiceProvider _app;
        public DynamicDBTFactory(ILoggerFactory loggerFactory, IOptionsMonitor<DynamicDBTOption> optMonitor, IServiceProvider app)
        {
            _logf = loggerFactory;
            _doptMonitor = optMonitor;
            _app = app;
        }

        public void CreateTable(string conString, DynamicDBType dbType, Type entityType, string suffix)
        {
            DynamicDBCParamater options = new DynamicDBCParamater
            {
                ConnectionString = conString,
                DynamicDatabase = dbType,
                EntityTypes = new Type[] { entityType },
                Suffix = suffix
            };

            using DbContext dbContext = GetDbContext(options, _doptMonitor.BuildOption(null));
            var databaseCreator = dbContext.Database.GetService<IDatabaseCreator>() as RelationalDatabaseCreator;
            try
            {
                databaseCreator.CreateTables();
            }
            catch
            {

            }
        }
         
        public IDynamicDBTService GetDBTService(DynamicDBCParamater dynamicDBCParamater, string optionName = null) 
        {
            DynamicDBTOption eFCoreShardingOptions = _doptMonitor.BuildOption(optionName);

            var dbContext = GetDbContext(dynamicDBCParamater, eFCoreShardingOptions);

            return GetProvider(dynamicDBCParamater.DynamicDatabase).GetDynamicDBTService(dbContext);
        }

        public DynamicDbContext GetDbContext(DynamicDBCParamater dynamicDBCParamater, DynamicDBTOption dynamicDBTOption) 
        {
            if (dynamicDBTOption == null)
            {
                dynamicDBTOption = _doptMonitor.BuildOption(null);
            }

            DynamicDBTProvider provider = GetProvider(dynamicDBCParamater.DynamicDatabase);

            DbConnection dbConnection = provider.GetDbConnection();
            dbConnection.ConnectionString = dynamicDBCParamater.ConnectionString;

            DbContextOptionsBuilder builder = new DbContextOptionsBuilder();
            builder.UseLoggerFactory(_logf);

            provider.UseDatabase(builder, dbConnection);
            builder.ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>();
            builder.ReplaceService<IMigrationsModelDiffer, DynamicMigrationsModelDiffer>();

            return new DynamicDbContext(builder.Options, dynamicDBCParamater, dynamicDBTOption, _app);
        }

        public static DynamicDBTProvider GetProvider(DynamicDBType databaseType)
        {
            try
            {
                switch (databaseType)
                {
                    case DynamicDBType.MySql:
                        return new MySqlProvider();                       
                    default:
                        return null;
                }
            }
            catch(Exception ex)
            {
                throw new Exception($"请安装nuget包:{ex.Message}");
            }
        }
    }
}

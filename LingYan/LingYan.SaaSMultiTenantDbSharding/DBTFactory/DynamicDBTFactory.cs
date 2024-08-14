using LingYan.DynamicShardingDBT.DBTAdaptor.Mysql;
using LingYan.DynamicShardingDBT.DBTCache;
using LingYan.DynamicShardingDBT.DBTContext;
using LingYan.DynamicShardingDBT.DBTExtension;
using LingYan.DynamicShardingDBT.DBTHelper;
using LingYan.DynamicShardingDBT.DBTModel;
using LingYan.DynamicShardingDBT.DBTProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data.Common;

namespace LingYan.DynamicShardingDBT.DBTFactory
{
    public class DynamicDBTFactory : IDynamicDBTFactory
    {
        private readonly ILoggerFactory _logf;
        private readonly IOptionsMonitor<DynamicDBTOption> _doptMonitor;
        private readonly IServiceProvider _app;
        //todo12
        public DynamicDBTFactory(ILoggerFactory loggerFactory, IOptionsMonitor<DynamicDBTOption> optMonitor, IServiceProvider app)
        {
            _logf = loggerFactory;
            _doptMonitor = optMonitor;
            _app = app;
        }
        //创建数据库与表加信号锁,此处需要注意全局环境配置以适应code first与生产环境当中动态增加表
        private static SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);        
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
            _semaphore.Wait();
            try
            {
                switch (DynamicDBTCache.GlobalDBTENV)
                {
                    case DBTEnv.DEV:
                        databaseCreator.CreateTables();
                        break;
                    case DBTEnv.PRO:
                        if (options.ConnectionString.QueryDatabaseIfNotExists())
                        {
                            var tableName = $"{AnnotationHelper.GetDbTableName(entityType)}_{suffix}";
                            if (!options.ConnectionString.QueryTableExist(tableName))
                            {
                                databaseCreator.CreateTables();
                            }
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine();
                //throw new Exception($"【分库分表框架】{ex.Message}");
            }
            finally
            {
                _semaphore.Release();
            }
        }
        //获取分表服务

        public IDynamicDBTService GetDBTService(DynamicDBCParamater dynamicDBCParamater, string optionName = null)
        {
            DynamicDBTOption eFCoreShardingOptions = _doptMonitor.BuildOption(optionName);

            var dbContext = GetDbContext(dynamicDBCParamater, eFCoreShardingOptions);

            return GetProvider(dynamicDBCParamater.DynamicDatabase).GetDynamicDBTService(dbContext);
        }
        //获取数据库上下文

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
        //获取不同数据库的管理员，并且可反射创建实例
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
            catch (Exception ex)
            {
                throw new Exception($"请安装nuget包:{ex.Message}");
            }
        }
    }
}

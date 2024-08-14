using LingYan.DynamicShardingDBT.DBTExtension;
using LingYan.DynamicShardingDBT.DBTFactory;
using LingYan.DynamicShardingDBT.DBTModel;
using Microsoft.EntityFrameworkCore.Design;

namespace LingYan.TestApi
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
    {
        static string mysql1 = "server=192.168.148.131;port=3306;database=sharding_test_1;user=laoda;password=12345678;AllowLoadLocalInfile=true;";
        static DateTime startTime = new DateTime(2024, 8, 14, 17, 20, 0);
        static DesignTimeDbContextFactory()
        {
            ServiceCollection services = new ServiceCollection();
            services.AddDynamicShardingDBT(DBTEnv.DEV,config =>
            {
                config.SetEntityAssemblies(typeof(DesignTimeDbContextFactory).Assembly);
                //使用分表迁移
                config.EnableShardingMigration(true);
                config.AddDataSource(mysql1, DynamicReadWriteType.Read | DynamicReadWriteType.Write, DynamicDBType.MySql);
                //按分钟分表
                config.SetDateSharding<TestEntity>(nameof(TestEntity.TimeKey), DynamicExpandByDateMode.PerMinute, startTime);
                //使用数据库
                config.UseDatabase(mysql1, DynamicDBType.MySql);
            });
            ServiceProvider = services.BuildServiceProvider();
            ServiceProvider.UseDynamicShardingDBT();
        }

        public static readonly IServiceProvider ServiceProvider;

        /// <summary>
        /// 创建数据库上下文
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public TestDbContext CreateDbContext(string[] args)
        {
            Console.WriteLine("开始创建数据库上下文");
            var db = ServiceProvider
                .GetService<IDynamicDBTFactory>()
                .GetDbContext(new DynamicDBCParamater
                {
                    ConnectionString = mysql1,
                    DynamicDatabase = DynamicDBType.MySql,
                });
            return new TestDbContext(db);
        }
    }
}

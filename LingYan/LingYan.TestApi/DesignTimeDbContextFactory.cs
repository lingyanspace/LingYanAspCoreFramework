using LingYan.DynamicShardingDBT.DBTExtension;
using LingYan.DynamicShardingDBT.DBTFactory;
using LingYan.DynamicShardingDBT.DBTIoc;
using LingYan.DynamicShardingDBT.DBTModel;
using Microsoft.EntityFrameworkCore.Design;

namespace LingYan.TestApi
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TestDbContext>
    {
        public static readonly IServiceProvider ServiceProvider;
        private static readonly string _connectionString = "server=192.168.148.131;port=3306;database=sharding_test_1;user=laoda;password=12345678;AllowLoadLocalInfile=true;";
        static DesignTimeDbContextFactory()
        {
            Console.WriteLine("进入工厂");
            DateTime startTime = new DateTime(2024, 1, 1);
            ServiceCollection services = new ServiceCollection();
            services.AddDynamicShardingDBT(x =>
            {
                Console.WriteLine("进入后执行委托");
                x.SetEntityAssemblies(typeof(DesignTimeDbContextFactory).Assembly);
                x.EnableComments(true);

                //取消建表
                x.CreateShardingTableOnStarting(false);

                //取消外键
                x.MigrationsWithoutForeignKey();

                //使用分表迁移
                x.EnableShardingMigration(true);

                //添加数据源
                x.AddDataSource(_connectionString, DynamicReadWriteType.Read | DynamicReadWriteType.Write, DynamicDBType.MySql);

                //按月分表
                x.SetDateSharding<TestEntity>(nameof(TestEntity.TimeKey), DynamicExpandByDateMode.PerMonth, startTime);

                x.UseDatabase(_connectionString, DynamicDBType.MySql);
            });
            ServiceProvider = services.BuildServiceProvider();
            Console.WriteLine("启动后台线程");
            new DynamicShardingBootstrapper(ServiceProvider).StartAsync(default).Wait();
        }      
        public TestDbContext CreateDbContext(string[] args)
        {
            Console.WriteLine("开始创建数据库上下文");
            var db = ServiceProvider
                .GetService<IDynamicDBTFactory>()
                .GetDbContext(new DynamicDBCParamater
                {
                    ConnectionString = _connectionString,
                    DynamicDatabase = DynamicDBType.MySql,
                });
            return new TestDbContext(db);
        }
    }
}

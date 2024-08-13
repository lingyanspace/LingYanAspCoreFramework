
using LingYan.DynamicShardingDBT.DBTExtension;
using LingYan.DynamicShardingDBT.DBTIoc;
using LingYan.DynamicShardingDBT.DBTModel;

namespace LingYan.TestApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("进入主机");
            var mysqlConnectionstr = "192.168.148.131;port=3306;database=test;user=laoda;password=12345678;AllowLoadLocalInfile=true;";
            DateTime startTime = new DateTime(2024, 1, 1);
            var builder = WebApplication.CreateBuilder(args);
            //配置初始化
            builder.Services.AddLogging(x =>
            {
                x.AddConsole();
            });
            builder.Services.AddDynamicShardingDBT(config =>
            {
                config.SetEntityAssemblies(typeof(TestEntity).Assembly);

                //添加数据源
                config.AddDataSource(mysqlConnectionstr, DynamicReadWriteType.Read | DynamicReadWriteType.Write, DynamicDBType.MySql);

                //按分钟分表
                config.SetDateSharding<TestEntity>(nameof(TestEntity.TimeKey), DynamicExpandByDateMode.PerMonth, startTime);
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
           
            var app = builder.Build();
            new DynamicShardingBootstrapper(app.Services).StartAsync(default).Wait();
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}

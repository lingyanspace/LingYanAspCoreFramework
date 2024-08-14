using LingYan.DynamicShardingDBT.DBTExtension;
using LingYan.DynamicShardingDBT.DBTModel;
using Microsoft.EntityFrameworkCore;

namespace LingYan.TestApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("进入主机");
            var mysql1 = "server=192.168.148.131;port=3306;database=sharding_test_1;user=laoda;password=12345678;AllowLoadLocalInfile=true;";
            DateTime startTime = new DateTime(2024, 8, 14, 17, 20, 0);
            var builder = WebApplication.CreateBuilder(args);           
            builder.Services.AddDynamicShardingDBT(DBTEnv.PRO, config =>
            {
                config.SetEntityAssemblies(typeof(TestEntity).Assembly);
                //添加数据源
                config.AddDataSource(mysql1, DynamicReadWriteType.Read | DynamicReadWriteType.Write, DynamicDBType.MySql);
                //按分钟分表
                config.SetDateSharding<TestEntity>(nameof(TestEntity.TimeKey), DynamicExpandByDateMode.PerMinute, startTime);
            });

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();           
            var app = builder.Build();
            app.Services.UseDynamicShardingDBT();
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

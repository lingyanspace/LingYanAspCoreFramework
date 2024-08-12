
using LingYan.SaaSMultiTenantDbSharding.ShardingProvider;
using Microsoft.EntityFrameworkCore;

namespace LingYan.TestApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddTransient<IShardingService, ShardingService>();
            builder.Services.add
            builder.Services.AddDbContext<TestDbContext>(option =>
            {
                string test = "192.168.148.131;port=3306;database=test;user=laoda;password=12345678;AllowLoadLocalInfile=true;";
                option.UseMySql(test,ServerVersion.AutoDetect(test));
            });


            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
           
            var app = builder.Build();

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

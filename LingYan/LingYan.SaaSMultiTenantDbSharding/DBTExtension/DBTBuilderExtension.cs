using LingYan.DynamicShardingDBT.DBTBuilder;
using LingYan.DynamicShardingDBT.DBTFactory;
using LingYan.DynamicShardingDBT.DBTModel;
using LingYan.DynamicShardingDBT.DBTProvider;
using Microsoft.Extensions.DependencyInjection;

namespace LingYan.DynamicShardingDBT.DBTExtension
{
    public static class DBTBuilderExtension
    {
        public static IServiceCollection AddDynamicShardingDBT(this IServiceCollection services,DBTEnv dBTEnv, Action<IDynamicDBTBuilder> builder)
        {
            Console.WriteLine("注入分表框架");
            services.AddOptions<DynamicDBTOption>();
            services.AddLogging();
            DynamicaDBTBuilder container = new DynamicaDBTBuilder(services);
            builder?.Invoke(container);
            services.AddSingleton(container);
            services.AddSingleton<IDynamicDBTBuilder>(container);
            services.AddSingleton<IDynamicDBTConfig>(container);
            services.AddScoped<DynamicDBTFactory>();
            services.AddScoped<IDynamicDBTFactory, DynamicDBTFactory>();
            services.AddScoped<IDynamicShardingDBTService, DynamicShardingDBTService>();
            services.AddHostedService<DynamicShardingInitializer>();
            return services;
        }
        public static void UseDynamicShardingDBT(this IServiceProvider app)
        {
            Console.WriteLine("启用分表框架");
            new DynamicShardingInitializer(app).StartAsync(default).Wait();
        }
    }
}

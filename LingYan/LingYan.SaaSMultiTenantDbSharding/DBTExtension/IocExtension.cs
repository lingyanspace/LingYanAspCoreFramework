using LingYan.DynamicShardingDBT.DBTFactory;
using LingYan.DynamicShardingDBT.DBTIoc;
using LingYan.DynamicShardingDBT.DBTModel;
using LingYan.DynamicShardingDBT.DBTProvider;
using LingYan.DynamicShardingDBT.ShardingIoc;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Spi;

namespace LingYan.DynamicShardingDBT.DBTExtension
{
    public static class IocExtension
    {
        public static IServiceCollection AddDynamicShardingDBT(this IServiceCollection services, Action<IDynamicDBTBuilder> builder)
        {
            services.AddOptions<DynamicDBTOption>();
            services.AddLogging();

            DynamicaDBTIoc container = new DynamicaDBTIoc(services);
            builder?.Invoke(container);
            services.AddSingleton(container);
            services.AddSingleton<IDynamicDBTBuilder>(container);
            services.AddSingleton<IDynamicDBTConfig>(container);
            services.AddScoped<DynamicDBTFactory>();
            services.AddScoped<IDynamicDBTFactory, DynamicDBTFactory>();
            services.AddScoped<IDynamicShardingDBTService, DynamicShardingDBTService>();

            services.AddHostedService<DynamicShardingBootstrapper>();

            return services;
        }
    }
}

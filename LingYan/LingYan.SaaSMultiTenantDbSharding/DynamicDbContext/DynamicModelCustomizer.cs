using LingYan.SaaSMultiTenantDbSharding.ShardingProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace LingYan.SaaSMultiTenantDbSharding.DynamicDbContext
{
    public class DynamicModelCustomizer : ModelCustomizer
    {
        public DynamicModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
        {
        }
        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);
            var dbContextBase = context as DynamicDbContext;
            var shardingTypeFinder = dbContextBase.ServiceProvider.GetService<IShardingService>();
            //查找需要重新映射表名的类
            var shardingTypes = shardingTypeFinder.GetShardingTables();

            if (shardingTypes != null && shardingTypes.Count() > 0)
            {

                if (context is DynamicDbContext contextBase)
                {
                    if (!string.IsNullOrEmpty(contextBase.DynamicDbContextParamater.Suffix))
                    {
                        foreach (var type in shardingTypes)
                        {
                            switch (contextBase.DynamicDbContextParamater.DynamicDatabase)
                            {
                                case DatabaseType.SqlServer:
                                    modelBuilder.Entity(type).ToTable($"{type.Name}_{contextBase.DynamicDbContextParamater.Suffix}");
                                    break;
                                case DatabaseType.SQLite:
                                    modelBuilder.Entity(type).ToTable($"{type.Name}_{contextBase.DynamicDbContextParamater.Suffix}");
                                    break;
                                case DatabaseType.MySql:
                                    modelBuilder.Entity(type).ToTable($"{type.Name}_{contextBase.DynamicDbContextParamater.Suffix}");
                                    break;
                                case DatabaseType.Oracle:
                                    modelBuilder.Entity(type).ToTable($"{type.Name}_{contextBase.DynamicDbContextParamater.Suffix}");
                                    break;
                                default:
                                    modelBuilder.Entity(type).ToTable($"{type.Name}_{contextBase.DynamicDbContextParamater.Suffix}");
                                    break;
                            }
                        }
                    }
                }

            }



        }

    }
}

using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Collections;

namespace LingYanAspCoreFramework.MultiTenants
{
    public static class TenantExtension
    {
        public static void InitTenant(this IServiceProvider serviceProvider,LYBuilderRuntimeModel lYBuilderRuntimeModel)
        {
            var tenantManager = serviceProvider.GetService<ITenantManager>();
            var shardingBuilder = serviceProvider.GetService<IShardingBuilder>();
            using (var scope = serviceProvider.CreateScope())
            {
                if (lYBuilderRuntimeModel.ModuleDbContextList.Count>0)
                {
                    //多租户配置表运行迁移
                    var configDbContextType = lYBuilderRuntimeModel.ModuleDbContextList.Where(pair => pair.Value == DbContextType.TenantConfigDbContext).Select(pair => pair.Key).FirstOrDefault();
                    if (configDbContextType!=null)
                    {
                        var configDbContext = (DbContext)scope.ServiceProvider.GetService(configDbContextType);
                        configDbContext.Database.Migrate();
                        var SetMethod = configDbContext.GetType().GetMethods().FirstOrDefault(f => f.Name == "Set").MakeGenericMethod(lYBuilderRuntimeModel.ModuleTenantBaseEntitys["BaseSysOwnerTenantConfig"]);
                        var dbSet = SetMethod.Invoke(configDbContext, null);
                        var toListMethod = typeof(Enumerable).GetMethod("ToList").MakeGenericMethod(lYBuilderRuntimeModel.ModuleTenantBaseEntitys["BaseSysOwnerTenantConfig"]);
                        var sysUserTenantConfigs = toListMethod.Invoke(null, new object[] { dbSet }) as IEnumerable;
                        if (sysUserTenantConfigs != null)
                        {
                            foreach (dynamic sysUserTenantConfig in sysUserTenantConfigs)
                            {
                                var shardingTenantOptions = JsonConvert.DeserializeObject<ShardingTenantOptions>(sysUserTenantConfig.ConfigJson);

                                var shardingRuntimeContext = shardingBuilder.Build(shardingTenantOptions);

                                tenantManager.AddTenantSharding(sysUserTenantConfig.CompanyId, shardingRuntimeContext);
                            }
                        }
                    }                  
                }               
            }
            var tenantIds = tenantManager.GetAll();
            foreach (var tenantId in tenantIds)
            {
                using (tenantManager.CreateScope(tenantId))
                using (var scope = serviceProvider.CreateScope())
                {
                    var shardingRuntimeContext = tenantManager.GetCurrentTenantContext().GetShardingRuntimeContext();
                    //开启定时任务
                    //shardingRuntimeContext.UseAutoShardingCreate();
                    var tenantDbContext = (DbContext)scope.ServiceProvider.GetService(lYBuilderRuntimeModel.TenantTemplateDbContexts.FirstOrDefault());
                    tenantDbContext.Database.Migrate();
                    //补偿表
                    shardingRuntimeContext.UseAutoTryCompensateTable();
                }
            }
        }
    }
}

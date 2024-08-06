using LingYan.Model;
using LingYan.MultiTenant.SysShardingBuilder;
using LongYuBuilding.ShardingModule.MultiTenant.SysTenantProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore.Extensions;

namespace LingYan.MultiTenant
{
    /// <summary>
    ///1、需要有Migration系统的租户管理数据库迁移
    ///2、需要有Migration系统的租户模版数据库迁移
    ///3、系统的租户管理数据库上下文
    ///4、系统租户模版数据库上下文
    ///5、如果系统租户模版数据库有虚拟路由，需要虚拟化
    /// </summary>
    public static class MultiTenantExtension
    {
        public static void AddSysTenant(this IServiceProvider provider, object tid, ShardingTenantOptions shardingTenantOptions)
        {
            try
            {
                var _shardingBuilder = provider.GetService<IShardingBuilder>();
                var _tenantManager = provider.GetService<ITenantManager>();
                //创建运行时
                var shardingRuntimeContext = _shardingBuilder.Build(shardingTenantOptions);
                //添加租户信息
                _tenantManager.AddTenantSharding(tid, shardingRuntimeContext);
                //创建租户环境
                using (_tenantManager.CreateScope(tid))
                //开启分片定时任务
                using (var scope = provider.CreateScope())
                {
                    var runtimeContext = _tenantManager.GetCurrentTenantContext().GetShardingRuntimeContext();
                    //runtimeContext.UseAutoShardingCreate(); //启动定时任务
                    var tenantDbContext = (DbContext)scope.ServiceProvider.GetService(LYExpose.LYBuilderRuntimeManager.TenantTemplateDbContexts.FirstOrDefault());
                    tenantDbContext.Database.Migrate();
                    runtimeContext.UseAutoTryCompensateTable();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}

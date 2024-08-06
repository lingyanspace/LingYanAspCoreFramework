using ShardingCore.Core.RuntimeContexts;

namespace LongYuBuilding.ShardingModule.MultiTenant.SysTenantProvider
{
    public interface ITenantManager
    {
        bool AddTenantSharding(object tenantId, IShardingRuntimeContext shardingRuntimeContext);
        TenantScope CreateScope(object tenantId);
        List<object> GetAll();
        TenantContext GetCurrentTenantContext();
    }
}
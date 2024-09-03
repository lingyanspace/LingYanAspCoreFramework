using ShardingCore.Core.RuntimeContexts;

namespace LingYanAspCoreFramework.MultiTenants
{
    public interface ITenantManager
    {
        bool AddTenantSharding(object tenantId, IShardingRuntimeContext shardingRuntimeContext);
        TenantScope CreateScope(object tenantId);
        List<object> GetAll();
        TenantContext GetCurrentTenantContext();
    }
}
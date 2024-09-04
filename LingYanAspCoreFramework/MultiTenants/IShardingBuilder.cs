using ShardingCore.Core.RuntimeContexts;

namespace LingYanAspCoreFramework.MultiTenants
{
    public interface IShardingBuilder
    {
        IShardingRuntimeContext Build(ShardingTenantOptions tenantOptions);
    }
}
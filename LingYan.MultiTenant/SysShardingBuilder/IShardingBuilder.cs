using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Sharding;

namespace LingYan.MultiTenant.SysShardingBuilder
{
    public interface IShardingBuilder
    {
        IShardingRuntimeContext Build(ShardingTenantOptions tenantOptions);
    }
}
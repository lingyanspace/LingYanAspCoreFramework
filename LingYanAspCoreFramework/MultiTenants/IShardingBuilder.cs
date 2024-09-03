namespace LingYanAspCoreFramework.MultiTenants
{
    public interface IShardingBuilder
    {
        IShardingRuntimeContext Build(ShardingTenantOptions tenantOptions);
    }
}
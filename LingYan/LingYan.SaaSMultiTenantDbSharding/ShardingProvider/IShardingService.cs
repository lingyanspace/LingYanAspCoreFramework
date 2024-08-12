namespace LingYan.SaaSMultiTenantDbSharding.ShardingProvider
{
    public interface IShardingService
    {
        List<Type> GetShardingTables();
    }
}

namespace LongYuBuilding.ShardingModule.MultiTenant.SysTenantProvider
{
    public interface ITenantContextAccessor
    {
        TenantContext? TenantContext { get; set; }
    }
}
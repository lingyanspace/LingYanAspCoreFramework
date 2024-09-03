namespace LingYanAspCoreFramework.MultiTenants
{
    public interface ITenantContextAccessor
    {
        TenantContext? TenantContext { get; set; }
    }
}
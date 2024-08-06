﻿namespace LongYuBuilding.ShardingModule.MultiTenant.SysTenantProvider
{
    public class TenantScope : IDisposable
    {
        public TenantScope(ITenantContextAccessor tenantContextAccessor)
        {
            TenantContextAccessor = tenantContextAccessor;
        }

        public ITenantContextAccessor TenantContextAccessor { get; }

        public void Dispose()
        {
            TenantContextAccessor.TenantContext = null;
        }
    }
}

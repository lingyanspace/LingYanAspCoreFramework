﻿namespace LingYanAspCoreFramework.MultiTenants
{
    public class TenantContext
    {
        private readonly IShardingRuntimeContext _shardingRuntimeContext;

        public TenantContext(IShardingRuntimeContext shardingRuntimeContext)
        {
            _shardingRuntimeContext = shardingRuntimeContext;
        }
        public IShardingRuntimeContext GetShardingRuntimeContext()
        {
            return _shardingRuntimeContext;
        }
    }
}

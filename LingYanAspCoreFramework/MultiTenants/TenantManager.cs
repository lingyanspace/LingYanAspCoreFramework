using ShardingCore.Core.RuntimeContexts;
using System.Collections.Concurrent;

namespace LingYanAspCoreFramework.MultiTenants
{
    public class TenantManager : ITenantManager
    {
        private readonly ITenantContextAccessor _tenantContextAccessor;
        private readonly ConcurrentDictionary<object, IShardingRuntimeContext> _cache = new();

        public TenantManager(ITenantContextAccessor tenantContextAccessor)
        {
            _tenantContextAccessor = tenantContextAccessor;
        } 

        public List<object> GetAll()
        {
            return _cache.Keys.ToList();
        }

        public TenantContext GetCurrentTenantContext()
        {
            return _tenantContextAccessor.TenantContext;
        }

        public bool AddTenantSharding(object tenantId, IShardingRuntimeContext shardingRuntimeContext)
        {
            return _cache.TryAdd(tenantId, shardingRuntimeContext);
        }

        public TenantScope CreateScope(object tenantId)
        {
            if (!_cache.TryGetValue(tenantId, out var shardingRuntimeContext))
            {
                throw new InvalidOperationException("未找到对应租户的配置");
            }
            _tenantContextAccessor.TenantContext = new TenantContext(shardingRuntimeContext);
            return new TenantScope(_tenantContextAccessor);
        }
    }
}

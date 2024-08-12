using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LingYan.SaaSMultiTenantDbSharding.DynamicDbContext
{
    // 实现 IModelCacheKeyFactory 接口，用于创建动态模型的缓存键
    public class DynamicModelCacheKeyFactoryDesignTimeSupport : IModelCacheKeyFactory 
    {
        // 创建缓存键的方法，接受 DbContext 和设计时标志
        public object Create(DbContext context, bool designTime)
        {
            // 检查上下文是否为 DynamicDbContext 类型
            if (context is DynamicDbContext dynamicContext)
            {
                // 如果是 DynamicDbContext，返回一个包含上下文类型、实体命名空间和后缀的元组Tuple
                return (context.GetType(), $"{dynamicContext.DynamicDbContextParamater.EntityNamespace}:{dynamicContext.DynamicDbContextParamater.Suffix}", designTime);
            }
            else
            {
                // 如果不是 DynamicDbContext，仅返回上下文类型
                return (object)context.GetType();
            }
        }

        // 重载的 Create 方法，默认设计时标志为 false
        public object Create(DbContext context)
        {
            return Create(context, false);
        }
    }
}

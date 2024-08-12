using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace LingYan.SaaSMultiTenantDbSharding.DynamicDbContext
{
    //动态数据库上下文
    public class DynamicDbContext : DbContext
    {

        public DynamicDbContext(DbContextOptions contextOptions, DynamicDbContextParamater dynamicDbContextParamater, IServiceProvider serviceProvider) : base(contextOptions)
        {
            ContextOptions = contextOptions;
            DynamicDbContextParamater = dynamicDbContextParamater;
            ServiceProvider = serviceProvider;
        }
        //原生数据库上下文描述
        public DbContextOptions ContextOptions { get; }
        //动态传参
        public DynamicDbContextParamater DynamicDbContextParamater { get; }
        public IServiceProvider ServiceProvider { get; }
        //​OnModelCreating​ 方法在 ​DbContext​ 的模型被初始化时调用，用于配置实体模型。
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
        //OnConfiguring​ 方法在 ​DbContext​ 被配置时调用，用于配置 ​DbContext​ 的选项。
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            //当需要进行分表时
            if (!string.IsNullOrEmpty(this.DynamicDbContextParamater.Suffix))
            {
                //使用 ​optionsBuilder.ReplaceService​ 方法替换 ​IModelCacheKeyFactory​ 和 ​IModelCustomizer​ 服务。
                //​​IModelCacheKeyFactory​ 服务被替换为 ​DynamicModelCacheKeyFactoryDesignTimeSupport​，用于生成动态模型的缓存键。
                //​​IModelCustomizer​ 服务被替换为 ​DynamicModelCustomizer​，用于自定义模型的配置。
                optionsBuilder.ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactoryDesignTimeSupport>().ReplaceService<IModelCustomizer, DynamicModelCustomizer>();
            }
        }
    }
}

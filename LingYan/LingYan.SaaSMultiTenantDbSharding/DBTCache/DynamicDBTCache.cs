using LingYan.DynamicShardingDBT.DBTContext;

namespace LingYan.DynamicShardingDBT.DBTCache
{
    public static class DynamicDBTCache
    {
        /// <summary>
        /// 默认数据源名
        /// </summary>
        public const string DefaultSource = nameof(DefaultSource);
        public static IServiceProvider ServiceProvider { get; set; }
        public static readonly SynchronizedCollection<DynamicDbContext> DynamicDbContexts = new SynchronizedCollection<DynamicDbContext>();
    }
}

using LingYan.DynamicShardingDBT.DBTContext;
using LingYan.DynamicShardingDBT.DBTModel;

namespace LingYan.DynamicShardingDBT.DBTCache
{
    public static class DynamicDBTCache
    {
        /// <summary>
        /// 默认数据源名
        /// </summary>
        public const string DefaultSource = nameof(DefaultSource);
        /// <summary>
        /// 开发环境与生产环境
        /// </summary>
        public static DBTEnv GlobalDBTENV = DBTEnv.DEV;
        public static IServiceProvider ServiceProvider { get; set; }
        public static readonly SynchronizedCollection<DynamicDbContext> DynamicDbContexts = new SynchronizedCollection<DynamicDbContext>();
    }
}

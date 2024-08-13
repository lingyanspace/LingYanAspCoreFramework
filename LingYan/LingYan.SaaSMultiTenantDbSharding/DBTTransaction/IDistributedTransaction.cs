using LingYan.DynamicShardingDBT.DBTProvider;

namespace LingYan.DynamicShardingDBT.DBTTransaction
{
    /// <summary>
    /// 分布式
    /// </summary>
    public interface IDistributedTransaction : ITransaction
    {
        /// <summary>
        /// 添加Db
        /// </summary>
        /// <param name="repositories"></param>
        void AddDBTService(params IDynamicDBTService[] repositories);
    }
}

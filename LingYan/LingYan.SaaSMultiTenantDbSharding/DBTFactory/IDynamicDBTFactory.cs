using LingYan.DynamicShardingDBT.DBTContext;
using LingYan.DynamicShardingDBT.DBTModel;
using LingYan.DynamicShardingDBT.DBTProvider;

namespace LingYan.DynamicShardingDBT.DBTFactory
{
    /// <summary>
    /// 数据库工厂
    /// </summary>
    public interface IDynamicDBTFactory 
    {
        /// <summary>
        /// 获取DbAccessor
        /// </summary>
        /// <param name="dbContextParamters">参数</param>
        /// <param name="optionName">选项名</param>
        /// <returns></returns>
        IDynamicDBTService GetDBTService(DynamicDBCParamater dbContextParamters, string optionName = null);

        /// <summary>
        /// 获取DbContext
        /// </summary>
        /// <param name="dbContextParamters">参数</param>
        /// <param name="eFCoreShardingOptions">参数</param>
        /// <returns></returns>
        DynamicDbContext GetDbContext(DynamicDBCParamater dbContextParamters, DynamicDBTOption eFCoreShardingOptions = null);
    }
}

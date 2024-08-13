namespace LingYan.DynamicShardingDBT.DBTProvider
{
    public interface IDynamicShardingDBTService : IBaseDbService 
    {
        #region 查询数据

        /// <summary>
        /// 获取IShardingQueryable
        /// </summary>
        /// <typeparam name="T">实体泛型</typeparam>
        /// <returns></returns>
        IDynamicShardingQueryable<T> GetIShardingQueryable<T>() where T : class;
        #endregion
    }
}

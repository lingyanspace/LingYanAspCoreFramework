using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace LingYan.UnitOfWork.BaseIUnitOfWork
{
    // <summary>
    /// 定义工作单元的接口。
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// 更改数据库名称。此功能仅适用于 MySQL。
        /// </summary>
        /// <param name="database">数据库名称。</param>
        void ChangeDatabase(string database);

        /// <summary>
        /// 获取指定类型的实体的存储库。
        /// </summary>
        /// <param name="hasCustomRepository">是否提供自定义存储库。</param>
        /// <typeparam name="TEntity">实体的类型。</typeparam>
        /// <returns>继承自 <see cref="IRepository{TEntity}"/> 接口的类型的实例。</returns>
        IRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false) where TEntity : class;

        /// <summary>
        /// 将该上下文所做的所有更改保存到数据库。
        /// </summary>
        /// <param name="ensureAutoHistory">如果要保存更改时自动记录更改历史，请设置为 true。</param>
        /// <returns>写入数据库的状态实体的数量。</returns>
        int SaveChanges(bool ensureAutoHistory = false);

        /// <summary>
        /// 异步将该工作单元中所做的所有更改保存到数据库。
        /// </summary>
        /// <param name="ensureAutoHistory">如果要保存更改时自动记录更改历史，请设置为 true。</param>
        /// <returns>表示异步保存操作的 <see cref="Task{TResult}"/>。任务结果包含写入数据库的状态实体的数量。</returns>
        Task<int> SaveChangesAsync(bool ensureAutoHistory = false);

        /// <summary>
        /// 执行指定的原始 SQL 命令。
        /// </summary>
        /// <param name="sql">原始 SQL。</param>
        /// <param name="parameters">参数。</param>
        /// <returns>写入数据库的状态实体的数量。</returns>
        int ExecuteSqlCommand(string sql, params object[] parameters);

        /// <summary>
        /// 使用原始 SQL 查询获取指定类型的实体数据。
        /// </summary>
        /// <typeparam name="TEntity">实体的类型。</typeparam>
        /// <param name="sql">原始 SQL。</param>
        /// <param name="parameters">参数。</param>
        /// <returns>包含满足原始 SQL 条件的元素的 <see cref="IQueryable{T}"/>。</returns>
        IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class;

        /// <summary>
        /// 使用 TrakGrap API 附加断开的实体。
        /// </summary>
        /// <param name="rootEntity">根实体。</param>
        /// <param name="callback">将对象的状态属性转换为实体的 Entry 状态的委托。</param>
        void TrackGraph(object rootEntity, Action<EntityEntryGraphNode> callback);
    }
}

using Microsoft.EntityFrameworkCore;

namespace LingYan.UnitOfWork.BaseIUnitOfWork
{
    /// <summary>
    /// 定义泛型工作单元的接口。
    /// </summary>
    public interface IUnitOfWork<TContext> : IUnitOfWork where TContext : DbContext
    {
        /// <summary>
        /// 获取数据库上下文。
        /// </summary>
        /// <returns>类型为 <typeparamref name="TContext"/> 的实例。</returns>
        TContext DbContext { get; }

        /// <summary>
        /// 使用分布式事务将该上下文所做的所有更改保存到数据库。
        /// </summary>
        /// <param name="ensureAutoHistory">如果要保存更改时自动记录更改历史，请设置为 true。</param>
        /// <param name="unitOfWorks">一个可选的 <see cref="IUnitOfWork"/> 数组。</param>
        /// <returns>表示异步保存操作的 <see cref="Task{TResult}"/>。任务结果包含写入数据库的状态实体的数量。</returns>
        Task<int> SaveChangesAsync(bool ensureAutoHistory = false, params IUnitOfWork[] unitOfWorks);
    }
}

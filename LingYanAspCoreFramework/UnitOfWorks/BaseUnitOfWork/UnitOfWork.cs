using LingYanAspCoreFramework.UnitOfWork.BaseIUnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Data;
using System.Text.RegularExpressions;

namespace LingYanAspCoreFramework.UnitOfWork.BaseUnitOfWork
{
    /// <summary>
    /// 表示 <see cref="IUnitOfWork"/> 和 <see cref="IUnitOfWork{TContext}"/> 接口的默认实现。
    /// </summary>
    /// <typeparam name="TContext">数据库上下文的类型。</typeparam>
    public class UnitOfWork<TContext> : IRepositoryFactory, IUnitOfWork<TContext>, IUnitOfWork where TContext : DbContext
    {
        private readonly TContext _context;
        private bool disposed = false;
        private Dictionary<Type, object> repositories;

        /// <summary>
        /// 初始化 <see cref="UnitOfWork{TContext}"/> 类的新实例。
        /// </summary>
        /// <param name="context">数据库上下文。</param>
        public UnitOfWork(TContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// 获取数据库上下文。
        /// </summary>
        /// <returns>类型为 <typeparamref name="TContext"/> 的实例。</returns>
        public TContext DbContext => _context;

        /// <summary>
        /// 更改数据库名称。目前仅适用于 MySQL。请注意，此功能要求在同一台机器上有多个数据库。
        /// </summary>
        /// <param name="database">数据库名称。</param>
        /// <remarks>
        /// 此方法主要用于支持在同一模型中使用多个数据库。要求在同一台机器上有多个数据库。
        /// </remarks>
        public void ChangeDatabase(string database)
        {
            var connection = _context.Database.GetDbConnection();
            if (connection.State.HasFlag(ConnectionState.Open))
            {
                connection.ChangeDatabase(database);
            }
            else
            {
                var connectionString = Regex.Replace(connection.ConnectionString.Replace(" ", ""), @"(?<=[Dd]atabase=)\w+(?=;)", database, RegexOptions.Singleline);
                connection.ConnectionString = connectionString;
            }

            // 以下代码仅适用于 MySQL。
            var items = _context.Model.GetEntityTypes();
            foreach (var item in items)
            {
                if (item is IConventionEntityType entityType)
                {
                    entityType.SetSchema(database);
                }

            }
        }

        /// <summary>
        /// 获取指定类型的实体的存储库。
        /// </summary>
        /// <param name="hasCustomRepository">是否提供自定义存储库。</param>
        /// <typeparam name="TEntity">实体的类型。</typeparam>
        /// <returns>继承自 <see cref="IRepository{TEntity}"/> 接口的类型的实例。</returns>
        public IRepository<TEntity> GetRepository<TEntity>(bool hasCustomRepository = false) where TEntity : class
        {
            if (repositories == null)
            {
                repositories = new Dictionary<Type, object>();
            }
            // 如何最好地支持自定义存储库？
            if (hasCustomRepository)
            {
                var customRepo = _context.GetService<IRepository<TEntity>>();
                if (customRepo != null)
                {
                    return customRepo;
                }
            }

            var type = typeof(TEntity);
            if (!repositories.ContainsKey(type))
            {
                repositories[type] = new Repository<TContext, TEntity>(_context);
            }

            return (IRepository<TEntity>)repositories[type];
        }

        /// <summary>
        /// 执行指定的原始 SQL 命令。
        /// </summary>
        /// <param name="sql">原始 SQL。</param>
        /// <param name="parameters">参数。</param>
        /// <returns>写入数据库的状态实体的数量。</returns>
        public int ExecuteSqlCommand(string sql, params object[] parameters) => _context.Database.ExecuteSqlRaw(sql, parameters);

        /// <summary>
        /// 使用原始 SQL 查询获取指定类型的实体数据。
        /// </summary>
        /// <typeparam name="TEntity">实体的类型。</typeparam>
        /// <param name="sql">原始 SQL。</param>
        /// <param name="parameters">参数。</param>
        /// <returns>包含满足原始 SQL 条件的元素的 <see cref="IQueryable{T}"/>。</returns>
        public IQueryable<TEntity> FromSql<TEntity>(string sql, params object[] parameters) where TEntity : class => _context.Set<TEntity>().FromSqlRaw(sql, parameters);

        /// <summary>
        /// 将该上下文所做的所有更改保存到数据库。
        /// </summary>
        /// <param name="ensureAutoHistory">如果要保存更改时自动记录更改历史，请设置为 true。</param>
        /// <returns>写入数据库的状态实体的数量。</returns>
        public int SaveChanges(bool ensureAutoHistory = false)
        {
            if (ensureAutoHistory)
            {
                _context.EnsureAutoHistory();
            }
            var result = _context.SaveChanges();
            _context.ChangeTracker.Clear();
            return result;
        }
        /// <summary>
        /// 异步将该工作单元中所做的所有更改保存到数据库。
        /// </summary>
        /// <param name="ensureAutoHistory">如果要保存更改时自动记录更改历史，请设置为 true。</param>
        /// <returns>表示异步保存操作的 <see cref="Task{TResult}"/>。任务结果包含写入数据库的状态实体的数量。</returns>
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);

        public async Task<int> SaveChangesAsync(bool ensureAutoHistory = false)
        {
            await _semaphoreSlim.WaitAsync();
            try
            {
                if (ensureAutoHistory)
                {
                    _context.EnsureAutoHistory();
                }
                var result = await _context.SaveChangesAsync();
                _context.ChangeTracker.Clear();
                return result;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        /// <summary>
        /// 将该上下文中所做的所有更改与分布式事务一起保存到数据库。
        /// </summary>
        /// <param name="ensureAutoHistory">如果要保存更改时自动记录更改历史，请设置为 true。</param>
        /// <param name="unitOfWorks">一个可选的 <see cref="IUnitOfWork"/> 数组。</param>
        /// <returns>表示异步保存操作的 <see cref="Task{TResult}"/>。任务结果包含写入数据库的状态实体的数量。</returns>
        public async Task<int> SaveChangesAsync(bool ensureAutoHistory = false, params IUnitOfWork[] unitOfWorks)
        {
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var count = 0;
                    foreach (var unitOfWork in unitOfWorks)
                    {
                        await _semaphoreSlim.WaitAsync();
                        try
                        {
                            count += await unitOfWork.SaveChangesAsync(ensureAutoHistory).ConfigureAwait(false);
                        }
                        finally
                        {
                            _semaphoreSlim.Release();
                        }
                    }

                    count += await SaveChangesAsync(ensureAutoHistory);

                    await transaction.CommitAsync();

                    return count;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw ex;
                }
                finally
                {
                    _context.ChangeTracker.Clear();
                }
            }
        }
        /// <summary>
        /// 执行与释放、释放或重置非托管资源相关联的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// 执行与释放、释放或重置非托管资源相关联的应用程序定义的任务。
        /// </summary>
        /// <param name="disposing">是否正在处理释放操作。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // 清空存储库
                    if (repositories != null)
                    {
                        repositories.Clear();
                    }

                    // 释放数据库上下文
                    _context.Dispose();
                }
            }

            disposed = true;
        }

        /// <summary>
        /// 跟踪对象图。
        /// </summary>
        /// <param name="rootEntity">根实体。</param>
        /// <param name="callback">回调函数。</param>
        public void TrackGraph(object rootEntity, Action<EntityEntryGraphNode> callback)
        {
            _context.ChangeTracker.TrackGraph(rootEntity, callback);
        }
    }
}

using Microsoft.EntityFrameworkCore;

namespace LingYan.UnitOfWork.BaseIUnitOfWork
{
    public static class IQueryablePageListExtensions
    {
        /// <summary>
        /// 通过指定的 <paramref name="pageIndex"/> 和 <paramref name="pageSize"/> 将指定的源转换为 <see cref="IPagedList{T}"/>。
        /// </summary>
        /// <typeparam name="T">源的类型。</typeparam>
        /// <param name="source">要分页的源。</param>
        /// <param name="pageIndex">页的索引。</param>
        /// <param name="pageSize">页的大小。</param>
        /// <param name="cancellationToken">
        /// 用于在等待任务完成时观察的 <see cref="CancellationToken"/>。
        /// </param>
        /// <param name="indexFrom">起始索引值。</param>
        /// <returns>继承自 <see cref="IPagedList{T}"/> 接口的实例。</returns>
        public static async Task<IPagedList<T>> ToPagedListAsync<T>(this IQueryable<T> source, int pageIndex, int pageSize, int indexFrom = 0, CancellationToken cancellationToken = default)
        {
            if (indexFrom > pageIndex)
            {
                throw new ArgumentException($"indexFrom: {indexFrom} > pageIndex: {pageIndex}，必须满足 indexFrom <= pageIndex");
            }

            var count = await source.CountAsync(cancellationToken).ConfigureAwait(false);
            var items = await source.Skip((pageIndex - indexFrom) * pageSize)
                                    .Take(pageSize).ToListAsync(cancellationToken).ConfigureAwait(false);

            var pagedList = new PagedList<T>()
            {
                PageIndex = pageIndex,
                PageSize = pageSize,
                IndexFrom = indexFrom,
                TotalCount = count,
                Items = items,
                TotalPages = (int)Math.Ceiling(count / (double)pageSize)
            };

            return pagedList;
        }
    }
}

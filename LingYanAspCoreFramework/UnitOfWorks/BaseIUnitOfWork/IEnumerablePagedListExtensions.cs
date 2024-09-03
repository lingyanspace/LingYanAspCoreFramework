namespace LingYanAspCoreFramework.UnitOfWork.BaseIUnitOfWork
{
    /// <summary>
    /// 为 <see cref="IEnumerable{T}"/> 提供一些扩展方法，以提供分页功能。
    /// </summary>
    public static class IEnumerablePagedListExtensions
    {
        /// <summary>
        /// 将指定的源通过指定的 <paramref name="pageIndex"/> 和 <paramref name="pageSize"/> 转换为 <see cref="IPagedList{T}"/>。
        /// </summary>
        /// <typeparam name="T">源的类型。</typeparam>
        /// <param name="source">要分页的源。</param>
        /// <param name="pageIndex">页的索引。</param>
        /// <param name="pageSize">页的大小。</param>
        /// <param name="indexFrom">起始索引值。</param>
        /// <returns>继承自 <see cref="IPagedList{T}"/> 接口的实例。</returns>
        public static IPagedList<T> ToPagedList<T>(this IEnumerable<T> source, int pageIndex, int pageSize, int indexFrom = 0) => new PagedList<T>(source, pageIndex, pageSize, indexFrom);

        /// <summary>
        /// 通过指定的转换器 <paramref name="converter"/>、<paramref name="pageIndex"/> 和 <paramref name="pageSize"/> 将指定的源转换为 <see cref="IPagedList{T}"/>。
        /// </summary>
        /// <typeparam name="TSource">源的类型。</typeparam>
        /// <typeparam name="TResult">结果的类型。</typeparam>
        /// <param name="source">要转换的源。</param>
        /// <param name="converter">用于将 <typeparamref name="TSource"/> 转换为 <typeparamref name="TResult"/> 的转换器。</param>
        /// <param name="pageIndex">页索引。</param>
        /// <param name="pageSize">页大小。</param>
        /// <param name="indexFrom">起始索引值。</param>
        /// <returns>继承自 <see cref="IPagedList{T}"/> 接口的实例。</returns>
        public static IPagedList<TResult> ToPagedList<TSource, TResult>(this IEnumerable<TSource> source, Func<IEnumerable<TSource>, IEnumerable<TResult>> converter, int pageIndex, int pageSize, int indexFrom = 0) => new PagedList<TSource, TResult>(source, converter, pageIndex, pageSize, indexFrom);
    }
}

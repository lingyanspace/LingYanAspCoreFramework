namespace LingYan.UnitOfWork.BaseIUnitOfWork
{
    public interface IPagedList<T>
    {
        /// <summary>
        /// 起始索引
        /// </summary>
        /// <value>The index start value.</value>
        int IndexFrom { get; }
        /// <summary>
        /// 分页索引
        /// </summary>
        int PageIndex { get; }
        /// <summary>
        /// 页大小
        /// </summary>
        int PageSize { get; }
        /// <summary>
        /// 总条数 <typeparamref name="T"/>
        /// </summary>
        int TotalCount { get; }
        /// <summary>
        ///总页数
        /// </summary>
        int TotalPages { get; }
        /// <summary>
        /// 列表
        /// </summary>
        IList<T> Items { get; }
        /// <summary>
        /// 有页面
        /// </summary>
        /// <value>The has previous page.</value>
        bool HasPreviousPage { get; }

        /// <summary>
        /// 有下一页
        /// </summary>
        /// <value>The has next page.</value>
        bool HasNextPage { get; }
    }
}

namespace LingYanAspCoreFramework.UnitOfWorks
{
    /// ​<summary>
    /// 定义了​<see cref="IRepository{TEntity}"/>接口的接口。
    /// ​</summary>
    public interface IRepositoryFactory
    {
        /// ​<summary>
        /// 获取指定实体类型 ​<typeparamref name="TEntity"/> 的存储库。
        /// ​</summary>
        /// ​<param name="hasCustomRepository">是否提供自定义存储库。如果为​<c>True​</c>则表示有自定义存储库。​</param>
        /// ​<typeparam name="TEntity">实体的类型​</typeparam>
        /// ​<returns>一个继承自​<see cref="IRepository{TEntity}"/>接口的实例。​</returns>
        IRepository​<TEntity> GetRepository​<TEntity>(bool hasCustomRepository = false) where TEntity : class;
    }
}

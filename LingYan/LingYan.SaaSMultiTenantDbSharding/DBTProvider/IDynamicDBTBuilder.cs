using LingYan.DynamicShardingDBT.DBTCache;
using LingYan.DynamicShardingDBT.DBTModel;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection;

namespace LingYan.DynamicShardingDBT.DBTProvider
{
    /// <summary>
    /// 构建者
    /// </summary>
    public interface IDynamicDBTBuilder
    {
        /// <summary>
        /// 设置实体的程序集
        /// </summary>
        /// <param name="assemblies">程序集</param>
        /// <returns></returns>
        IDynamicDBTBuilder SetEntityAssemblies(params Assembly[] assemblies);

        /// <summary>
        /// 设置SQL执行超时时间(单位秒,默认30)
        /// </summary>
        /// <param name="timeout">超时时间</param>
        /// <returns></returns>
        IDynamicDBTBuilder SetCommandTimeout(int timeout);

        /// <summary>
        /// 添加实体模型构建过滤器
        /// </summary>
        /// <param name="filter">过滤器</param>
        /// <returns></returns>
        IDynamicDBTBuilder AddEntityTypeBuilderFilter(Action<EntityTypeBuilder> filter);

        /// <summary>
        /// 使用Code First进行迁移时忽略外键
        /// </summary>
        /// <returns></returns>
        IDynamicDBTBuilder MigrationsWithoutForeignKey();

        /// <summary>
        /// 是否在启动时自动创建分表,默认true
        /// </summary>
        /// <param name="enable">是否启用</param>
        /// <returns></returns>
        IDynamicDBTBuilder CreateShardingTableOnStarting(bool enable);

        /// <summary>
        /// 是否启用分表数据库迁移,默认false
        /// </summary>
        /// <param name="enable">是否启用</param>
        /// <returns></returns>
        IDynamicDBTBuilder EnableShardingMigration(bool enable);

        /// <summary>
        /// 是否启用注释,默认false
        /// </summary>
        /// <param name="enable">是否启用</param>
        /// <returns></returns>
        IDynamicDBTBuilder EnableComments(bool enable);
        /// <summary>
        /// 使用逻辑删除
        /// </summary>
        /// <param name="keyField">主键字段,字段类型为string</param>
        /// <param name="deletedField">已删除标志字段,字段类型为bool</param>
        /// <returns></returns>
        IDynamicDBTBuilder UseLogicDelete(string keyField = "Id", string deletedField = "Deleted");
        /// <summary>
        /// 设置记录执行SQL的最小执行时长，默认50ms
        /// </summary>
        /// <param name="minCommandElapsedMilliseconds"></param>
        /// <returns></returns>
        IDynamicDBTBuilder SetMinCommandElapsedMilliseconds(int minCommandElapsedMilliseconds);
        /// <summary>
        /// 使用默认数据库
        /// 注入IDbAccessor
        /// </summary>
        /// <param name="conString">连接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="entityNamespace">实体命名空间</param>
        /// <param name="optionsBuilder">自定义配置</param>
        /// <returns></returns>
        IDynamicDBTBuilder UseDatabase(string conString, DynamicDBType dbType, string entityNamespace = null, Action<DynamicDBTOption> optionsBuilder = null);
        /// <summary>
        /// 使用数据库
        /// </summary>
        /// <typeparam name="TDbAccessor">自定义的IDbAccessor</typeparam>
        /// <param name="conString">连接字符串</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="entityNamespace">实体命名空间</param>
        /// <param name="optionsBuilder">自定义配置</param>
        /// <returns></returns>
        IDynamicDBTBuilder UseDatabase<TDbAccessor>(string conString, DynamicDBType dbType, string entityNamespace = null, Action<DynamicDBTOption> optionsBuilder = null) where TDbAccessor : class, IDynamicDBTService;
        /// <summary>
        /// 使用默认数据库
        /// 注入IDbAccessor
        /// </summary>
        /// <param name="dbs">读写数据库配置</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="entityNamespace">实体命名空间</param>
        /// <param name="optionsBuilder">自定义配置</param>
        /// <returns></returns>
        IDynamicDBTBuilder UseDatabase((string connectionString, DynamicReadWriteType readWriteType)[] dbs, DynamicDBType dbType, string entityNamespace = null, Action<DynamicDBTOption> optionsBuilder = null);
        /// <summary>
        /// 使用数据库
        /// </summary>
        /// <typeparam name="TDbAccessor">自定义的IDbAccessor</typeparam>
        /// <param name="dbs">读写数据库配置</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="entityNamespace">实体命名空间</param>
        /// <param name="optionsBuilder">自定义配置</param>
        /// <returns></returns>
        IDynamicDBTBuilder UseDatabase<TDbAccessor>((string connectionString, DynamicReadWriteType readWriteType)[] dbs, DynamicDBType dbType, string entityNamespace = null, Action<DynamicDBTOption> optionsBuilder = null) where TDbAccessor : class, IDynamicDBTService;
        /// <summary>
        /// 添加数据源
        /// </summary>
        /// <param name="connectionString">连接字符串</param>
        /// <param name="readWriteType">读写模式</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="sourceName">数据源名</param>
        /// <returns></returns>
        IDynamicDBTBuilder AddDataSource(string connectionString, DynamicReadWriteType readWriteType, DynamicDBType dbType, string sourceName = "DefaultSource");
        /// <summary>
        /// 添加数据源
        /// </summary>
        /// <param name="dbs">数据库组</param>
        /// <param name="dbType">数据库类型</param>
        /// <param name="sourceName">数据源名</param>
        /// <returns></returns>
        IDynamicDBTBuilder AddDataSource((string connectionString, DynamicReadWriteType readWriteType)[] dbs, DynamicDBType dbType, string sourceName = "DefaultSource");
        /// <summary>
        /// 设置分表规则(哈希取模)
        /// 注:默认自动创建分表(若分表不存在)
        /// </summary>
        /// <typeparam name="TEntity">对应抽象表类型</typeparam>
        /// <param name="shardingField">分表字段</param>
        /// <param name="mod">取模</param>
        /// <param name="sourceName">数据源名</param>
        /// <returns></returns>
        IDynamicDBTBuilder SetHashModSharding<TEntity>(
            string shardingField,
            int mod,
            string sourceName = DynamicDBTCache.DefaultSource
            );
        /// <summary>
        /// 设置分表规则(哈希取模)
        /// 注:默认自动创建分表(若分表不存在)
        /// </summary>
        /// <typeparam name="TEntity">对应抽象表类型</typeparam>
        /// <param name="shardingField">分表字段</param>
        /// <param name="mod">取模</param>
        /// <param name="ranges">分库配置</param>
        /// <returns></returns>
        IDynamicDBTBuilder SetHashModSharding<TEntity>(
            string shardingField,
            int mod,
            params (int start, int end, string sourceName)[] ranges
            );
        /// <summary>
        /// 设置分表规则(按日期)
        /// </summary>
        /// <typeparam name="TEntity">对应抽象表类型</typeparam>
        /// <param name="shardingField">分表字段</param>
        /// <param name="expandByDateMode">扩容模式</param>
        /// <param name="startTime">开始时间</param>
        /// <param name="sourceName">数据源名</param>
        /// <returns></returns>
        IDynamicDBTBuilder SetDateSharding<TEntity>(
            string shardingField,
            DynamicExpandByDateMode expandByDateMode,
            DateTime startTime,
            string sourceName = DynamicDBTCache.DefaultSource
            );
        /// <summary>
        /// 设置分表规则(按日期)
        /// </summary>
        /// <typeparam name="TEntity">对应抽象表类型</typeparam>
        /// <param name="shardingField">分表字段</param>
        /// <param name="expandByDateMode">扩容模式</param>
        /// <param name="ranges">分库配置</param>
        /// <returns></returns>
        IDynamicDBTBuilder SetDateSharding<TEntity>(
            string shardingField,
            DynamicExpandByDateMode expandByDateMode,
            params (DateTime startTime, DateTime endTime, string sourceName)[] ranges);
    }
}

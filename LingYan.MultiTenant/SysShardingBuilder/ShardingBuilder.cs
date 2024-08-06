using LingYan.Model;
using LingYan.Model.ContextModel;
using LingYan.MultiTenant.SysExtension;
using LingYan.MultiTenant.SysMigrationsAssemblies;
using LingYan.MultiTenant.SysSharding;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ShardingCore;
using ShardingCore.Core.RuntimeContexts;
using ShardingCore.Core.ServiceProviders;
using ShardingCore.Core.ShardingConfigurations;
using ShardingCore.Core.ShardingConfigurations.Abstractions;
using ShardingCore.TableExists;
using ShardingCore.TableExists.Abstractions;

namespace LingYan.MultiTenant.SysShardingBuilder
{
    public class ShardingBuilder : IShardingBuilder
    {
        private readonly IServiceProvider _serviceProvider;

        public ShardingBuilder(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IShardingRuntimeContext Build(ShardingTenantOptions tenantOptions)
        {
            Type shardingRuntimeBuilderType = typeof(ShardingRuntimeBuilder<>);
            Type dbContextType = LYExpose.LYBuilderRuntimeManager.TenantTemplateDbContexts.FirstOrDefault();
            Type constructedType = shardingRuntimeBuilderType.MakeGenericType(dbContextType);
            var shardingRuntimeBuilderInstance = Activator.CreateInstance(constructedType);
            // 具体化后的类型确定后，获取具体化后的类型的MethodInfo
            Type specificType = shardingRuntimeBuilderInstance.GetType();
            //获取配置路由方法信息
            var UseRouteConfigMethodInfo = specificType.GetMethods().FirstOrDefault(f => f.Name == "UseRouteConfig" && f.GetParameters()[0].ParameterType == typeof(Action<IShardingRouteConfigOptions>));
            //配置路由委托
            var UseRouteConfigDelegate = new Action<IShardingRouteConfigOptions>((option) =>
            {
                if (LYExpose.LYBuilderRuntimeManager.VirtualTableList.Keys.Count > 0)
                {
                    if (tenantOptions.ShardingKeyType == ShardingKeyType.Mod)
                    {
                        LYExpose.LYBuilderRuntimeManager.VirtualTableList[ShardingKeyType.Mod].ForEach(modShardingTable =>
                        {
                            option.AddShardingTableRoute(modShardingTable);
                        });
                    }
                    if (tenantOptions.ShardingKeyType == ShardingKeyType.Time)
                    {
                        LYExpose.LYBuilderRuntimeManager.VirtualTableList[ShardingKeyType.Time].ForEach(modShardingTable =>
                        {
                            option.AddShardingTableRoute(modShardingTable);
                        });
                    }
                }
            });
            //执行配置
            UseRouteConfigMethodInfo.Invoke(shardingRuntimeBuilderInstance, new object[] { UseRouteConfigDelegate });
            //反射配置数据库方法信息
            var UseConfigMethodInfo = specificType.GetMethods().FirstOrDefault(f => f.Name == "UseConfig" && f.GetParameters()[0].ParameterType == typeof(Action<IShardingProvider, ShardingConfigOptions>));
            //配置数据库委托
            var UseConfigDelegate = new Action<IShardingProvider, ShardingConfigOptions>((provider, option) =>
            {
                option.ThrowIfQueryRouteNotMatch = false;
                option.UseShardingQuery((conStr, builder) =>
                {
                    if (tenantOptions.DataBaseType == DataBaseType.MYSQL)
                    {
                        builder.UseMySql(conStr, MySqlServerVersion.AutoDetect(conStr))
                            .UseMigrationNamespace(new MySqlMigrationNamespace());
                    }
                    if (tenantOptions.DataBaseType == DataBaseType.MSSQL)
                    {
                        builder.UseSqlServer(conStr)
                            .UseMigrationNamespace(new SqlServerMigrationNamespace());
                    }
                    builder.UseLoggerFactory(provider.GetService<ILoggerFactory>())
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                        .ReplaceService<IMigrationsAssembly, MultiDatabaseMigrationsAssembly>();
                });
                option.UseShardingTransaction((connection, builder) =>
                {
                    if (tenantOptions.DataBaseType == DataBaseType.MYSQL)
                    {
                        builder.UseMySql(connection, MySqlServerVersion.AutoDetect(connection.ConnectionString))
                        .UseMigrationNamespace(new MySqlMigrationNamespace());//迁移只会用connection string创建所以可以不加
                    }
                    if (tenantOptions.DataBaseType == DataBaseType.MSSQL)
                    {
                        builder.UseSqlServer(connection)
                        .UseMigrationNamespace(new SqlServerMigrationNamespace());
                    }
                    builder.UseLoggerFactory(provider.GetService<ILoggerFactory>())
                        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
                });
                option.AddDefaultDataSource(tenantOptions.DefaultDataSourceName, tenantOptions.DefaultConnectionString);
                //迁移配置
                option.UseShardingMigrationConfigure(b =>
                {
                    if (tenantOptions.DataBaseType == DataBaseType.MYSQL)
                    {
                        b.ReplaceService<IMigrationsSqlGenerator, ShardingMySqlMigrationsSqlGenerator>();
                    }
                    if (tenantOptions.DataBaseType == DataBaseType.MSSQL)
                    {
                        b.ReplaceService<IMigrationsSqlGenerator, ShardingSqlServerMigrationsSqlGenerator>();
                    }
                });
            });
            //执行
            UseConfigMethodInfo.Invoke(shardingRuntimeBuilderInstance, new object[] { UseConfigDelegate });
            //反射添加服务配置方法信息
            var AddServiceConfigureMethodInfo = specificType.GetMethods().FirstOrDefault(f => f.Name == "AddServiceConfigure" && f.GetParameters()[0].ParameterType == typeof(Action<IServiceCollection>));
            //添加服务配置委托
            var AddServiceConfigureDelegate = new Action<IServiceCollection>((service) =>
            {
                service.AddSingleton(tenantOptions);
            });
            //执行
            AddServiceConfigureMethodInfo.Invoke(shardingRuntimeBuilderInstance, new object[] { AddServiceConfigureDelegate });
            if (tenantOptions.DataBaseType == DataBaseType.MYSQL)
            {
                //反射替换服务方法信息
                var ReplaceServiceMethodInfo = specificType.GetMethod("ReplaceService").MakeGenericMethod(typeof(ITableEnsureManager), typeof(MySqlTableEnsureManager));
                ReplaceServiceMethodInfo.Invoke(shardingRuntimeBuilderInstance, new object[] { ServiceLifetime.Singleton });
            }
            if (tenantOptions.DataBaseType == DataBaseType.MSSQL)
            {
                //反射替换服务方法信息
                var ReplaceServiceMethodInfo = specificType.GetMethod("ReplaceService").MakeGenericMethod(typeof(ITableEnsureManager), typeof(SqlServerTableEnsureManager));
                ReplaceServiceMethodInfo.Invoke(shardingRuntimeBuilderInstance, new object[] { ServiceLifetime.Singleton });
            }
            //反射构建分片运行时方法信息
            var BuildMethodInfo = specificType.GetMethods().FirstOrDefault(w => w.Name == "Build" && w.GetParameters() != null && w.GetParameters().Count() == 1 && w.GetParameters()[0].ParameterType == typeof(IServiceProvider));
            return (IShardingRuntimeContext)BuildMethodInfo.Invoke(shardingRuntimeBuilderInstance, new object[] { _serviceProvider });
        }

    }
}

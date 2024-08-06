using CSRedis;
using LingYan.DynamicWebApi;
using LingYan.Extension;
using LingYan.Model;
using LingYan.Model.AbilityModel;
using LingYan.Model.BaseAttributes;
using LingYan.MultiTenant.SysShardingBuilder;
using LingYan.UnitOfWork.BaseUnitOfWork;
using LongYuBuilding.ShardingModule.MultiTenant.SysTenantProvider;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ShardingCore;
using ShardingCore.Core.RuntimeContexts;
using System.Reflection;
using System.Runtime.CompilerServices;
using Yitter.IdGenerator;
namespace LingYan.DDDModule
{
    public static class RegisterExtension
    {
        /// <summary>
        /// 注册模块
        /// </summary>
        /// <typeparam name="TWebApplicationBuilder"></typeparam>
        /// <param name="ModuleList"></param>
        /// <param name="builder"></param>
        public static void RegisterModule(this object builder, LYBuilderRuntimeModel lYBuilderRuntimeModel)
        {
            //模块的注册方法执行
            foreach (var module in lYBuilderRuntimeModel.ModuleList)
            {
                //方法
                var method = module.GetType().GetMethod("ARegisterModule");
                //参数
                var parameters = builder.GetInstanceParameters(method.GetParameters());
                //模块整体注册
                Console.WriteLine($"{method.Name}运行方法“ARegisterModule”，模块方法容器注册");
                method.Invoke(module, parameters);
            }
        }
        /// <summary>
        /// 注册仓储与工作单元
        /// </summary>
        /// <param name="BuilderService"></param>
        public static void RegisterRepository(this IServiceCollection BuilderService, LYBuilderRuntimeModel lYBuilderRuntimeModel)
        {
            foreach (var dbcontext in lYBuilderRuntimeModel.ModuleDbContextList.Keys)
            {
                var dbsetGener = typeof(DbSet<>);
                var entitys = dbcontext.GetProperties().Where(w => w.PropertyType.IsGenericType && w.PropertyType.GetGenericTypeDefinition() == dbsetGener)
                     .Select(w => w.PropertyType.GetGenericArguments()[0])
                     .ToList();
                BuilderService.AddRepositorys(dbcontext, entitys.ToArray());
                BuilderService.AddUnitOfWorks(dbcontext);
            }
        }
        /// <summary>
        /// 注册实例
        /// </summary>
        /// <typeparam name="TBuilder"></typeparam>
        /// <param name="BuilderService"></param>
        /// <param name="builder"></param>
        public static void RegisterTInstance(this IServiceCollection BuilderService, LYBuilderRuntimeModel lYBuilderRuntimeModel)
        {
            foreach (var service in lYBuilderRuntimeModel.ModuleTInstance)
            {
                //var Parameters = service.GetConstructors().FirstOrDefault().GetParameters();
                //var objects= builder.GetInstanceParameters(Parameters);
                //var serviceInstance = Activator.CreateInstance(service, objects);
                BuilderService.AddSingleton(service);
            }
        }
        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="BuilderService"></param>
        public static void RegisterTService(this IServiceCollection BuilderService, LYBuilderRuntimeModel lYBuilderRuntimeModel)
        {
            foreach (var service in lYBuilderRuntimeModel.ModuleTService)
            {
                var tservice = (LYTServiceAttribute)service.GetCustomAttributes().FirstOrDefault(f => f.GetType() == typeof(LYTServiceAttribute));
                switch (tservice.ServiceLifetime)
                {
                    case ServiceLifetime.Transient:
                        BuilderService.AddTransient(tservice.TService, service);
                        break;
                    case ServiceLifetime.Scoped:
                        BuilderService.AddScoped(tservice.TService, service);
                        break;
                    case ServiceLifetime.Singleton:
                        BuilderService.AddSingleton(tservice.TService, service);
                        break;
                }
            }
        }
        /// <summary>
        /// 注册Manager
        /// </summary>
        /// <param name="BuilderService"></param>
        public static void RegisterManager(this IServiceCollection BuilderService, LYBuilderRuntimeModel lYBuilderRuntimeModel)
        {
            foreach (var manager in lYBuilderRuntimeModel.ModuleManagerList)
            {
                BuilderService.AddScoped(manager);
            }
        }
        /// <summary>
        /// 注册数据库上下文
        /// </summary>
        public static void RegisterDbContext(this IServiceCollection BuilderService, LYBuilderRuntimeModel lYBuilderRuntimeModel)
        {
            foreach (var dbContextType in lYBuilderRuntimeModel.ModuleDbContextList.Keys)
            {
                var attribute = dbContextType.GetCustomAttribute<LYDbContextAttribute>();
                if (attribute != null && !string.IsNullOrEmpty(attribute.ConnectionString))
                {
                    // 获取 AddDbContext 泛型方法
                    var addDbContextMethod = typeof(EntityFrameworkServiceCollectionExtensions).GetMethods()
                        .Where(m => m.Name == "AddDbContext" && m.GetParameters().Length == 4)
                        .FirstOrDefault();
                    // 数据库连接字符串
                    var connectionString = LYExpose.MysqlConfigModel.Others[attribute.ConnectionString];
                    //依据数据库连接字符串获取数据库版本
                    var serverVersion = ServerVersion.AutoDetect(connectionString);
                    //配置行为
                    var optionsAction = new Action<DbContextOptionsBuilder>(opt =>
                    {
                        opt.UseMySql(connectionString, serverVersion)
                           .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                           .EnableSensitiveDataLogging();
                    });
                    // 调用 AddDbContext 泛型方法注入数据库上下文
                    addDbContextMethod.MakeGenericMethod(dbContextType).Invoke(null, new object[] { BuilderService, optionsAction, ServiceLifetime.Scoped, ServiceLifetime.Scoped });
                }
            }
        }
        /// <summary>
        /// 注册雪花id
        /// </summary>
        /// <param name="BuilderService"></param>
        public static void RegisterYitIdHelper(this IServiceCollection BuilderService, LYBuilderRuntimeModel lYBuilderRuntimeModel)
        {
            if (LYExpose.IdGeneratorOptionConfigModel != null)
            {
                //初始化雪花ID|||最大
                var idGeneratorOptions = new IdGeneratorOptions();
                idGeneratorOptions.WorkerId = LYExpose.IdGeneratorOptionConfigModel.WorkerId;
                idGeneratorOptions.WorkerIdBitLength = LYExpose.IdGeneratorOptionConfigModel.WorkerIdBitLength;
                idGeneratorOptions.SeqBitLength = LYExpose.IdGeneratorOptionConfigModel.SeqBitLength;
                idGeneratorOptions.BaseTime = LYExpose.IdGeneratorOptionConfigModel.BaseTime;
                YitIdHelper.SetIdGenerator(idGeneratorOptions);
            }
        }
        /// <summary>
        /// 注册redis缓存
        /// </summary>
        /// <param name="BuilderService"></param>
        public static void RegisterRedis(this IServiceCollection BuilderService, LYBuilderRuntimeModel lYBuilderRuntimeModel)
        {
            try
            {
                var backBody = RegisterRedisCilent();
                if (backBody!=null)
                {
                    Console.WriteLine("Redis注册成功");
                    RedisHelper.Initialization(backBody);
                }
                else
                {
                    Console.WriteLine("Redis注册失败");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("redis配置出错" + ex.Message);
            }
        }
        private static CSRedisClient RegisterRedisCilent()
        {
            switch (LYExpose.RedisCofigModel.Pattern)
            {
                case "Single":
                    return new CSRedisClient(LYExpose.RedisCofigModel.Single);
                case "Sentinel":
                    return new CSRedisClient(LYExpose.RedisCofigModel.SentinelModel.Master, LYExpose.RedisCofigModel.SentinelModel.Slave.Split("*"));
                case "Cluster":
                    return new CSRedisClient(null, LYExpose.RedisCofigModel.Cluster.Split("*"));
                default:
                    return null;
            }
        }
        /// <summary>
        /// 注册动态路由
        /// </summary>
        /// <param name="BuilderService"></param>
        public static void RegisterDynamicWebApi(this IServiceCollection BuilderService, LYBuilderRuntimeModel lYBuilderRuntimeModel)
        {
            BuilderService.AddControllers(options =>
            {
                //路由规则
                options.UseCentralRouteJsonConfig(lYBuilderRuntimeModel.ConfigurationManager);
            }).ConfigureApplicationPartManager(t =>
            {
                //路由扫射
                t.FeatureProviders.Add(new CoreDynamicExtendControlleFeatureProvider());
            });
        }
        /// <summary>
        /// 注册表单大小
        /// </summary>
        /// <param name="BuilderService"></param>
        public static void RegisterFormSize(this IServiceCollection BuilderService)
        {
            //设置文件上传大小限制
            //用于处理传入的HTTP请求并生成响应。Kestrel位于应用程序层之下，
            //处理从客户端（浏览器）发送的请求，然后将请求转发给应用程序进行处理
            BuilderService.Configure<FormOptions>(x =>
            {
                x.MultipartBodyLengthLimit = 1024 * 1024 * 1000;//100M
            });
            BuilderService.Configure<KestrelServerOptions>(options =>
            {
                options.Limits.MaxRequestBodySize = 1024 * 1024 * 1000;//100M
            });
            BuilderService.Configure<IISServerOptions>(options =>
            {
                options.MaxRequestBodySize = 1024 * 1024 * 1000;//100M
            });
        }
        /// <summary>
        /// 注册多租户模版数据上下文
        /// </summary>
        /// <param name="BuilderService"></param>
        public static void RegisterTenantTemplateDbContext(this IServiceCollection BuilderService, LYBuilderRuntimeModel lYBuilderRuntimeModel)
        {
            //Add-Migration InitialCreate -Context TenantDbContext -OutputDir Migrations\SqlServer -Args "--provider SqlServer"
            //Add-Migration InitialCreate -Context TenantDbContext -OutputDir Migrations\MySql -Args "--provider MySql"   
            //反射获取注册数据库上下文扩展
            var addDbContextMethod = typeof(EntityFrameworkServiceCollectionExtensions).GetMethods()
                .Where(m => m.Name == "AddDbContext" && m.GetParameters()[1].ParameterType == typeof(Action<IServiceProvider, DbContextOptionsBuilder>))
                .FirstOrDefault();
            //反射获取初始化多租户扩展
            var useDefaultShardingMethod = typeof(ShardingCoreExtension).GetMethods()
              .Where(m => m.Name == "UseDefaultSharding" && m.GetParameters().Length == 2 && m.GetParameters()[0].ParameterType == typeof(DbContextOptionsBuilder)
              && m.GetParameters()[1].ParameterType == typeof(IShardingRuntimeContext)).FirstOrDefault();
            //注册数据库上下文的委托、如果有上下文那么创建租户dbcontext否则就是启动命令Add-Migration
            var optionsAction = new Action<IServiceProvider, DbContextOptionsBuilder>((provider, optionBuilder) =>
            {
                var tenantManager = provider.GetService<ITenantManager>();
                var currentTenantContext = tenantManager.GetCurrentTenantContext();
                if (currentTenantContext != null)
                {
                    var shardingRuntimeContext = currentTenantContext.GetShardingRuntimeContext();
                    useDefaultShardingMethod.MakeGenericMethod(lYBuilderRuntimeModel.TenantTemplateDbContexts.FirstOrDefault()).Invoke(null, new object[] { optionBuilder, shardingRuntimeContext });
                }
            });
            //注册多租户数据上下文模版
            if (lYBuilderRuntimeModel.TenantTemplateDbContexts.FirstOrDefault() != null)
            {
                addDbContextMethod.MakeGenericMethod(lYBuilderRuntimeModel.TenantTemplateDbContexts.FirstOrDefault()).Invoke(null, new object[] { BuilderService, optionsAction, ServiceLifetime.Scoped, ServiceLifetime.Scoped });
            }
        }
        /// <summary>
        /// 注册多租户管理
        /// </summary>
        /// <param name="BuilderService"></param>
        public static void RegisterTenantService(this IServiceCollection BuilderService)
        {
            BuilderService.AddSingleton<ITenantManager, TenantManager>();
            BuilderService.AddSingleton<ITenantContextAccessor, TenantContextAccessor>();
            BuilderService.AddSingleton<IShardingBuilder, ShardingBuilder>();
        }
    }
}

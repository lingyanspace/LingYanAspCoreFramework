using CSRedis;
using LingYanAspCoreFramework.Attributes;
using LingYanAspCoreFramework.DynamicApis;
using LingYanAspCoreFramework.Events;
using LingYanAspCoreFramework.Helpers;
using LingYanAspCoreFramework.Models;
using LingYanAspCoreFramework.MultiTenants;
using LingYanAspCoreFramework.Roots;
using LingYanAspCoreFramework.UnitOfWork;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog.Web;
using ShardingCore;
using ShardingCore.Core.RuntimeContexts;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using Yitter.IdGenerator;

namespace LingYanAspCoreFramework.Extensions
{
    public static class IocExtension
    {
        /// <summary>
        /// 获取项目路由
        /// </summary>
        /// <param name="actionDescriptorCollectionProvider"></param>
        /// <returns></returns>
        public static List<LYRouteModel> GetProjectRoute(this IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            var routes = actionDescriptorCollectionProvider.ActionDescriptors.Items.Select(x =>
            {
                var lyRouteModel = new LYRouteModel();
                lyRouteModel.ActionName = x.RouteValues["Action"];
                lyRouteModel.ControllerName = x.RouteValues["Controller"];
                lyRouteModel.HttpMethod = x.ActionConstraints?.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods.First();
                lyRouteModel.TemplatePath = "/" + x.AttributeRouteInfo.Template;
                lyRouteModel.PrefixName = lyRouteModel.TemplatePath.Replace(lyRouteModel.ActionName, "").Replace(lyRouteModel.ControllerName, "").Replace("/", "");
                return lyRouteModel;
            }).ToList();
            return routes;
        }
        /// <summary>
        /// 获取模块集合初始化（将类加载到运行时中）
        /// </summary>
        /// <param name="lYBuilderRuntimeModel"></param>
        private static void GetModules(this RuntimeCacheModel lYBuilderRuntimeModel)
        {
            //当前项目
            var currentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            //获取项目
            var compilationLibrary = DependencyContext.Default.CompileLibraries
                .Where(x => !x.Serviceable && x.Type != "package" && x.Type == "project" && x.Name != currentAssemblyName);
            //所有类型
            foreach (var singleLib in compilationLibrary)
            {
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(singleLib.Name));
                //当前模块
                var currentModule = assembly.GetTypes()
                    .Where(x => !x.IsAbstract && !x.IsInterface && (x.IsAssignableToGenericType(typeof(BaseModule<,>)) || x.IsAssignableToGenericType(typeof(BaseModule<,,>))))
                    .FirstOrDefault();
                //获得多租户基类实现
                assembly.GetTenantBaseEntitys(lYBuilderRuntimeModel);
                //获得DbContext
                assembly.GetDbContexts(lYBuilderRuntimeModel);
                //获得Manager
                assembly.GetManagers(lYBuilderRuntimeModel);
                //获取服务
                assembly.GetTService(lYBuilderRuntimeModel);
                //获取实例
                assembly.GetInstances(lYBuilderRuntimeModel);
                //获取过滤器实例
                assembly.GetFilers(lYBuilderRuntimeModel);
                //获得模块
                if (currentModule != null)
                {
                    //实例
                    var moduleinstance = Activator.CreateInstance(currentModule);
                    //添加实例
                    lYBuilderRuntimeModel.ModuleList.Add(moduleinstance);
                }
            }
            //模块排序
            lYBuilderRuntimeModel.ModuleList = lYBuilderRuntimeModel.ModuleList.OrderBy(s => (int)s.GetType().GetProperty("PageIndex").GetValue(s)).ToList();
        }
        /// <summary>
        /// 获取多租户实体
        /// </summary>
        /// <param name="assembly"></param>
        private static void GetTenantBaseEntitys(this Assembly assembly, RuntimeCacheModel lYBuilderRuntimeModel)
        {
            var moduleEntity = assembly.GetTypes()
                .Where(x => x.BaseType != null && x.BaseType.IsGenericType &&
                ((x.BaseType.GetGenericTypeDefinition() == typeof(BaseSysOwner<>) ||
                x.BaseType.GetGenericTypeDefinition() == typeof(BaseSysOwnerTenantConfig<>)) &&
                x.BaseType.GetGenericArguments().Length > 0 &&
                !assembly.GetTypes().Any(z => z.BaseType == x))).ToList();
            if (moduleEntity != null && moduleEntity.Count > 0)
            {
                moduleEntity.ForEach(f =>
                {
                    lYBuilderRuntimeModel.ModuleTenantBaseEntitys.TryAdd(f.BaseType.Name.Replace("`1", ""), f);
                });

            }
        }
        /// <summary>
        /// 获取数据库上下文
        /// </summary>
        /// <param name="assembly"></param>
        private static void GetDbContexts(this Assembly assembly, RuntimeCacheModel lYBuilderRuntimeModel)
        {
            //获得DbContext
            var moduleDbContext = assembly.GetTypes()
                .Where(x => x.GetCustomAttributes(true)
               .Any(y => y.GetType() == typeof(LYDbContextAttribute)) &&
               !assembly.GetTypes().Any(z => z.BaseType == x))
                .ToList();
            if (moduleDbContext != null && moduleDbContext.Count > 0)
            {
                moduleDbContext.ForEach(dbcontext =>
                {
                    var longyudb = dbcontext.GetCustomAttribute<LYDbContextAttribute>();
                    var value = longyudb.GetPropertyValue<DbContextType>("DbContextType");
                    if (value != DbContextType.TenantTemplateDbContext)
                    {
                        lYBuilderRuntimeModel.ModuleDbContextList.TryAdd(dbcontext, dbcontext.GetCustomAttribute<LYDbContextAttribute>().DbContextType);
                    }
                    else if (value == DbContextType.TenantTemplateDbContext)
                    {
                        lYBuilderRuntimeModel.TenantTemplateDbContexts.Add(dbcontext);
                        lYBuilderRuntimeModel.VirtualTableList.TryAdd(ShardingKeyType.Mod, longyudb.GetPropertyValue<Type[]>("ShardingTable").Where(w =>
                        w.BaseType != null && w.BaseType.IsGenericType &&
                        w.BaseType.GetGenericTypeDefinition().Name == "AbstractSimpleShardingModKeyIntVirtualTableRoute`1" &&
                        w.BaseType.GetGenericArguments().Length > 0).ToList());
                        lYBuilderRuntimeModel.VirtualTableList.TryAdd(ShardingKeyType.Time, longyudb.GetPropertyValue<Type[]>("ShardingTable").Where(w =>
                        w.BaseType != null && w.BaseType.IsGenericType &&
                        w.BaseType.GetGenericTypeDefinition().Name != "AbstractSimpleShardingModKeyIntVirtualTableRoute`1" &&
                        w.BaseType.GetGenericArguments().Length > 0).ToList());

                    }
                });
            }
        }
        /// <summary>
        /// 获取瞬态服务
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="lYBuilderRuntimeModel"></param>
        private static void GetTService(this Assembly assembly, RuntimeCacheModel lYBuilderRuntimeModel)
        {
            var moduleDbContext = assembly.GetTypes()
                .Where(x => x.GetCustomAttributes(true)
               .Any(y => y.GetType() == typeof(LYTServiceAttribute)) &&
               !assembly.GetTypes().Any(z => z.BaseType == x))
                .ToList();
            if (moduleDbContext != null)
            {
                lYBuilderRuntimeModel.ModuleTService.AddRange(moduleDbContext);
            }
        }
        /// <summary>
        /// 获取Domain领域服务
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="lYBuilderRuntimeModel"></param>
        private static void GetManagers(this Assembly assembly, RuntimeCacheModel lYBuilderRuntimeModel)
        {
            var moduleEntity = assembly.GetTypes()
                 .Where(x => x.GetCustomAttributes(true)
                .Any(y => y.GetType() == typeof(LYManagerAttribute)) &&
                !assembly.GetTypes().Any(z => z.BaseType == x))
                 .ToList();
            if (moduleEntity != null)
            {
                lYBuilderRuntimeModel.ModuleManagerList.AddRange(moduleEntity);
            }
        }
        /// <summary>
        /// 获取注册实例
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="lYBuilderRuntimeModel"></param>
        private static void GetInstances(this Assembly assembly, RuntimeCacheModel lYBuilderRuntimeModel)
        {
            var moduleEntity = assembly.GetTypes()
                 .Where(x => x.GetCustomAttributes(true)
                .Any(y => y.GetType() == typeof(LYInstanceAttribute)) &&
                !assembly.GetTypes().Any(z => z.BaseType == x))
                 .ToList();
            if (moduleEntity != null)
            {
                lYBuilderRuntimeModel.ModuleTInstance.AddRange(moduleEntity);
            }
        }
        /// <summary>
        /// 获取过滤器实例
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="lYBuilderRuntimeModel"></param>
        private static void GetFilers(this Assembly assembly, RuntimeCacheModel lYBuilderRuntimeModel)
        {
            var moduleEntity = assembly.GetTypes()
                 .Where(x => x.GetCustomAttributes(true)
                .Any(y => y.GetType() == typeof(LYFilerAttribute)) &&
                !assembly.GetTypes().Any(z => z.BaseType == x))
                 .ToList();
            if (moduleEntity != null)
            {
                lYBuilderRuntimeModel.ModuleFiler.AddRange(moduleEntity);
            }
        }
        /// <summary>
        /// 注册模块
        /// </summary>
        /// <typeparam name="TWebApplicationBuilder"></typeparam>
        /// <param name="ModuleList"></param>
        /// <param name="builder"></param>
        private static void RegisterModule(this object builder, RuntimeCacheModel lYBuilderRuntimeModel)
        {
            //模块的注册方法执行
            foreach (var module in lYBuilderRuntimeModel.ModuleList)
            {
                //模块整体注册
                ConsoleHelper.DefaultLog($"【模块注入容器方法执行】{module.GetType().Name}运行方法ARegisterModule...");
                //方法
                var method = module.GetType().GetMethod("ARegisterModule");
                //参数
                var parameters = builder.GetInstanceParameters(method.GetParameters());
                method.Invoke(module, parameters);
            }
        }
        /// <summary>
        /// 注册仓储与工作单元
        /// </summary>
        /// <param name="BuilderService"></param>
        private static void RegisterRepository(this IServiceCollection BuilderService, RuntimeCacheModel lYBuilderRuntimeModel)
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
        private static void RegisterTInstance(this IServiceCollection BuilderService, RuntimeCacheModel lYBuilderRuntimeModel)
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
        private static void RegisterTService(this IServiceCollection BuilderService, RuntimeCacheModel lYBuilderRuntimeModel)
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
        private static void RegisterManager(this IServiceCollection BuilderService, RuntimeCacheModel lYBuilderRuntimeModel)
        {
            foreach (var manager in lYBuilderRuntimeModel.ModuleManagerList)
            {
                BuilderService.AddScoped(manager);
            }
        }
        /// <summary>
        /// 注册数据库上下文
        /// </summary>
        private static void RegisterDbContext(this IServiceCollection BuilderService, RuntimeCacheModel lYBuilderRuntimeModel)
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
                    var connectionString = LingYanRuntimeManager.MysqlConfigModel.Others[attribute.ConnectionString];
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
        private static void RegisterYitIdHelper(this IServiceCollection BuilderService, RuntimeCacheModel lYBuilderRuntimeModel)
        {
            if (LingYanRuntimeManager.IdGeneratorOptionConfigModel != null)
            {
                //初始化雪花ID|||最大
                var idGeneratorOptions = new IdGeneratorOptions();
                idGeneratorOptions.WorkerId = LingYanRuntimeManager.IdGeneratorOptionConfigModel.WorkerId;
                idGeneratorOptions.WorkerIdBitLength = LingYanRuntimeManager.IdGeneratorOptionConfigModel.WorkerIdBitLength;
                idGeneratorOptions.SeqBitLength = LingYanRuntimeManager.IdGeneratorOptionConfigModel.SeqBitLength;
                idGeneratorOptions.BaseTime = LingYanRuntimeManager.IdGeneratorOptionConfigModel.BaseTime;
                YitIdHelper.SetIdGenerator(idGeneratorOptions);
            }
        }
        /// <summary>
        /// 注册redis缓存
        /// </summary>
        /// <param name="BuilderService"></param>
        private static void RegisterRedis(this IServiceCollection BuilderService, RuntimeCacheModel lYBuilderRuntimeModel)
        {
            try
            {
                var backBody = RegisterRedisCilent();
                if (backBody != null)
                {
                    ConsoleHelper.SuccessLog("【Redis注册成功】...");
                    RedisHelper.Initialization(backBody);
                }
                else
                {
                    ConsoleHelper.WarnrningLog("【Redis注册失败】...");
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.ErrorLog($"【redis配置出错】{ex.Message}...");
            }
        }
        private static CSRedisClient RegisterRedisCilent()
        {
            switch (LingYanRuntimeManager.RedisCofigModel.Pattern)
            {
                case "Single":
                    return new CSRedisClient(LingYanRuntimeManager.RedisCofigModel.Single);
                case "Sentinel":
                    return new CSRedisClient(LingYanRuntimeManager.RedisCofigModel.SentinelModel.Master, LingYanRuntimeManager.RedisCofigModel.SentinelModel.Slave.Split("*"));
                case "Cluster":
                    return new CSRedisClient(null, LingYanRuntimeManager.RedisCofigModel.Cluster.Split("*"));
                default:
                    return null;
            }
        }
        /// <summary>
        /// 注册动态路由
        /// </summary>
        /// <param name="BuilderService"></param>
        private static void RegisterDynamicWebApi(this IServiceCollection BuilderService, RuntimeCacheModel lYBuilderRuntimeModel)
        {
            BuilderService.AddControllers(options =>
            {
                //路由规则
                options.UseCentralRouteJsonConfig(lYBuilderRuntimeModel.LingYanConfiguration);
            }).ConfigureApplicationPartManager(t =>
            {
                //路由扫射
                t.FeatureProviders.Add(new DynamicControlleFeatureProvider());
            });
        }
        /// <summary>
        /// 注册表单大小
        /// </summary>
        /// <param name="BuilderService"></param>
        private static void RegisterFormSize(this IServiceCollection BuilderService)
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
        private static void RegisterTenantTemplateDbContext(this IServiceCollection BuilderService, RuntimeCacheModel lYBuilderRuntimeModel)
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
        private static void RegisterTenantService(this IServiceCollection BuilderService)
        {
            BuilderService.AddSingleton<ITenantManager, TenantManager>();
            BuilderService.AddSingleton<ITenantContextAccessor, TenantContextAccessor>();
            BuilderService.AddSingleton<IShardingBuilder, ShardingBuilder>();
        }
        /// <summary>
        /// 初始化模块
        /// </summary>
        /// <param name="ModuleList"></param>
        /// <param name="web"></param>
        private static void InitModules(this List<object> ModuleList, object web)
        {
            foreach (var module in ModuleList)
            {
                //模块整体注册
                ConsoleHelper.DefaultLog($"【模块初始化方法执行】{module.GetType().Name}运行方法BInitializationModule...");
                //方法
                var method = module.GetType().GetMethod("BInitializationModule");
                //参数
                var parameters = web.GetInstanceParameters(method.GetParameters());
                method.Invoke(module, parameters);
            }
        }
        /// <summary>
        /// 反射获取属性的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            PropertyInfo propertyType = obj.GetType().GetProperty(propertyName);
            var peropertyValue = propertyType.GetValue(obj);
            return (T)peropertyValue;
        }
        /// <summary>
        /// 判断泛型方法
        /// </summary>
        /// <param name="givenType"></param>
        /// <param name="genericType"></param>
        /// <returns></returns>
        private static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            if (givenType == null || genericType == null)
                return false;

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            foreach (var interfaceType in givenType.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == genericType)
                    return true;
            }
            var baseType = givenType.BaseType;
            if (baseType == null)
                return false;
            return baseType.IsAssignableToGenericType(genericType);
        }
        /// <summary>
        /// 获取实例需要传入的参数
        /// </summary>
        /// <typeparam name="TWebApplicationBuilder"></typeparam>
        /// <param name="builder"></param>
        /// <param name="parameterInfos"></param>
        /// <returns></returns>
        private static object[] GetInstanceParameters<TWebApplicationBuilder>(this TWebApplicationBuilder builder, ParameterInfo[] parameterInfos) where TWebApplicationBuilder : class
        {
            object[] objects = new object[parameterInfos.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                if (parameterInfos[i].ParameterType == builder.GetType())
                {
                    objects[i] = builder;
                }
                else
                {
                    var matchingProperties = builder.GetType().GetProperties()
                        .Where(f => f.PropertyType == parameterInfos[i].ParameterType || parameterInfos[i].ParameterType.IsAssignableFrom(f.PropertyType));
                    if (matchingProperties?.Count() > 0)
                    {
                        objects[i] = matchingProperties.FirstOrDefault()?.GetValue(builder);
                    }
                }
            }
            return objects;
        }
        /// <summary>
        /// 获取容器
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        private static IServiceCollection GetFirstIServiceCollection(this object[] objects)
        {
            foreach (var objs in objects)
            {
                var result = objs.GetType().GetProperties()
                    .FirstOrDefault(w => typeof(IServiceCollection).IsAssignableFrom(w.PropertyType));

                if (result != null)
                {
                    return (IServiceCollection)result.GetValue(objs);
                }
            }
            return null;
        }
        /// <summary>
        /// 获取Builder的容器
        /// </summary>
        /// <typeparam name="Builder"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        private static IServiceCollection GetBuilderServiceCollection<Builder>(this Builder builder) where Builder : class
        {
            if (builder.GetType() == typeof(IServiceCollection))
            {
                return (IServiceCollection)builder;
            }
            else
            {
                var matchingProperties = builder.GetType().GetProperties().Where(f => f.PropertyType == typeof(IServiceCollection));
                return (IServiceCollection)matchingProperties.FirstOrDefault()?.GetValue(builder);
            }
        }
        /// <summary>
        /// 获取TWeb的容器管理员
        /// </summary>
        /// <typeparam name="TApp"></typeparam>
        /// <param name="app"></param>
        /// <returns></returns>
        private static IServiceProvider GetBuilderServiceProvider<TApp>(this TApp app) where TApp : class
        {
            if (app.GetType() == typeof(IServiceProvider))
            {
                return (IServiceProvider)app;
            }
            else
            {
                var matchingProperties = app.GetType().GetProperties().Where(f => f.PropertyType == typeof(IServiceProvider));
                return (IServiceProvider)matchingProperties.FirstOrDefault()?.GetValue(app);
            }
        }
        /// <summary>
        /// 注册项目模块+附带注册服务
        /// </summary>
        public static void AppRegisterProjectModule(this object builder)
        {
            try
            {
                ConsoleHelper.DefaultLog("【注入灵燕框架】...");
                LingYanRuntimeManager.Init();
                ConsoleHelper.DefaultLog("【配置系统appsettings.json】...");
                LingYanRuntimeManager.SysConfiguration = builder.GetPropertyValue<ConfigurationManager>("Configuration");
                ConsoleHelper.DefaultLog("【临时归纳项目模块】...");
                LingYanRuntimeManager.RuntimeCacheModel.GetModules();
                ConsoleHelper.DefaultLog("【注册项目模块】...");
                builder.RegisterModule(LingYanRuntimeManager.RuntimeCacheModel);
                ConsoleHelper.DefaultLog("【获取容器】...");
                var BuilderService = builder.GetBuilderServiceCollection();
                ConsoleHelper.DefaultLog("【容器注册官方通用服务】...");
                if (builder is WebApplicationBuilder web)
                {
                    web.RegisterProjectSelf();
                }
                ConsoleHelper.DefaultLog("【注册仓储与工作单元】...");
                BuilderService.RegisterRepository(LingYanRuntimeManager.RuntimeCacheModel);
                ConsoleHelper.DefaultLog("【注册领域Manager】...");
                BuilderService.RegisterManager(LingYanRuntimeManager.RuntimeCacheModel);
                ConsoleHelper.DefaultLog("【注册服务TService】...");
                BuilderService.RegisterTService(LingYanRuntimeManager.RuntimeCacheModel);
                ConsoleHelper.DefaultLog("【注册本地事件总线】...");
                BuilderService.AddSingleton<ILYEventBus, LYEventBus>();
                ConsoleHelper.DefaultLog("【注册实例】...");
                BuilderService.RegisterTInstance(LingYanRuntimeManager.RuntimeCacheModel);
                ConsoleHelper.DefaultLog("【注册数据库上下文】...");
                BuilderService.RegisterDbContext(LingYanRuntimeManager.RuntimeCacheModel);
                ConsoleHelper.DefaultLog("【初始化雪花ID】...");
                BuilderService.RegisterYitIdHelper(LingYanRuntimeManager.RuntimeCacheModel);
                ConsoleHelper.DefaultLog("【注册Redis】...");
                BuilderService.RegisterRedis(LingYanRuntimeManager.RuntimeCacheModel);
                ConsoleHelper.DefaultLog("【注册动态路由】...");
                BuilderService.RegisterDynamicWebApi(LingYanRuntimeManager.RuntimeCacheModel);
                ConsoleHelper.DefaultLog("【注册配置表单大小限制】...");
                BuilderService.RegisterFormSize();
                ConsoleHelper.DefaultLog("【注册多租户服务】...");
                BuilderService.RegisterTenantService();
                ConsoleHelper.DefaultLog("【注册多租户模版数据上下文】...");
                BuilderService.RegisterTenantTemplateDbContext(LingYanRuntimeManager.RuntimeCacheModel);
            }
            catch (Exception ex)
            {
                ConsoleHelper.ErrorLog($"【全局注册出错】{ex.Message}...");
            }
        }
        /// <summary>
        /// 初始化项目模块+附带初始化服务
        /// </summary>
        public static void AppInitializationProjectModule(this object app)
        {
            try
            {
                ConsoleHelper.SuccessLog($"【全局初始化开始】...");
                if (app is WebApplication web)
                {
                    web.InitProjectSelf();
                }
            }
            catch (Exception ex)
            {
                ConsoleHelper.ErrorLog($"【全局初始化出错】{ex.Message}...");
            }
        }
        /// <summary>
        /// 容器注入框架服务
        /// </summary>
        /// <param name="builder"></param>
        private static void RegisterProjectSelf(this WebApplicationBuilder builder)
        {
            ConsoleHelper.DefaultLog("【日志清除自带并添加Nlog日志配置】...");
            builder.Logging.ClearProviders();
            builder.Logging.AddNLog(LingYanRuntimeManager.CommonConfigModel.NlogConfig);
            ConsoleHelper.DefaultLog("【添加API端点】...");
            builder.Services.AddEndpointsApiExplorer();
            ConsoleHelper.DefaultLog("【添加SignalR】...");
            builder.Services.AddSignalR();
            ConsoleHelper.DefaultLog("【添加本地MemoryCache缓存】...");
            builder.Services.AddSingleton<IMemoryCache, MemoryCache>();
            ConsoleHelper.DefaultLog("【添加跨域策略】...");
            if (LingYanRuntimeManager.CrossDomains != null)
            {
                builder.Services.AddCors(c => c.AddPolicy(LingYanRuntimeManager.CrossPolicy, p => p.WithOrigins(LingYanRuntimeManager.CrossDomains).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
            }
            ConsoleHelper.DefaultLog("【添加安全定义】...");
            builder.Services.AddSwaggerGen(swaggerGenOption =>
            {
                // [ApiExplorerSettings(GroupName = "v2")]
                //接口文档
                swaggerGenOption.SwaggerDoc("dev1", new OpenApiInfo { Title = LingYanRuntimeManager.ProjectName, Version = "v1" });
                //注解配置
                swaggerGenOption.IncludeXmlComments(LingYanRuntimeManager.HostPhysicsRoot.GetLocalUrl(LingYanRuntimeManager.ProjectName+".xml"), true);
                //接口排序
                swaggerGenOption.OrderActionsBy(apiDescription => apiDescription.RelativePath.Length.ToString());
                //添加访问前缀
                var ip4LocalArea = IPHelper.GetInternalIPv4Address();
                LingYanRuntimeManager.ListeningPorts.ToList().ForEach(port =>
                {
                    swaggerGenOption.AddServer(new OpenApiServer() { Url = port.Replace("*", "localhost"), Description = "本机回环Server" });
                    swaggerGenOption.AddServer(new OpenApiServer() { Url = port.Replace("*", "127.0.0.1"), Description = "物理回环Server" });
                    if (!string.IsNullOrEmpty(ip4LocalArea))
                    {
                        swaggerGenOption.AddServer(new OpenApiServer() { Url = port.Replace("*", ip4LocalArea), Description = "局域网Server" });
                    }
                });
                // 自定义Swagger文档中每个操作的ID
                swaggerGenOption.CustomOperationIds(apiDesc =>
                {
                    var controllerAction = apiDesc.ActionDescriptor as ControllerActionDescriptor;
                    return controllerAction.ControllerName + "-" + controllerAction.ActionName;
                });
                //添加JWT认证方式
                swaggerGenOption.AddSecurityDefinition(LingYanRuntimeManager.BearerScheme, new OpenApiSecurityScheme
                {
                    Description = "格式：Bearer {Token}",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    BearerFormat = "Jwt",
                    Scheme = "Bearer",
                });
                //安全要求
                swaggerGenOption.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = LingYanRuntimeManager.BearerScheme
                            }
                        },
                        new string[]{ }
                    }
                });
            });
            ConsoleHelper.DefaultLog("【注册全局过滤器】...");
            builder.Services.AddControllers(mvcOption =>
            {
                //添加全局过滤器
                LingYanRuntimeManager.RuntimeCacheModel.ModuleFiler.ForEach(filer =>
                {
                    mvcOption.Filters.Add(filer);
                });
            });
            ConsoleHelper.DefaultLog("【注册JWT鉴权】...");
            builder.Services.AddAuthentication(options =>
            {
                //自定义鉴权方案可以添加
                options.DefaultScheme = LingYanRuntimeManager.BearerScheme;
            }).AddJwtBearer(LingYanRuntimeManager.BearerScheme, options =>
            {
                try
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        //是否验证Issuer
                        ValidateIssuer = true,
                        //发行人Issuer
                        ValidIssuer = LingYanRuntimeManager.JwtModel.Issuer,
                        //是否验证Audience
                        ValidateAudience = true,
                        //订阅人Audience
                        ValidAudience = LingYanRuntimeManager.JwtModel.Audience,
                        //是否验证SecurityKey
                        ValidateIssuerSigningKey = true,
                        //SecurityKey
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(LingYanRuntimeManager.JwtModel.SecretKey)),
                        //密码算法
                        //是否验证失效时间
                        ValidateLifetime = true,
                        //过期时间容错值，解决服务器端时间不同步问题（秒）
                        ClockSkew = TimeSpan.FromDays(Convert.ToDouble(LingYanRuntimeManager.JwtModel.Expres)),
                        RequireExpirationTime = true,
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            //终止
                            context.HandleResponse();
                            //context.Response.ContentType = "application/json";
                            //context.Response.StatusCode = StatusCodes.Status203NonAuthoritative;
                            //context.Response.WriteAsJsonAsync(new ResponceBody(203, "鉴权失败", null));
                            return Task.FromResult(0);
                        }
                    };
                }
                catch (Exception ex)
                {
                    options.Events = new JwtBearerEvents
                    {
                        OnChallenge = context =>
                        {
                            //终止
                            context.HandleResponse();
                            //context.Response.ContentType = "application/json";
                            //context.Response.StatusCode = StatusCodes.Status203NonAuthoritative;
                            //context.Response.WriteAsJsonAsync(new ResponceBody(202, "鉴权参数验证不通过", null));
                            return Task.FromResult(0);
                        }
                    };
                }
            });
            ConsoleHelper.DefaultLog("【HTTP 上下文的访问】...");
            builder.Services.AddHttpContextAccessor();
            ConsoleHelper.DefaultLog("【注册授权策略】...");
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy(LingYanRuntimeManager.EmpowerPolicy, policy =>
                {
                    policy.Requirements.Add(new PermissionRequirement(builder.Services.BuildServiceProvider().CreateScope()));
                });
            });

        }
        /// <summary>
        /// 容器管理员初始化框架服务
        /// </summary>
        /// <param name="app"></param>
        private static void InitProjectSelf(this WebApplication app)
        {
            ConsoleHelper.SuccessLog($"【配置Swagger中间件和可视化界面】...");
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();             
            }
            ConsoleHelper.SuccessLog($"【配置开发环境异常页面】...");
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }
            ConsoleHelper.SuccessLog($"【配置路由中间件】...");
            app.UseRouting();
            ConsoleHelper.SuccessLog($"【配置HTTPS重定向中间件】...");
            app.UseHttpsRedirection();
            ConsoleHelper.SuccessLog($"【配置跨域策略】...");
            app.UseCors(LingYanRuntimeManager.CrossPolicy);
            ConsoleHelper.SuccessLog($"【配置鉴权中间件】...");
            app.UseAuthentication();
            ConsoleHelper.SuccessLog($"【配置授权中间件】...");
            app.UseAuthorization();
            ConsoleHelper.SuccessLog($"【项目各模块初始化】...");
            LingYanRuntimeManager.RuntimeCacheModel.ModuleList.InitModules(app);
            ConsoleHelper.SuccessLog($"【配置静态权限文件夹】...");
            app.UseWhen(context => context.Request.Path.StartsWithSegments($"/{LingYanRuntimeManager.CommonConfigModel.LimitFile}"), appBuilder =>
            {
                appBuilder.Use(async (context, next) =>
                {
                    if (!context.User.Identity.IsAuthenticated)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsJsonAsync(new ResponceBody(40000, "用户鉴权不通过", null));
                        return;
                    }
                    await next();
                });
            });
            ConsoleHelper.SuccessLog($"【配置静态开放文件夹】...");
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(LingYanRuntimeManager.HostPhysicsRoot.GetLocalPath(LingYanRuntimeManager.CommonConfigModel.SoftFile)),
                RequestPath = $"/{LingYanRuntimeManager.CommonConfigModel.SoftFile}"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(LingYanRuntimeManager.HostPhysicsRoot.GetLocalPath(LingYanRuntimeManager.CommonConfigModel.LimitFile)),
                RequestPath = $"/{LingYanRuntimeManager.CommonConfigModel.LimitFile}"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(LingYanRuntimeManager.HostPhysicsRoot.GetLocalPath(LingYanRuntimeManager.CommonConfigModel.OpenFile)),
                RequestPath = $"/{LingYanRuntimeManager.CommonConfigModel.OpenFile}"
            });
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(LingYanRuntimeManager.HostPhysicsRoot.GetLocalPath(LingYanRuntimeManager.CommonConfigModel.ChatFile)),
                RequestPath = $"/{LingYanRuntimeManager.CommonConfigModel.ChatFile}"
            });
            ConsoleHelper.SuccessLog($"【启用多租户中间件】...");
            app.UseMiddleware<TenantSelectMiddleware>();
            ConsoleHelper.SuccessLog($"【初始化多租户配置】...");
            app.Services.InitTenant(LingYanRuntimeManager.RuntimeCacheModel);
            ConsoleHelper.SuccessLog($"【配置全局开启身份验证】...");
            app.MapControllers().RequireAuthorization();
            ConsoleHelper.SuccessLog($"【配置服务启动监听端口】...");
            LingYanRuntimeManager.ListeningPorts.ToList().ForEach(port =>
            {
                app.Urls.Add(port);
            });

        }
    }
}

using LingYan.Extension;
using LingYanAspCoreFramework.Models;
using Microsoft.Extensions.DependencyModel;
using System.Reflection;
using System.Runtime.Loader;

namespace LingYanAspCoreFramework.Extension
{
    public static class GainTypeExtension
    {
        /// <summary>
        /// 获取模块集合初始化（将类加载到运行时中）
        /// </summary>
        /// <param name="lYBuilderRuntimeModel"></param>
        public static void GainModuleServiceCollectionInit(this LYBuilderRuntimeModel lYBuilderRuntimeModel)
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
                    .Where(x => !x.IsAbstract && !x.IsInterface && (x.IsAssignableToGenericType(typeof(LYModule<,>)) || x.IsAssignableToGenericType(typeof(LYModule<,,>))))
                    .FirstOrDefault();
                //获得多租户基类实现
                assembly.GainTenantBaseEntitys(lYBuilderRuntimeModel);
                //获得DbContext
                assembly.GainDbContexts(lYBuilderRuntimeModel);
                //获得Manager
                assembly.GainManagers(lYBuilderRuntimeModel);
                //获取服务
                assembly.GainTService(lYBuilderRuntimeModel);
                //获取实例
                assembly.GainInstances(lYBuilderRuntimeModel);
                //获取过滤器实例
                assembly.GainFilers(lYBuilderRuntimeModel);
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
        private static void GainTenantBaseEntitys(this Assembly assembly, LYBuilderRuntimeModel lYBuilderRuntimeModel)
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
        private static void GainDbContexts(this Assembly assembly, LYBuilderRuntimeModel lYBuilderRuntimeModel)
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
        private static void GainTService(this Assembly assembly, LYBuilderRuntimeModel lYBuilderRuntimeModel)
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
        private static void GainManagers(this Assembly assembly, LYBuilderRuntimeModel lYBuilderRuntimeModel)
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
        private static void GainInstances(this Assembly assembly, LYBuilderRuntimeModel lYBuilderRuntimeModel)
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
        private static void GainFilers(this Assembly assembly, LYBuilderRuntimeModel lYBuilderRuntimeModel)
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
    }
}

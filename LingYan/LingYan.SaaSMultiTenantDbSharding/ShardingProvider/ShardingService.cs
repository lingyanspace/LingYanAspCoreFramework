using Microsoft.Extensions.DependencyModel;
using System.Reflection;
using System.Runtime.Loader;

namespace LingYan.SaaSMultiTenantDbSharding.ShardingProvider
{
    public class ShardingService : IShardingService
    {
        public List<Type> GetShardingTables()
        {
            List<Type> types = new List<Type>();
            //当前项目
            var currentAssemblyName = Assembly.GetExecutingAssembly().GetName().Name;
            //获取项目
            var compilationLibrary = DependencyContext.Default.CompileLibraries
                .Where(x => !x.Serviceable && x.Type != "package" && x.Type == "project" && x.Name != currentAssemblyName);
            //所有类型
            foreach (var singleLib in compilationLibrary)
            {
                //程序集
                var assembly = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(singleLib.Name));
                //获得分表实体
                var shardingTBs = assembly.GetTypes()
                    .Where(x => x.GetCustomAttributes(true)
                   .Any(y => y.GetType() == typeof(ShardingTBAttribute)) &&
                   !assembly.GetTypes().Any(z => z.BaseType == x))
                    .ToList();
                if (shardingTBs!=null&& shardingTBs.Count>0)
                {
                    types.AddRange(shardingTBs);
                }                
            }
            return types;
        }
    }
}

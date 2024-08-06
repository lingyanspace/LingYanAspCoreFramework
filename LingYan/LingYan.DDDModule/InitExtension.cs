using LingYan.Extension;

namespace LingYan.DDDModule
{
    public static class InitExtension
    {
        public static void InitializationModule(this List<object> ModuleList, object web)
        {
            foreach (var module in ModuleList)
            {
                //方法
                var method = module.GetType().GetMethod("BInitializationModule");
                //参数
                var parameters = web.GetInstanceParameters(method.GetParameters());
                Console.WriteLine($"{module}运行方法“BInitializationModule”，初始化该模块");
                // 调用方法，并传递参数值数组
                method.Invoke(module, parameters);
            }
        }
    }
}

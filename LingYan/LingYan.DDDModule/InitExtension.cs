using LingYan.Extension;

namespace LingYan.DDDModule
{
    public static class InitExtension
    {
        public static void InitializationModule(this List<object> ModuleList, object web)
        {
            foreach (var module in ModuleList)
            {
                //模块整体注册
                ConsoleColor.DarkMagenta.ConsoleLogger($"【模块初始化方法执行】{module.GetType().Name}运行方法BInitializationModule...");
                //方法
                var method = module.GetType().GetMethod("BInitializationModule");
                //参数
                var parameters = web.GetInstanceParameters(method.GetParameters());              
                method.Invoke(module, parameters);
            }
        }
    }
}

using LingYanAspCoreFramework.DynamicApis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;

namespace LingYanAspCoreFramework.DynamicApis
{
    internal static class MvcOptionsExtensions
    {
        /// <summary>
        /// 依据特性配置
        /// </summary>
        /// <param name="opts"></param>
        /// <param name="routeAttribute"></param>
        public static void UseCentralRoutePrefix(this MvcOptions opts, IRouteTemplateProvider routeAttribute)
        {
            //添加我们自定义实现
            opts.Conventions.Insert(0, new RouteConvention(routeAttribute));
        }
        /// <summary>
        /// 依据JSON配置
        /// </summary>
        /// <param name="opts"></param>
        /// <param name="configuration"></param>
        public static void UseCentralRouteJsonConfig(this MvcOptions opts, IConfiguration configuration)
        {
            opts.Conventions.Insert(0, new DynamicControllerConvention(configuration)); 
        }
    }
}

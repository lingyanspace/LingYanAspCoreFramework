using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace LingYanAspCoreFramework.Helpers
{
    public class FilterHelper
    {
        /// <summary>
        /// 授权过滤器解析
        /// </summary>
        /// <param name="authorizationFilterContext"></param>
        /// <returns></returns>
        public static ILogger ResolveFilterLogger(AuthorizationFilterContext authorizationFilterContext)
        {
            var controllerType = (authorizationFilterContext.ActionDescriptor as ControllerActionDescriptor).ControllerTypeInfo;
            Type loggerType = typeof(ILogger<>).MakeGenericType(controllerType);
            var actionLogger = (ILogger)authorizationFilterContext.HttpContext.RequestServices.GetService(loggerType);
            return actionLogger;
        }
        /// <summary>
        /// 方法过滤器解析
        /// </summary>
        /// <param name="actionExecutedContext"></param>
        /// <returns></returns>
        public static ILogger ResolveFilterLogger(ActionExecutedContext actionExecutedContext)
        {
            Type loggerType = typeof(ILogger<>).MakeGenericType(actionExecutedContext.Controller.GetType());
            var actionLogger = (ILogger)actionExecutedContext.HttpContext.RequestServices.GetService(loggerType);
            return actionLogger;
        }
    }
}

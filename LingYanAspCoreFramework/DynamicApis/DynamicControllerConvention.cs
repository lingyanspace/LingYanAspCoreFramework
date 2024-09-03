using LingYan.Model;
using LingYan.Model.DynamicHttpMethodModel;
using LingYanAspCoreFramework.Attributes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Configuration;
namespace LingYanAspCoreFramework.DynamicApis
{
    internal class DynamicControllerConvention : IApplicationModelConvention
    {
        private IConfiguration _configuration;
        private List<HttpMethodConfigure> httpMethods = new();
        public DynamicControllerConvention(IConfiguration configuration)
        {
            _configuration = configuration;
            httpMethods = LYExpose.HttpMethodConfigure;
        }
        public void Apply(ApplicationModel application)
        {
            //循环每一个控制器信息
            foreach (var controller in application.Controllers)
            {
                var controllerType = controller.ControllerType.AsType();
                //是否继承ICoreDynamicController接口
                if ((typeof(LYDynamicRouteAttribute).IsAssignableFrom(controllerType)||controllerType.IsDefined(typeof(LYDynamicRouteAttribute), true)))
                {
                    foreach (var item in controller.Actions)
                    {
                        ConfigureSelector(controller.ControllerName, item);
                    }
                }
            }
        }

        private void ConfigureSelector(string controllerName, ActionModel action)
        {
            if (action.Selectors.Any(a => a.AttributeRouteModel is not null || a.ActionConstraints.Count > 0))
            {
                //此时要么路由路径模板有、接口风格有、要么两者都有
                foreach (var item in action.Selectors)
                {
                    //配置路由
                    if (item.AttributeRouteModel is null)
                    {
                        item.AttributeRouteModel = CreateActionRoutePath(controllerName, action.ActionName);
                    }
                    else
                    {
                        if (!item.AttributeRouteModel.Template.Contains("/"))
                        {
                            string actionName = "";
                            int index = item.AttributeRouteModel.Template.IndexOf('('); // 查找逗号的位置
                            if (index > 0)
                            {
                                actionName = item.AttributeRouteModel.Template.Substring(0, index); // 截取逗号之前的内容  
                            }
                            else
                            {
                                actionName = item.AttributeRouteModel.Template;
                            }
                            item.AttributeRouteModel = CreateActionRoutePath(controllerName, actionName);
                        }
                    }
                    //配置接口
                    if (item.ActionConstraints.Count == 0)
                    {
                        var httpMethod = CreateActionHttpMethod(action);
                        item.ActionConstraints.Add(new HttpMethodActionConstraint(new[] { httpMethod }));
                    }

                }
            }
            else
            {
                var httpMethod = CreateActionHttpMethod(action);
                var httpActionPath = CreateActionRoutePath(controllerName, action.ActionName);
                action.Selectors[0].AttributeRouteModel = httpActionPath;
                action.Selectors[0].ActionConstraints.Add(new HttpMethodActionConstraint(new[] { httpMethod }));
            }
        }
        /// <summary>
        /// 获取接口风格
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        private string CreateActionHttpMethod(ActionModel action)
        {
            string httpMethod = string.Empty;
            //是否有HttpMethodAttribute
            var routeAttributes = action.ActionMethod.GetCustomAttributes(typeof(HttpMethodAttribute), false);
            //如果标记了HttpMethodAttribute
            if (routeAttributes != null && routeAttributes.Any())
            {
                httpMethod = routeAttributes.SelectMany(m => (m as HttpMethodAttribute).HttpMethods).ToList().Distinct().FirstOrDefault();
            }
            else
            {
                var methodName = action.ActionMethod.Name.ToLower();

                foreach (var item in httpMethods)
                {
                    if (item.MethodVal.Any(a => methodName.Contains(a.ToLower())))
                    {
                        httpMethod = item.MethodKey;
                        break;
                    }
                }
                if (string.IsNullOrEmpty(httpMethod))
                {
                    httpMethod = "Post";
                }
            }
            return httpMethod;
        }
        /// <summary>
        /// 获取路由路径
        /// </summary>
        /// <param name="controllerName"></param>
        /// <param name="actionName"></param>
        /// <returns></returns>
        private AttributeRouteModel CreateActionRoutePath(string controllerName, string actionName)
        {
            string routePrefix = "dwq";
            if (!string.IsNullOrEmpty(_configuration.GetSection("DynamicHttpRoutePrefix").Get<string>()))
            {
                routePrefix = _configuration.GetSection("DynamicHttpRoutePrefix").Get<string>();
            }
            //创建路由路径
            var routePath = string.Concat(routePrefix + "/", controllerName + "/", actionName).Replace("//", "/");
            //给此Action添加路由
            return new AttributeRouteModel(new RouteAttribute(routePath));
        }
    }

}

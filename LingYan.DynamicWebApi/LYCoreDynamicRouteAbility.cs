using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace LingYan.DynamicWebApi
{
    public class LYCoreDynamicRouteAbility
    {
        //private readonly IActionDescriptorCollectionProvider collectionProvider;

        //public LYCoreDynamicRouteAbility(IActionDescriptorCollectionProvider collectionProvider)
        //{
        //    this.collectionProvider = collectionProvider;
        //}
        protected OkObjectResult Ok([ActionResultObjectValue] object? value)
        {
            OkObjectResult body = new OkObjectResult(value);
            return body;
        }
        //[AllowAnonymous]
        //public async Task<IActionResult> MicroServicesRoutes() 
        //{
        //    var routes = collectionProvider.ActionDescriptors.Items.Select(x =>
        //    {
        //        //x.GetPropertyValue<controlle>
        //        //var type = typeInfo.AsType();
        //        //if ((typeof(LYCoreDynamicRouteAbility).IsAssignableFrom(type) || //判断是否继承ICoreDynamicController接口
        //        //    type.IsDefined(typeof(CoreDynamicControllerAttribute), true) || // 判断是否标记了ICoreDynamicController特性
        //        //    type.BaseType == typeof(Microsoft.AspNetCore.Mvc.Controller)) && //判断基类型是否是Controller
        //        //    type != typeof(LYCoreDynamicRouteAbility) &&
        //        //(typeInfo.IsPublic && !typeInfo.IsAbstract && !typeInfo.IsGenericType && !typeInfo.IsInterface)) //必须是Public、不能是抽象类、必须是非泛型的
        //        //{
        //        //    return true;
        //        //}

        //        var lyRouteModel = new LYRouteModel();
        //        lyRouteModel.ActionName = x.RouteValues["Action"];
        //        lyRouteModel.ControllerName = x.RouteValues["Controller"];
        //        lyRouteModel.HttpMethod = x.ActionConstraints?.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods.First();
        //        lyRouteModel.TemplatePath = "/" + x.AttributeRouteInfo.Template;
        //        lyRouteModel.PrefixName = lyRouteModel.TemplatePath.Replace(lyRouteModel.ControllerName, "").Replace(lyRouteModel.ActionName, "").Replace("/", "");
        //        return lyRouteModel;
        //    }).ToList();
        //    return Ok(routes);
        //}
    }
}

using LingYanAspCoreFramework.Attributes;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace LingYanAspCoreFramework.DynamicApis
{
    internal class DynamicControlleFeatureProvider : ControllerFeatureProvider
    {
        protected override bool IsController(TypeInfo typeInfo)
        { 
            var type = typeInfo.AsType();
            if ((typeof(LYDynamicRouteAttribute).IsAssignableFrom(type) ||
                type.IsDefined(typeof(LYDynamicRouteAttribute), true) ||
                type.BaseType == typeof(Microsoft.AspNetCore.Mvc.Controller)) &&
                type != typeof(LYDynamicRouteAttribute) &&
                (typeInfo.IsPublic && !typeInfo.IsAbstract && !typeInfo.IsGenericType && !typeInfo.IsInterface))
            {

                return true;
            }
            return false;
        }
    }
}

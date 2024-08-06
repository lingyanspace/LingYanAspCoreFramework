using Microsoft.AspNetCore.Mvc.Controllers;
using System.Reflection;

namespace LingYan.DynamicWebApi
{
    public class CoreDynamicExtendControlleFeatureProvider : ControllerFeatureProvider
    {
        protected override bool IsController(TypeInfo typeInfo)
        {
            var type = typeInfo.AsType();
            if ((typeof(LYCoreDynamicRouteAbility).IsAssignableFrom(type) ||
                type.IsDefined(typeof(LYCoreDynamicRouteAbilityAttribute), true) ||
                type.BaseType == typeof(Microsoft.AspNetCore.Mvc.Controller)) &&
                type != typeof(LYCoreDynamicRouteAbility) &&
                (typeInfo.IsPublic && !typeInfo.IsAbstract && !typeInfo.IsGenericType && !typeInfo.IsInterface))
            {

                return true;
            }
            return false;
        }
    }
}

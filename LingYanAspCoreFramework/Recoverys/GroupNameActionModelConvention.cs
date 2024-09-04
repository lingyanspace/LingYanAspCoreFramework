using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace LingYanAspCoreFramework.Recoverys
{
    public class GroupNameActionModelConvention : IActionModelConvention
    {
        public void Apply(ActionModel action)
        {
            if (action.Controller.ControllerName == "WeatherForecast")
            {
                if (action.ActionName == "Get")
                {
                    action.ApiExplorer.GroupName = "v1";
                    action.ApiExplorer.IsVisible = true;
                }
                else if (action.ActionName == "Post")
                {
                    action.ApiExplorer.GroupName = "v2";
                    action.ApiExplorer.IsVisible = true;
                }
            }
        }
    }
}

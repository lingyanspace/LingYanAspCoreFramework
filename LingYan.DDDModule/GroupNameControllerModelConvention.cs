using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace LingYan.DDDModule
{
    public class GroupNameControllerModelConvention : IControllerModelConvention
    {
        public void Apply(ControllerModel controller)
        {
            if (controller.ControllerName == "Home")
            {
                foreach (var action in controller.Actions)
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
}

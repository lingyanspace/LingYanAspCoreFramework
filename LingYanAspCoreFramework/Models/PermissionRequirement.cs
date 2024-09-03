using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace LingYanAspCoreFramework.Models
{
    public class PermissionRequirement : IAuthorizationRequirement
    {
        public IServiceScope ServiceScope { get; set; }
        public PermissionRequirement(IServiceScope serviceScope)
        {
            this.ServiceScope = serviceScope;
        }

    }
}

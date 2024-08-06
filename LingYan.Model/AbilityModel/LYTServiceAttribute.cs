using Microsoft.Extensions.DependencyInjection;

namespace LingYan.Model.AbilityModel
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class LYTServiceAttribute : Attribute
    {
        public Type TService { get; set; }
        public ServiceLifetime ServiceLifetime { get; set; } 
        public LYTServiceAttribute(Type type, ServiceLifetime serviceLifetime = ServiceLifetime.Transient)
        {
            this.TService = type;
            this.ServiceLifetime = serviceLifetime;
        }
    }
}

using LingYanAspCoreFramework.Attributes;
using Microsoft.Extensions.Configuration;

namespace LingYanAspCoreFramework.Models
{
    public class LYBuilderRuntimeModel
    {
        //模块项目
        public List<object> ModuleList { get; set; }
        //临时缓存
        public Dictionary<Type, DbContextType> ModuleDbContextList { get; set; }
        public Dictionary<string, Type> ModuleTenantBaseEntitys { get; set; }
        public List<Type> ModuleManagerList { get; set; }
        public List<Type> ModuleTService { get; set; }
        public List<Type> ModuleTInstance { get; set; }
        public Dictionary<ShardingKeyType, List<Type>> VirtualTableList { get; set; }
        public List<Type> TenantTemplateDbContexts { get; set; }
        public List<Type> ModuleFiler { get; set; }
        public ConfigurationManager LingYanConfiguration { get; set; }
        public LYBuilderRuntimeModel()
        {
            this.ModuleList = new List<object>();
            this.ModuleDbContextList = new Dictionary<Type, DbContextType>();
            this.ModuleTenantBaseEntitys = new Dictionary<string, Type>();
            this.ModuleManagerList = new List<Type>();
            this.ModuleTService = new List<Type>();
            this.ModuleTInstance = new List<Type>();
            this.VirtualTableList = new Dictionary<ShardingKeyType, List<Type>>();
            this.TenantTemplateDbContexts = new List<Type>();
            this.ModuleFiler = new List<Type>();
            this.LingYanConfiguration = new ConfigurationManager();
        }
    }
}

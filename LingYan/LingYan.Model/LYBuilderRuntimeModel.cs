using LingYan.Model.ContextModel;
using Microsoft.Extensions.Configuration;

namespace LingYan.Model
{
    public class LYBuilderRuntimeModel
    {
        //
        public List<object> ModuleList = new List<object>();
        //临时缓存
        public Dictionary<Type, DbContextType> ModuleDbContextList = new Dictionary<Type, DbContextType>();
        public Dictionary<string, Type> ModuleTenantBaseEntitys = new Dictionary<string, Type>();
        public List<Type> ModuleManagerList = new List<Type>();
        public List<Type> ModuleTService = new List<Type>();
        public List<Type> ModuleTInstance = new List<Type>();
        public Dictionary<ShardingKeyType, List<Type>> VirtualTableList = new Dictionary<ShardingKeyType, List<Type>>();
        public List<Type> TenantTemplateDbContexts = new List<Type>();
        public List<Type> ModuleFiler = new List<Type>(); 
        public ConfigurationManager ConfigurationManager { get; set; }
    }
}

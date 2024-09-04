namespace LingYanAspCoreFramework.Models
{
    public class MysqlConfigModel
    {
        public string SysTenant { get; set; }
        public string SysTenantTemplate { get; set; }
        public Dictionary<string,string> Others { get; set; }     
    }
}

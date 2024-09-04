using LingYan.Model.RedisModelLingYanAspCoreFramework.Models;

namespace LingYanAspCoreFramework.Models
{
    public class RedisCofigModel
    { 
        public string Pattern { get; set; }
        public string Single { get; set; }
        public SentinelModel SentinelModel { get; set; }
        public string Cluster { get; set; }      
    }
}

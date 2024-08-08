using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingYan.Model.RedisModel
{
    public class RedisCofigModel
    { 
        public string Pattern { get; set; }
        public string Single { get; set; }
        public SentinelModel SentinelModel { get; set; }
        public string Cluster { get; set; }
        public RedisCofigModel()
        {
            this.Pattern = "Single";
            this.Single = "192.168.148.130:6379,defaultDatabase=1,password=,prefix=ly";
        }
    }
}

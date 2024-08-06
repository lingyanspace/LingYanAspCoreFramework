using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingYan.Model.RedisModel
{
    public class SentinelModel
    {
        public string Master { get; set; }
        public string Slave { get; set; }
    }
}

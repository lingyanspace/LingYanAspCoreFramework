using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingYan.Model.MysqlModel
{
    public class MysqlConfigModel
    {
        public string SysTenant { get; set; }
        public string SysTenantTemplate { get; set; }
        public Dictionary<string,string> Others { get; set; } 
    }
}

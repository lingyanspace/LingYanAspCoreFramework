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
        public MysqlConfigModel()
        {
            this.Others = new Dictionary<string, string>();
            this.Others.Add("Default", "server=192.168.148.130;port=3306;database=test;user=root;password=123456;AllowLoadLocalInfile=true;");
        }
    }
}

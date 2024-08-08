using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingYan.Model.CommonModel
{
    public class CommonConfigModel
    {
        public string FFMpegWinConfig { get; set; }
        public string FFMpegLinuxConfig { get; set; }
        public string NlogConfig { get; set; }
        //网络路由IP
        public string IPLimitVideo { get; set; }
        public string IPLimitFile { get; set; }
        public string IPOpenFile { get; set; }
        //物理文件夹名 
        public string LimitVideo { get; set; }
        public string LimitFile { get; set; }
        public string OpenFile { get; set; }
        //聊天文件
        public string ChatFile { get; set; }
        //软件包
        public string SoftFile { get; set; }
    }

}

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
        //软件包
        public string SoftConfig { get; set; } = "SoftFile";
        //路由IP
        public string IPLimitVideo { get; set; }
        public string IPLimitFile { get; set; }
        public string IPOpenFile { get; set; }
        //路径标识 
        public string LimitVideo = "LimitVideo";
        public string LimitFile = "LimitFile";
        public string OpenFile = "OpenFile";
        //聊天文件
        public string ChatFile { get; set; } = "ChatFile";
    }
    
}

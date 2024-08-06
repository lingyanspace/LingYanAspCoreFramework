using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingYan.Model.CommonModel
{
    // 定义用于存储文件格式标签信息的类
    public class FFMpegFormatTags
    {
        public string MajorBrand { get; set; }
        public string MinorVersion { get; set; }
        public string CompatibleBrands { get; set; }
        public string CreationTime { get; set; }
        public string Hw { get; set; }
        public string Bitrate { get; set; }
        public string Maxrate { get; set; }
        public string TeIsReencode { get; set; }
        public string Encoder { get; set; }
    }
}

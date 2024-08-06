using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingYan.Model.CommonModel
{
    // 定义用于存储完整文件信息的根类
    public class FFMpegRoot
    {
        public List<FFMpegStream> Streams { get; set; }
        public FFMpegFormat Format { get; set; }
    }
}

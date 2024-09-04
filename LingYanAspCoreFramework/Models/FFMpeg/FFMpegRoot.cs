namespace LingYanAspCoreFramework.Models.FFMpeg
{
    // 定义用于存储完整文件信息的根类
    public class FFMpegRoot
    {
        public List<FFMpegStream> Streams { get; set; }
        public FFMpegFormat Format { get; set; }
    }
}

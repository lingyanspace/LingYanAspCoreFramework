namespace LingYanAspCoreFramework.Models.FFMpeg
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

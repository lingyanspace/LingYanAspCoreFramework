namespace LingYanAspCoreFramework.Models
{
    public class CloudVodLiveConfigModel
    {
        public string VodServer { get; set; }
        public string LiveServer { get; set; }
        public CloudVodLiveConfigModel()
        {
            this.VodServer = "rtmp://127.0.0.1:7878/vod/";
            this.LiveServer = "rtmp://127.0.0.1:7878/live/";
        }
    }
}

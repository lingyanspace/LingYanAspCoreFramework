namespace LingYanAspCoreFramework.Models.FFMpeg
{
    //定义用于存储视频和音频流信息的类
    public class FFMpegStream
    {
        public int Index { get; set; }
        public string CodecName { get; set; }
        public string CodecLongName { get; set; }
        public string Profile { get; set; }
        public string CodecType { get; set; }
        public string CodecTagString { get; set; }
        public string CodecTag { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public int CodedWidth { get; set; }
        public int CodedHeight { get; set; }
        public int ClosedCaptions { get; set; }
        public int FilmGrain { get; set; }
        public int HasBFrames { get; set; }
        public string SampleAspectRatio { get; set; }
        public string DisplayAspectRatio { get; set; }
        public string PixFmt { get; set; }
        public int Level { get; set; }
        public string ColorRange { get; set; }
        public string ColorSpace { get; set; }
        public string ColorTransfer { get; set; }
        public string ColorPrimaries { get; set; }
        public string ChromaLocation { get; set; }
        public string FieldOrder { get; set; }
        public int Refs { get; set; }
        public bool IsAvc { get; set; }
        public string NalLengthSize { get; set; }
        public string Id { get; set; }
        public string RFrameRate { get; set; }
        public string AvgFrameRate { get; set; }
        public string TimeBase { get; set; }
        public long StartPts { get; set; }
        public string StartTime { get; set; }
        public long DurationTs { get; set; }
        public string Duration { get; set; }
        public string BitRate { get; set; }
        public string BitsPerRawSample { get; set; }
        public string NbFrames { get; set; }
        public int ExtradataSize { get; set; }
        public FFMpegDisposition Disposition { get; set; }
        public FFMpegTags Tags { get; set; }
    }
}

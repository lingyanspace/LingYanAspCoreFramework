namespace LingYanAspCoreFramework.Models
{
    public class IdGeneratorOptionConfigModel
    {
        public ushort WorkerId { get; set; }
        public byte WorkerIdBitLength { get; set; }
        public byte SeqBitLength { get; set; }
        public DateTime BaseTime { get; set; }     
    }
}

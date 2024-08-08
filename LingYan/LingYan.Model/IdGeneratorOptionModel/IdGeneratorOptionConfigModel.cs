namespace LingYan.Model.IdGeneratorOptionModel
{
    public class IdGeneratorOptionConfigModel
    {
        public ushort WorkerId { get; set; }
        public byte WorkerIdBitLength { get; set; }
        public byte SeqBitLength { get; set; }
        public DateTime BaseTime { get; set; }
        public IdGeneratorOptionConfigModel()
        {
            this.WorkerId = 1;
            this.WorkerIdBitLength = 10;
            this.SeqBitLength = 10;
            this.BaseTime = new DateTime(2024,1,1);
        }
    }
}

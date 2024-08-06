using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingYan.Model.IdGeneratorOptionModel
{
    public class IdGeneratorOptionConfigModel
    {
        public ushort WorkerId { get; set; }
        public byte WorkerIdBitLength { get; set; }
        public byte SeqBitLength { get; set; }
        public DateTime BaseTime { get; set; }
    }
}

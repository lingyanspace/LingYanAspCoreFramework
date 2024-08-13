using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingYan.DynamicShardingDBT.DBTModel
{
    public enum DynamicShardingType
    {
        /// <summary>
        /// 通过哈希取模分表
        /// </summary>
        HashMod,

        /// <summary>
        /// 按日期分表
        /// </summary>
        Date
    }
}

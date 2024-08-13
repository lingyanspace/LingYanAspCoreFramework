namespace LingYan.DynamicShardingDBT.DBTModel
{
    public enum DynamicUpdateType
    {
        /// <summary>
        /// 等,即赋值,[Field]= value
        /// </summary>
        Equal = 0,

        /// <summary>
        /// 自增,[Field]=[Field] + value
        /// </summary>
        /// 
        Add = 1,

        /// <summary>
        /// 自减,[Field]=[Field] - value
        /// </summary>
        /// 
        Minus = 2,

        /// <summary>
        /// 自乘,[Field]=[Field] * value
        /// </summary>
        /// 
        Multiply = 3,

        /// <summary>
        /// 自除,[Field]=[Field] / value
        /// </summary>
        Divide = 4,

        /// <summary>
        /// 字符串拼接[Field]=[Field] + value，不同数据库实现有差异
        /// </summary>
        Concat = 5
    }
}

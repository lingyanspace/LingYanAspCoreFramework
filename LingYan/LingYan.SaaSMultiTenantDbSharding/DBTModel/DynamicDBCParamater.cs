using LingYan.DynamicShardingDBT.DBTModel;

namespace LingYan.DynamicShardingDBT.DBTModel
{
    public class DynamicDBCParamater
    {  
        /// <summary>
        /// 连接字符串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public DynamicDBType DynamicDatabase { get; set; } 

        /// <summary>
        /// 实体命名空间
        /// </summary>
        public string EntityNamespace { get; set; }

        /// <summary>
        /// 自定义实体类
        /// </summary>
        public Type[] EntityTypes { get; set; }

        /// <summary>
        /// 表名后缀(分表时需要)
        /// </summary>
        public string Suffix { get; set; }
    }
}

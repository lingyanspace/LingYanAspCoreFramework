namespace LingYan.DynamicShardingDBT.DBTModel
{
    internal class DynamicDataSource
    {
        public string Name { get; set; }
        public DynamicDBType DbType { get; set; }
        public (string connectionString, DynamicReadWriteType dynamicReadWriteType)[] Dbs { get; set; }
    }
}

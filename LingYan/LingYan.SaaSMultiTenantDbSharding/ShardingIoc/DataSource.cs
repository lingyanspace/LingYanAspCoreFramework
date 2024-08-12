﻿using LingYan.SaaSMultiTenantDbSharding.DynamicDbContext;

namespace LingYan.SaaSMultiTenantDbSharding.ShardingIoc
{
    internal class DataSource
    {
        public string Name { get; set; }
        public DatabaseType DbType { get; set; }
        public (string connectionString, ReadWriteType readWriteType)[] Dbs { get; set; }
    }
}

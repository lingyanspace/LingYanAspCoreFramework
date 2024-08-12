namespace LingYan.SaaSMultiTenantDbSharding.ShardingProvider
{
    public class PhysicTable
    {
        public Type EntityType { get; set; }
        public string DataSourceName { get; set; }
        public string Suffix { get; set; }
    }
}

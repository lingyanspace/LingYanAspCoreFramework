using LingYan.SaaSMultiTenantDbSharding.ShardingProvider;

namespace LingYan.TestApi
{
    [ShardingTB]
    public class TestEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public int ModKey { get; set; }
        public DateTime TimeKey { get; set; }
    }
}

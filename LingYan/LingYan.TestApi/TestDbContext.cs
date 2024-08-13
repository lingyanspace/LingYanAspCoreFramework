using LingYan.DynamicShardingDBT.DBTContext;
using Microsoft.EntityFrameworkCore;

namespace LingYan.TestApi
{
    public class TestDbContext : DynamicDbContext
    {
        public DbSet<TestEntity> TestEntity { get; set; }
        public TestDbContext(DynamicDbContext dynamicDbContext) : base(dynamicDbContext)
        {
        }
    }
}

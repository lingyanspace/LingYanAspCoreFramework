using LingYan.DynamicShardingDBT.DBTContext;
using LingYan.DynamicShardingDBT.DBTFactory;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Design.Internal;

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

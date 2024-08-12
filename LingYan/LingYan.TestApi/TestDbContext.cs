using LingYan.SaaSMultiTenantDbSharding.DynamicDbContext;
using Microsoft.EntityFrameworkCore;

namespace LingYan.TestApi
{
    public class TestDbContext : DynamicDbContext
    {
        public TestDbContext(DbContextOptions contextOptions, DynamicDbContextParamater dynamicDbContextParamater, IServiceProvider serviceProvider) : base(contextOptions, dynamicDbContextParamater, serviceProvider)
        {
            Console.WriteLine("TestDbContext进入");
        }
        public DbSet<TestEntity> Tests { get; set; }
    
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            Console.WriteLine("OnConfiguring进入");
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            Console.WriteLine("OnModelCreating进入");
        }
    }
}

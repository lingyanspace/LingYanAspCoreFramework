namespace LingYanAspCoreFramework.Attributes
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class LYDbContextAttribute : Attribute
    {
        public DbContextType DbContextType { get; set; } 
        public string ConnectionString { get; set; }
        public Type[] ShardingTable { get; set; }
        public LYDbContextAttribute(string connectionString, DbContextType dbContextType = DbContextType.DbContext, params Type[] shardingTable) 
        {
            this.ConnectionString = connectionString;
            DbContextType = dbContextType;
            ShardingTable = shardingTable;
        }       
    }
    public enum DataBaseType
    {
        MYSQL = 1,
        MSSQL = 2
    }
    public enum DbContextType
    {
        DbContext = 1,
        TenantConfigDbContext = 2,
        TenantTemplateDbContext = 3,
        ShardingDbContext = 4
    }
    public enum ShardingKeyType
    {
        Mod = 1,
        Time = 2
    }
}

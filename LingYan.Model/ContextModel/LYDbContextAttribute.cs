using LingYan.Model.ContextModel;

namespace LingYan.Model.BaseAttributes
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
}

namespace LingYanAspCoreFramework.MultiTenants
{
    public class SqlServerMigrationNamespace : IMigrationNamespace
    {
        public string GetNamespace()
        {
            return "ShardingCoreMultiTenantSys.Migrations.SqlServer";
        }
    }
}

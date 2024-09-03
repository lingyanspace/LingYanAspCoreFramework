namespace LingYanAspCoreFramework.MultiTenants
{
    public class MySqlMigrationNamespace : IMigrationNamespace
    {
        public string GetNamespace()
        {
            return "ShardingCoreMultiTenantSys.Migrations.MySql";
        }
    }
}

namespace LingYan.MultiTenant.SysMigrationsAssemblies
{
    public class MySqlMigrationNamespace : IMigrationNamespace
    {
        public string GetNamespace()
        {
            return "ShardingCoreMultiTenantSys.Migrations.MySql";
        }
    }
}

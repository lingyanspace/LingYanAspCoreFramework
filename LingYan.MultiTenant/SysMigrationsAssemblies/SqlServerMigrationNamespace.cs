using LingYan.MultiTenant.SysMigrationsAssemblies;

public class SqlServerMigrationNamespace : IMigrationNamespace
{
    public string GetNamespace()
    {
        return "ShardingCoreMultiTenantSys.Migrations.SqlServer";
    }
}

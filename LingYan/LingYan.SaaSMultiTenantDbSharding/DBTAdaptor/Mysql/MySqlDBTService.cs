using LingYan.DynamicShardingDBT.DBTContext;
using LingYan.DynamicShardingDBT.DBTProvider;

namespace LingYan.DynamicShardingDBT.DBTAdaptor.Mysql
{
    internal class MySqlDBTService : DynamicDBTService, IDynamicDBTService
    {
        public MySqlDBTService(DynamicDbContext baseDbContext)
            : base(baseDbContext)
        {
        }

        protected override string FormatFieldName(string name)
        {
            return $"`{name}`";
        }

        protected override string GetSchema(string schema)
        {
            return null;
        }
    }
}

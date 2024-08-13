using LingYan.DynamicShardingDBT.DBTModel;
using LingYan.DynamicShardingDBT.DBTProvider;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace LingYan.DynamicShardingDBT.DBTContext
{
    public class DynamicModelCustomizer : ModelCustomizer 
    {
        public DynamicModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
        {
        }
        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);
            var dbContextBase = context as DynamicDbContext;
            var shardingTypeFinder = dbContextBase.ServiceProvider.GetService<IDynamicDBTService>();
            ////查找需要重新映射表名的类
            //var shardingTypes = shardingTypeFinder.GetIQueryable();

            //if (shardingTypes != null && shardingTypes.Count() > 0)
            //{

            //    if (context is DynamicDbContext contextBase)
            //    {
            //        if (!string.IsNullOrEmpty(contextBase.DynamicDBCParamater.Suffix))
            //        {
            //            foreach (var type in shardingTypes)
            //            {
            //                switch (contextBase.DynamicDBCParamater.DynamicDatabase)
            //                {
            //                    case DynamicDBType.SqlServer:
            //                        modelBuilder.Entity(type).ToTable($"{type.Name}_{contextBase.DynamicDBCParamater.Suffix}");
            //                        break;
            //                    case DynamicDBType.SQLite:
            //                        modelBuilder.Entity(type).ToTable($"{type.Name}_{contextBase.DynamicDBCParamater.Suffix}");
            //                        break;
            //                    case DynamicDBType.MySql:
            //                        modelBuilder.Entity(type).ToTable($"{type.Name}_{contextBase.DynamicDBCParamater.Suffix}");
            //                        break;
            //                    case DynamicDBType.Oracle:
            //                        modelBuilder.Entity(type).ToTable($"{type.Name}_{contextBase.DynamicDBCParamater.Suffix}");
            //                        break;
            //                    default:
            //                        modelBuilder.Entity(type).ToTable($"{type.Name}_{contextBase.DynamicDBCParamater.Suffix}");
            //                        break;
            //                }
            //            }
            //        }
            //    }

            //}
        }
    }
}

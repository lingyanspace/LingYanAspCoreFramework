using LingYan.DynamicShardingDBT.DBTCache;
using LingYan.DynamicShardingDBT.DBTModel;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Update.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics.CodeAnalysis;

namespace LingYan.DynamicShardingDBT.DBTContext
{
    [SuppressMessage("Usage", "EF1001:Internal EF Core API usage.", Justification = "<挂起>")]
    public class DynamicMigrationsModelDiffer : MigrationsModelDiffer
    {
#if NET8_0
        public DynamicMigrationsModelDiffer(IRelationalTypeMappingSource typeMappingSource, IMigrationsAnnotationProvider migrationsAnnotationProvider, IRowIdentityMapFactory rowIdentityMapFactory, CommandBatchPreparerDependencies commandBatchPreparerDependencies) : base(typeMappingSource, migrationsAnnotationProvider, rowIdentityMapFactory, commandBatchPreparerDependencies)
        {
        } 
#endif
#if NET7_0
        public ShardingMigration(IRelationalTypeMappingSource typeMappingSource, IMigrationsAnnotationProvider migrationsAnnotationProvider, IRowIdentityMapFactory rowIdentityMapFactory, CommandBatchPreparerDependencies commandBatchPreparerDependencies) : base(typeMappingSource, migrationsAnnotationProvider, rowIdentityMapFactory, commandBatchPreparerDependencies)
        {
        }
#endif
#if NET6_0
        public ShardingMigration(IRelationalTypeMappingSource typeMappingSource, IMigrationsAnnotationProvider migrationsAnnotations, IChangeDetector changeDetector, IUpdateAdapterFactory updateAdapterFactory, CommandBatchPreparerDependencies commandBatchPreparerDependencies) : base(typeMappingSource, migrationsAnnotations, changeDetector, updateAdapterFactory, commandBatchPreparerDependencies)
        {
        }
#endif
#if NETSTANDARD2_1
        public ShardingMigration(
            IRelationalTypeMappingSource typeMappingSource,
            IMigrationsAnnotationProvider migrationsAnnotations,
            IChangeDetector changeDetector,
            IUpdateAdapterFactory updateAdapterFactory,
            CommandBatchPreparerDependencies commandBatchPreparerDependencies
            )
        : base(typeMappingSource, migrationsAnnotations, changeDetector, updateAdapterFactory, commandBatchPreparerDependencies)
        {

        }
#endif
        public override IReadOnlyList<MigrationOperation> GetDifferences(IRelationalModel source, IRelationalModel target)
        {
            var shardingOption = DynamicDBTCache.ServiceProvider.GetService<IOptions<DynamicDBTOption>>().Value;
            var sourceOperations = base.GetDifferences(source, target).ToList();

            //忽略外键
            if (shardingOption.MigrationsWithoutForeignKey)
            {
                sourceOperations.RemoveAll(x => x is AddForeignKeyOperation || x is DropForeignKeyOperation);
                foreach (var operation in sourceOperations.OfType<CreateTableOperation>())
                {
                    operation.ForeignKeys?.Clear();
                }
            }

            return sourceOperations;
        }
    }
}

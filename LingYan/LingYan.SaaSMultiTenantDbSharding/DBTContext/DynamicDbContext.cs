using LingYan.DynamicShardingDBT.DBTCache;
using LingYan.DynamicShardingDBT.DBTHelper;
using LingYan.DynamicShardingDBT.DBTModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace LingYan.DynamicShardingDBT.DBTContext
{
    //动态数据库上下文 
    public class DynamicDbContext : DbContext
    {
        /// <summary>
        /// 当前DbContext所在注入周期
        /// </summary>
        public IServiceProvider ServiceProvider;

        /// <summary>
        /// DbContext原生配置
        /// </summary>
        public DbContextOptions DbContextOption { get; }

        /// <summary>
        /// 全局自定义配置
        /// </summary>
        public DynamicDBTOption DynamicDBTOption { get; }

        /// <summary>
        /// 构建参数
        /// </summary>
        public DynamicDBCParamater DynamicDBCParamater { get; }

        internal readonly string CreateStackTrace;
        internal readonly DateTimeOffset CreateTime;
        internal string FirstCallStackTrace;
        public DynamicDbContext(DbContextOptions contextOptions, DynamicDBCParamater dynamicDBCParamater, DynamicDBTOption dynamicDBTOption, IServiceProvider serviceProvider) : base(contextOptions)
        {
          
            ServiceProvider = serviceProvider;
            CreateTime = DateTimeOffset.Now;
            CreateStackTrace = Environment.StackTrace;
            DynamicDBTCache.DynamicDbContexts.Add(this);

            DbContextOption = contextOptions;
            DynamicDBCParamater = dynamicDBCParamater;
            DynamicDBTOption = dynamicDBTOption;

            Database.SetCommandTimeout(dynamicDBTOption.CommandTimeout);
        }
        public DynamicDbContext(DynamicDbContext dbContext) : this(dbContext.DbContextOption, dbContext.DynamicDBCParamater, dbContext.DynamicDBTOption, dbContext.ServiceProvider)
        {
        }
        private static readonly ValueConverter<DateTime, DateTime> _dateTimeConverter = new ValueConverter<DateTime, DateTime>(v => v, v => DateTime.SpecifyKind(v, DateTimeKind.Local));

        /// <summary>
        /// 模型构建
        /// </summary>
        /// <param name="modelBuilder"></param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {            
            FirstCallStackTrace = Environment.StackTrace;
            List<Type> entityTypes;
            if (DynamicDBCParamater.EntityTypes?.Length > 0)
            {
                entityTypes = DynamicDBCParamater.EntityTypes.ToList();
            }
            else
            {
                var q = DynamicDBTOption.Types.Where(x => x.GetCustomAttribute(typeof(TableAttribute), false) != null);

                //通过Namespace解决同表名问题
                if (!string.IsNullOrEmpty(DynamicDBCParamater.EntityNamespace))
                {
                    q = q.Where(x => x.Namespace.Contains(DynamicDBCParamater.EntityNamespace));
                }

                entityTypes = q.ToList();
            }
            entityTypes.ForEach(aEntity =>
            {
                var entity = modelBuilder.Entity(aEntity);

                DynamicDBTOption.EntityTypeBuilderFilter?.Invoke(entity);

                if (!string.IsNullOrEmpty(DynamicDBCParamater.Suffix))
                {
                    entity.ToTable($"{AnnotationHelper.GetDbTableName(aEntity)}_{DynamicDBCParamater.Suffix}", AnnotationHelper.GetDbSchemaName(aEntity));
                }
            });
            //支持IEntityTypeConfiguration配置
            entityTypes.ForEach(aEntityType =>
            {
                var entityTypeConfigurationTypes = DynamicDBTOption.Types
                    .Where(x => x.GetInterfaces().Any(y =>
                        y.IsGenericType
                        && y.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)
                        && aEntityType == y.GetGenericArguments()[0])
                        )
                    .ToList();
                entityTypeConfigurationTypes.ForEach(aEntityConfig =>
                {
                    var method = modelBuilder.GetType().GetMethods()
                        .Where(x => x.Name == nameof(ModelBuilder.ApplyConfiguration)
                            && x.GetParameters().Count() == 1
                            && x.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IEntityTypeConfiguration<>)
                        )
                        .FirstOrDefault();

                    method.MakeGenericMethod(aEntityType).Invoke(modelBuilder, new object[] { Activator.CreateInstance(aEntityConfig) });
                });
            });
            //DateTime默认为Local
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                        property.SetValueConverter(_dateTimeConverter);
                }
            }           
        }

        /// <summary>
        /// 取消跟踪
        /// </summary>
        public void Detach()
        {
            ChangeTracker.Entries().ToList().ForEach(aEntry =>
            {
                if (aEntry.State != EntityState.Detached)
                    aEntry.State = EntityState.Detached;
            });
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override int SaveChanges()
        {
            int count = 0;

            if (DynamicDBTOption.OnSaveChanges != null)
            {
                AsyncHelper.RunSync(() => DynamicDBTOption.OnSaveChanges?.Invoke(ServiceProvider, this, async () =>
                {
                    count = await base.SaveChangesAsync();
                }));
            }
            else
            {
                count = base.SaveChanges();
            }

            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            int count = 0;

            if (DynamicDBTOption.OnSaveChanges != null)
            {
                await DynamicDBTOption.OnSaveChanges?.Invoke(ServiceProvider, this, async () =>
                {
                    count = await base.SaveChangesAsync();
                });
            }
            else
            {
                count = await base.SaveChangesAsync();
            }

            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            DynamicDBTCache.DynamicDbContexts.Remove(this);

            base.Dispose();
        }
    }
}

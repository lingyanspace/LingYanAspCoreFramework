using LingYan.DynamicShardingDBT.DBTExtension;
using LingYan.DynamicShardingDBT.DBTFactory;
using LingYan.DynamicShardingDBT.DBTHelper;
using LingYan.DynamicShardingDBT.DBTModel;
using LingYan.DynamicShardingDBT.DBTProvider;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Reflection;
namespace LingYan.DynamicShardingDBT.ShardingIoc
{
    //分片容器
    internal class DynamicaDBTIoc:IDynamicDBTBuilder, IDynamicDBTConfig
    {
        #region 构造函数

        private readonly IServiceCollection _services;
        public DynamicaDBTIoc(IServiceCollection services)
        {
            _services = services;
        }

        #endregion

        #region 私有成员

        private readonly SynchronizedCollection<DynamicDataSource> _dataSources = new SynchronizedCollection<DynamicDataSource>();
        private readonly SynchronizedCollection<DynamicShardingRule> _shardingRules = new SynchronizedCollection<DynamicShardingRule>();
        private readonly SynchronizedCollection<DynamicPhysicTable> _physicTables = new SynchronizedCollection<DynamicPhysicTable>();

        private List<(string suffix, string conString, DynamicDBType dbType)> GetTargetTables<TEntity>(DynamicReadWriteType dynamicReadWriteType, object obj = null)
        {
            var entityType = typeof(TEntity);
            var rule = _shardingRules.Where(x => x.EntityType == entityType).FirstOrDefault();

            //获取数据库组
            var tables = _physicTables.Where(x => x.EntityType == entityType).ToList();

            //若为写操作则只获取特定表
            if (obj != null)
            {
                string tableSuffix = rule.GetTableSuffixByEntity(obj);
                tables = tables.Where(x => x.Suffix == tableSuffix).ToList();
            }

            //数据库组中数据库负载均衡
            var resList = tables.Select(x =>
            {
                var theSource = _dataSources.Where(y => y.Name == x.DataSourceName).FirstOrDefault();

                var dbs = theSource.Dbs.Where(y => y.dynamicReadWriteType.HasFlag(dynamicReadWriteType)).ToList();
                var theDb = RandomHelper.Next(dbs);

                return (x.Suffix, theDb.connectionString, theSource.DbType);
            }).ToList();

            return resList;
        }
        private void CheckRule<TEntity>(DynamicShardingType dynamicShardingType, string shardingField)
        { 
            if (_shardingRules.Any(x => x.EntityType == typeof(TEntity)))
                throw new Exception($"{typeof(TEntity).Name}已存在分表规则!");

            Type fieldType = typeof(TEntity).GetProperty(shardingField)?.PropertyType;
            if (fieldType == null)
                throw new Exception($"不存在分表字段:{shardingField}");
            if (fieldType.IsNullOrEmpty())
                throw new Exception($"分表字段:{shardingField}不能为可空类型");

            if (dynamicShardingType == DynamicShardingType.Date)
            {
                if (fieldType != typeof(DateTime))
                {
                    throw new Exception($"分表字段:{shardingField}类型必须为DateTime");
                }
            }
        }
        private void AddPhysicTable<TEntity>(string suffix, string sourceName)
        {
            var entityType = typeof(TEntity);

            if (!_physicTables.Any(x => x.EntityType == entityType && x.Suffix == suffix && x.DataSourceName == sourceName))
            {
                _physicTables.Add(new DynamicPhysicTable
                {
                    DataSourceName = sourceName,
                    EntityType = entityType,
                    Suffix = suffix
                });
            }
        }
        private void CreateTable<TEntity>(IServiceProvider serviceProvider, string sourceName, string suffix)
        {
            var theSource = _dataSources.Where(x => x.Name == sourceName).FirstOrDefault();
            theSource.Dbs.ForEach(aDb =>
            {
                serviceProvider.GetService<DynamicDBTFactory>().CreateTable(aDb.connectionString, theSource.DbType, typeof(TEntity), suffix);
            });
        }
        private List<(string suffix, string conString, DynamicDBType dbType)> FilterTable<T>(
            List<(string suffix, string conString, DynamicDBType dbType)> allTables, IQueryable<T> source)
        {
            var entityType = typeof(T);
            string absTable = AnnotationHelper.GetDbTableName(source.ElementType);
            var rule = _shardingRules.Where(x => x.EntityType == entityType).Single();
            var allTableSuffixs = allTables.Select(x => x.suffix).ToList();
            var findSuffixs = ShardingHelper.FilterTable(source, allTableSuffixs, rule);
            allTables = allTables.Where(x => findSuffixs.Contains(x.suffix)).ToList();
#if DEBUG
            Console.WriteLine($"访问分表:{string.Join(",", findSuffixs.Select(x => $"{absTable}_{x}"))}");
#endif
            return allTables;
        }
        private void AddShardingTable(string absTableName, string fullTableName)
        {
            if (!ExistsShardingTables.ContainsKey(absTableName))
            {
                ExistsShardingTables.Add(absTableName, new List<string>());
            }
            ExistsShardingTables[absTableName].Add(fullTableName);
        }

        #endregion

        #region 配置提供

        public List<(string suffix, string conString, DynamicDBType dbType)> GetWriteTables<T>(IQueryable<T> source = null)
        {
            var tables = GetTargetTables<T>(DynamicReadWriteType.Write, null);
            if (source != null)
            {
                tables = FilterTable(tables, source);
            }

            return tables;
        }
        public (string suffix, string conString, DynamicDBType dbType) GetTheWriteTable<T>(T obj)
        {
            return GetTargetTables<T>(DynamicReadWriteType.Write, obj).Single();
        }
        public List<(string suffix, string conString, DynamicDBType dbType)> GetReadTables<T>(IQueryable<T> source)
        {
            var allTables = GetTargetTables<T>(DynamicReadWriteType.Read);

            return FilterTable(allTables, source);
        }
        public DynamicDBType FindADbType()
        {
            return _dataSources.FirstOrDefault().DbType;
        }
        public readonly Dictionary<string, List<string>> ExistsShardingTables
            = new Dictionary<string, List<string>>();

        #endregion

        #region 配置构建

        public IDynamicDBTBuilder SetEntityAssemblies(params Assembly[] assemblies)
        {
            DynamicDBTOption.EntityAssemblies = assemblies;

            return this;
        }
        public IDynamicDBTBuilder SetCommandTimeout(int timeout)
        {
            _services.Configure<DynamicDBTOption>(x =>
            {
                x.CommandTimeout = timeout;
            });

            return this;
        }
        public IDynamicDBTBuilder AddEntityTypeBuilderFilter(Action<EntityTypeBuilder> filter)
        {
            _services.Configure<DynamicDBTOption>(x =>
            {
                x.EntityTypeBuilderFilter += filter;
            });

            return this;
        }
        public IDynamicDBTBuilder MigrationsWithoutForeignKey()
        {
            _services.Configure<DynamicDBTOption>(x =>
            {
                x.MigrationsWithoutForeignKey = true;
            });

            return this;
        }
        public IDynamicDBTBuilder CreateShardingTableOnStarting(bool enable)
        {
            _services.Configure<DynamicDBTOption>(x =>
            {
                x.CreateShardingTableOnStarting = enable;
            });

            return this;
        }
        public IDynamicDBTBuilder EnableShardingMigration(bool enable)
        {
            _services.Configure<DynamicDBTOption>(x =>
            {
                x.EnableShardingMigration = enable;
            });

            return this;
        }
        public IDynamicDBTBuilder EnableComments(bool enable)
        {
            _services.Configure<DynamicDBTOption>(x =>
            {
                x.EnableComments = enable;
            });

            return this;
        }
        public IDynamicDBTBuilder UseLogicDelete(string keyField = "Id", string deletedField = "Deleted")
        {
            _services.Configure<DynamicDBTOption>(x =>
            {
                x.LogicDelete = true;
                x.KeyField = keyField;
                x.DeletedField = deletedField;
            });

            return this;
        }
        public IDynamicDBTBuilder SetMinCommandElapsedMilliseconds(int minCommandElapsedMilliseconds)
        {
            _services.Configure<DynamicDBTOption>(x =>
            {
                x.MinCommandElapsedMilliseconds = minCommandElapsedMilliseconds;
            });
            return this;
        }
        public IDynamicDBTBuilder UseDatabase(string conString, DynamicDBType dbType, string entityNamespace = null, Action<DynamicDBTOption> optionsBuilder = null)
        {
            return UseDatabase<IDynamicDBTService>(conString, dbType, entityNamespace, optionsBuilder);
        }
        public IDynamicDBTBuilder UseDatabase<TDbAccessor>(string conString, DynamicDBType dbType, string entityNamespace, Action<DynamicDBTOption> optionsBuilder = null) where TDbAccessor : class, IDynamicDBTService
        {
            var optionName = typeof(TDbAccessor).FullName;
            _services.AddOptions<DynamicDBTOption>(optionName);

            if (optionsBuilder != null)
            {
                _services.Configure(optionName, optionsBuilder);
            }

            _services.AddScoped(serviceProvider =>
            {
                var dbFactory = serviceProvider.GetService<IDynamicDBTFactory>();
                var options = serviceProvider.GetService<IOptionsMonitor<DynamicDBTOption>>().BuildOption(optionName);
                IDynamicDBTService db = dbFactory.GetDBTService(new DynamicDBCParamater
                {
                    ConnectionString = conString,
                    DynamicDatabase = dbType,
                    EntityNamespace = entityNamespace
                }, optionName);
                if (options.LogicDelete)
                    db = new LogicDeleteDbService(db, options);

                if (typeof(TDbAccessor) == typeof(IDynamicDBTService))
                    return (TDbAccessor)db;
                else
                    return db.ActLike<TDbAccessor>();
            });

            return this;
        }
        public IDynamicDBTBuilder UseDatabase((string connectionString, DynamicReadWriteType readWriteType)[] dbs, DynamicDBType dbType, string entityNamespace = null, Action<DynamicDBTOption> optionsBuilder = null)
        {
            return UseDatabase<IDynamicDBTService>(dbs, dbType, entityNamespace, optionsBuilder);
        }
        public IDynamicDBTBuilder UseDatabase<TDynamicDBTService>((string connectionString, DynamicReadWriteType readWriteType)[] dbs, DynamicDBType dbType, string entityNamespace, Action<DynamicDBTOption> optionsBuilder = null) where TDynamicDBTService : class, IDynamicDBTService
        {
            var optionName = typeof(TDynamicDBTService).FullName;
            _services.AddOptions<DynamicDBTOption>(optionName);

            if (optionsBuilder != null)
            {
                _services.Configure(optionName, optionsBuilder);
            }

            if (!(dbs.Any(x => x.readWriteType.HasFlag(DynamicReadWriteType.Read))
                && dbs.Any(x => x.readWriteType.HasFlag(DynamicReadWriteType.Write))))
                throw new Exception("dbs必须包含写库与读库");

            _services.AddScoped(serviceProvider =>
            {
                var options = serviceProvider.GetService<IOptionsMonitor<DynamicDBTOption>>().BuildOption(optionName);

                IDynamicDBTService db = new ReadWriteDbService(
                    dbs,
                    dbType,
                    entityNamespace,
                    serviceProvider.GetService<IDynamicDBTFactory>(),
                    options
                    );

                if (typeof(IDynamicDBTService) == typeof(IDynamicDBTService))
                    return (IDynamicDBTService)db;
                else
                    return db.ActLike<IDynamicDBTService>();
            });

            return this;
        }
        public IDynamicDBTBuilder AddDataSource(string connectionString, DynamicReadWriteType readWriteType, DynamicDBType dbType, string sourceName = "DefaultSource")
        {
            return AddDataSource(new (string, DynamicReadWriteType)[] { (connectionString, readWriteType) }, dbType, sourceName);
        }
        public IDynamicDBTBuilder AddDataSource((string connectionString, DynamicReadWriteType readWriteType)[] dbs, DynamicDBType dbType, string sourceName = "DefaultSource")
        {
            _dataSources.Add(new DynamicDataSource
            {
                Dbs = dbs,
                DbType = dbType,
                Name = sourceName
            });

            return this;
        }
        public IDynamicDBTBuilder SetDateSharding<TEntity>(string shardingField, DynamicExpandByDateMode expandByDateMode, DateTime startTime, string sourceName = "DefaultSource")
        {
            return SetDateSharding<TEntity>(shardingField, expandByDateMode, (startTime, DateTime.MaxValue, sourceName));
        }
        public IDynamicDBTBuilder SetDateSharding<TEntity>(string shardingField, DynamicExpandByDateMode expandByDateMode, params (DateTime startTime, DateTime endTime, string sourceName)[] ranges)
        {
            CheckRule<TEntity>(DynamicShardingType.Date, shardingField);

            var shardingRule = new DynamicShardingRule
            {
                EntityType = typeof(TEntity),
                DynamicExpandByDateMode = expandByDateMode,
                ShardingField = shardingField,
                DynamicShardingType = DynamicShardingType.Date
            };
            _shardingRules.Add(shardingRule);

            DynamicDBTOption.Bootstrapper += serviceProvider =>
            {
                var sharingOption = serviceProvider.GetService<IOptions<DynamicDBTOption>>().Value;

                (string conExpression, string startTimeFormat, Func<DateTime, DateTime> nextTime) paramter =
                    expandByDateMode switch
                    {
                        DynamicExpandByDateMode.PerMinute => ("0 * * * * ? *", "yyyy/MM/dd HH:mm:00", x => x.AddMinutes(1)),
                        DynamicExpandByDateMode.PerHour => ("0 0 * * * ? *", "yyyy/MM/dd HH:00:00", x => x.AddHours(1)),
                        DynamicExpandByDateMode.PerDay => ("0 0 0 * * ? *", "yyyy/MM/dd 00:00:00", x => x.AddDays(1)),
                        DynamicExpandByDateMode.PerMonth => ("0 0 0 1 * ? *", "yyyy/MM/01 00:00:00", x => x.AddMonths(1)),
                        DynamicExpandByDateMode.PerYear => ("0 0 0 1 1 ? *", "yyyy/01/01 00:00:00", x => x.AddYears(1)),
                        _ => throw new Exception("expandByDateMode参数无效")
                    };

                //确保之前的表已存在
                var theTime = ranges.Min(x => x.startTime);
                theTime = DateTime.Parse(theTime.ToString(paramter.startTimeFormat));

                DateTime endTime = paramter.nextTime(DateTime.Parse(DateTime.Now.ToString(paramter.startTimeFormat)));

                while (theTime <= endTime)
                {
                    var theSourceName = GetSourceName(theTime);
                    string suffix = shardingRule.GetTableSuffixByField(theTime);

                    string absTableName = AnnotationHelper.GetDbTableName(typeof(TEntity));
                    string fullTableName = $"{absTableName}_{suffix}";
                    AddShardingTable(absTableName, fullTableName);

                    //启动时建表
                    if (sharingOption.CreateShardingTableOnStarting)
                    {
                        CreateTable<TEntity>(serviceProvider, theSourceName, suffix);
                    }

                    AddPhysicTable<TEntity>(suffix, theSourceName);

                    theTime = paramter.nextTime(theTime);
                }

                //定时自动建表
                JobHelper.SetCronJob(() =>
                {
                    DateTime trueDate = paramter.nextTime(DateTime.Parse(DateTime.Now.ToString(paramter.startTimeFormat)));
                    var theSourceName = GetSourceName(trueDate);
                    string suffix = shardingRule.GetTableSuffixByField(trueDate);
                    //添加物理表
                    CreateTable<TEntity>(serviceProvider, theSourceName, suffix);
                    AddPhysicTable<TEntity>(suffix, theSourceName);
                }, paramter.conExpression);

                string GetSourceName(DateTime time)
                {
                    return ranges
                        .Where(x => time >= DateTime.Parse(x.startTime.ToString(paramter.startTimeFormat))
                            && time < x.endTime)
                        .FirstOrDefault()
                        .sourceName;
                }
            };

            return this;
        }
        public IDynamicDBTBuilder SetHashModSharding<TEntity>(string shardingField, int mod, string sourceName = "DefaultSource")
        {
            return SetHashModSharding<TEntity>(shardingField, mod, (0, mod, sourceName));
        }
        public IDynamicDBTBuilder SetHashModSharding<TEntity>(string shardingField, int mod, params (int start, int end, string sourceName)[] ranges)
        {
            CheckRule<TEntity>(DynamicShardingType.HashMod, shardingField);

            DynamicShardingRule rule = new DynamicShardingRule
            {
                EntityType = typeof(TEntity),
                ShardingField = shardingField,
                Mod = mod,
                DynamicShardingType = DynamicShardingType.HashMod
            };
            _shardingRules.Add(rule);

            DynamicDBTOption.Bootstrapper += serviceProvider =>
            {
                var sharingOption = serviceProvider.GetService<IOptions<DynamicDBTOption>>().Value;

                //建表
                for (int i = 0; i < mod; i++)
                {
                    var sourceName = ranges.Where(x => i >= x.start && i < x.end).FirstOrDefault().sourceName;

                    string absTableName = AnnotationHelper.GetDbTableName(typeof(TEntity));
                    string fullTableName = $"{absTableName}_{i}";
                    AddShardingTable(absTableName, fullTableName);

                    //启动时建表
                    if (sharingOption.CreateShardingTableOnStarting)
                    {
                        CreateTable<TEntity>(serviceProvider, sourceName, i.ToString());
                    }

                    AddPhysicTable<TEntity>(i.ToString(), sourceName);
                }
            };

            return this;
        }

        #endregion
    }
}

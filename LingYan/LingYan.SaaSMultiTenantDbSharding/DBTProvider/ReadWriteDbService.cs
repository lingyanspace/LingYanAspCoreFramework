using LingYan.DynamicShardingDBT.DBTFactory;
using LingYan.DynamicShardingDBT.DBTHelper;
using LingYan.DynamicShardingDBT.DBTModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Data;

namespace LingYan.DynamicShardingDBT.DBTProvider
{
    internal class ReadWriteDbService : DefaultDbService, IDynamicDBTService
    {
        #region 私有成员

        private readonly (string connectionString, DynamicReadWriteType readWriteType)[] _dbConfigs;
        private readonly DynamicDBType _dbType;
        private readonly string _entityNamespace;
        private readonly bool _logicDelete;
        private readonly IDynamicDBTFactory _dbFactory;
        private readonly DynamicDBTOption _shardingOptions;
        public ReadWriteDbService((string connectionString, DynamicReadWriteType readWriteType)[] dbs, DynamicDBType dbType, string entityNamespace, IDynamicDBTFactory dbFactory,
            DynamicDBTOption shardingOptions)
        {
            _dbConfigs = dbs;
            _entityNamespace = entityNamespace;
            _dbType = dbType;
            _logicDelete = shardingOptions.LogicDelete;
            _dbFactory = dbFactory;
            _shardingOptions = shardingOptions;
        }

        private (IDynamicDBTService db, DynamicReadWriteType readWriteType)[] _allDbs;
        private (IDynamicDBTService db, DynamicReadWriteType readWriteType)[] AllDbs
        {
            get
            {
                if (_allDbs == null)
                {
                    _allDbs = _dbConfigs
                        .Select(x => (_dbFactory.GetDBTService(new DynamicDBCParamater
                        {
                            ConnectionString = x.connectionString,
                            DynamicDatabase = _dbType,
                            EntityNamespace = _entityNamespace
                        }), x.readWriteType))
                        .ToArray();
                }

                return _allDbs;
            }
        }
        private IDynamicDBTService GetRandomDb(DynamicReadWriteType readWriteType)
        {
            var dbs = AllDbs.Where(x => x.readWriteType.HasFlag(readWriteType)).ToList();

            var theDb = RandomHelper.Next(dbs).db;

            if (_logicDelete)
                theDb = new LogicDeleteDbService(theDb, _shardingOptions);

            return theDb;
        }
        private IDynamicDBTService _writeDb;
        private IDynamicDBTService _readDb;
        private IDynamicDBTService WriteDb
        {
            get
            {
                if (_writeDb == null)
                {
                    _writeDb = GetRandomDb(DynamicReadWriteType.Write);
                }

                return _writeDb;
            }
        }
        private IDynamicDBTService ReadDb
        {
            get
            {
                if (_openedTransaction)
                {
                    return WriteDb;
                }
                else
                {
                    if (_readDb == null)
                    {
                        _readDb = GetRandomDb(DynamicReadWriteType.Read);
                    }

                    return _readDb;
                }
            }
        }
        private bool _openedTransaction = false;
        private bool _disposed = false;

        #endregion
        public override EntityEntry Entry(object entity)
        {
            return WriteDb.Entry(entity);
        }
        public override string ConnectionString => throw new Exception("读写分离模式不支持");
        public override DynamicDBType DbType => throw new Exception("读写分离模式不支持");
        public override IDynamicDBTService FullDbAccessor => throw new Exception("读写分离模式不支持");
        public override void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _writeDb?.Dispose();
            _readDb?.Dispose();
        }
        public override void BulkInsert<T>(List<T> entities, string tableName = null)
        {
            WriteDb.BulkInsert(entities, tableName);
        }
        public override Task<int> DeleteSqlAsync(IQueryable source)
        {
            return WriteDb.DeleteSqlAsync(source);
        }
        public override Task<int> ExecuteSqlAsync(string sql, params (string paramterName, object paramterValue)[] parameters)
        {
            return WriteDb.ExecuteSqlAsync(sql, parameters);
        }
        public override Task<T> GetEntityAsync<T>(params object[] keyValue)
        {
            return ReadDb.GetEntityAsync<T>(keyValue);
        }
        public override IQueryable<T> GetIQueryable<T>(bool tracking = false)
        {
            var db = tracking ? WriteDb : ReadDb;

            return db.GetIQueryable<T>(tracking);
        }
        public override Task<int> SaveChangesAsync(bool tracking = true)
        {
            return WriteDb.SaveChangesAsync(tracking);
        }
        public override Task<int> UpdateSqlAsync(IQueryable source, params (string field, DynamicUpdateType updateType, object value)[] values)
        {
            return WriteDb.UpdateSqlAsync(source, values);
        }
        public override Task BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            return WriteDb.BeginTransactionAsync(isolationLevel);
        }
        public override void CommitTransaction()
        {
            WriteDb.CommitTransaction();
        }
        public override void DisposeTransaction()
        {
            WriteDb.DisposeTransaction();
        }
        public override void RollbackTransaction()
        {
            WriteDb.RollbackTransaction();
        }
        public override Task<int> DeleteAsync<T>(List<T> entities)
        {
            return WriteDb.DeleteAsync<T>(entities);
        }
        public override Task<int> InsertAsync<T>(List<T> entities, bool tracking = false)
        {
            return WriteDb.InsertAsync(entities, tracking);
        }
        public override Task<int> UpdateAsync<T>(List<T> entities, bool tracking = false)
        {
            return WriteDb.UpdateAsync(entities, tracking);
        }
        public override Task<int> UpdateAsync<T>(List<T> entities, List<string> properties, bool tracking = false)
        {
            return WriteDb.UpdateAsync(entities, properties, tracking);
        }
    }
}

using LingYan.DynamicShardingDBT.DBTExtension;
using LingYan.DynamicShardingDBT.DBTModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Data;
using System.Linq.Dynamic.Core;

namespace LingYan.DynamicShardingDBT.DBTProvider
{
    internal class LogicDeleteDbService : DefaultDbService, IDynamicDBTService
    {
        private bool _logicDelete;
        private string _deletedField;
        private string _keyField;
        public LogicDeleteDbService(IDynamicDBTService db, DynamicDBTOption dynamicDBTOption)
        {
            FullDbAccessor = db;
            _logicDelete = dynamicDBTOption.LogicDelete;
            _deletedField = dynamicDBTOption.DeletedField;
            _keyField = dynamicDBTOption.KeyField;
        }
        public override IDynamicDBTService FullDbAccessor { get; }
        bool NeedLogicDelete(Type entityType)
        {
            return _logicDelete && entityType.GetProperties().Any(x => x.Name == _deletedField);
        }

        #region 重写

        public override async Task<int> DeleteAsync<T>(List<string> keys)
        {
            var entities = await GetIQueryable<T>().Where($"@0.Contains({_keyField})", keys).ToListAsync();

            return await DeleteAsync(entities);
        }
        public override async Task<int> DeleteAsync<T>(List<T> entities)
        {
            if (entities?.Count > 0)
            {
                if (NeedLogicDelete(typeof(T)))
                {
                    entities.ForEach(aData =>
                    {
                        aData.SetPropertyValue(_deletedField, true);
                    });

                    return await UpdateAsync<T>(entities);
                }
                else
                {
                    return await FullDbAccessor.DeleteAsync<T>(entities);
                }
            }
            else
                return 0;
        }
        public override async Task<int> DeleteSqlAsync(IQueryable source)
        {
            if (NeedLogicDelete(source.ElementType))
                return await UpdateSqlAsync(source, (_deletedField, DynamicUpdateType.Equal, true));
            else
                return await FullDbAccessor.DeleteSqlAsync(source);
        }
        public override IQueryable<T> GetIQueryable<T>(bool tracking = false)
        {
            var q = FullDbAccessor.GetIQueryable<T>(tracking);
            if (NeedLogicDelete(typeof(T)))
            {
                q = q.Where($"{_deletedField} = @0", false);
            }

            return q;
        }
        public override EntityEntry Entry(object entity)
        {
            return FullDbAccessor.Entry(entity);
        }

        #endregion

        #region 忽略
        public override string ConnectionString => FullDbAccessor.ConnectionString;
        public override DynamicDBType DbType => FullDbAccessor.DbType;
        public override Task BeginTransactionAsync(IsolationLevel isolationLevel)
        {
            return FullDbAccessor.BeginTransactionAsync(isolationLevel);
        }
        public override void BulkInsert<T>(List<T> entities, string tableName = null)
        {
            FullDbAccessor.BulkInsert(entities, tableName);
        }
        public override void CommitTransaction()
        {
            FullDbAccessor.CommitTransaction();
        }
        public override void Dispose()
        {
            FullDbAccessor.Dispose();
        }
        public override void DisposeTransaction()
        {
            FullDbAccessor.DisposeTransaction();
        }
        public override Task<int> ExecuteSqlAsync(string sql, params (string paramterName, object paramterValue)[] parameters)
        {
            return FullDbAccessor.ExecuteSqlAsync(sql, parameters);
        }
        public override Task<T> GetEntityAsync<T>(params object[] keyValue)
        {
            return FullDbAccessor.GetEntityAsync<T>(keyValue);
        }
        public override Task<int> InsertAsync<T>(List<T> entities, bool tracking = false)
        {
            return FullDbAccessor.InsertAsync(entities, tracking);
        }
        public override void RollbackTransaction()
        {
            FullDbAccessor.RollbackTransaction();
        }
        public override Task<int> SaveChangesAsync(bool tracking = true)
        {
            return FullDbAccessor.SaveChangesAsync(tracking);
        }
        public override Task<int> UpdateAsync<T>(List<T> entities, bool tracking = false)
        {
            return FullDbAccessor.UpdateAsync(entities, tracking);
        }
        public override Task<int> UpdateAsync<T>(List<T> entities, List<string> properties, bool tracking = false)
        {
            return FullDbAccessor.UpdateAsync(entities, properties, tracking);
        }
        public override Task<int> UpdateSqlAsync(IQueryable source, params (string field, DynamicUpdateType updateType, object value)[] values)
        {
            return FullDbAccessor.UpdateSqlAsync(source, values);
        }

        #endregion
    }
}

using LingYan.DynamicShardingDBT.DBTModel;

namespace LingYan.DynamicShardingDBT.DBTProvider
{
    public interface IDynamicDBTConfig
    {
        DynamicDBType FindADbType();
        List<(string suffix, string conString, DynamicDBType dbType)> GetReadTables<T>(IQueryable<T> source);
        List<(string suffix, string conString, DynamicDBType dbType)> GetWriteTables<T>(IQueryable<T> source = null);
        (string suffix, string conString, DynamicDBType dbType) GetTheWriteTable<T>(T obj);
    }
}

using MySqlConnector;

namespace LingYan.DynamicShardingDBT.DBTExtension
{
    public static class SqlExtension
    {     
        /// <summary>
        /// 判断表是否存在
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static bool QueryTableExist(this string connectionString, string tableName)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @tableName";
                    command.Parameters.Add(new MySqlParameter("@tableName", tableName));

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    return count > 0;
                }
            }          
        }
        /// <summary>
        /// 判断数据库是否存在,若不存在则创建，若存在则继续
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="charset"></param>
        /// <returns></returns>
        public static bool QueryDatabaseIfNotExists(this string connectionString,string charset = "utf8mb4")
        {
            // 从连接字符串中提取数据库名
            var builder = new MySqlConnectionStringBuilder(connectionString);
            string databaseName = builder.Database;
            // 暂时移除数据库名称以连接到 MySQL 服务器
            builder.Database = null;
            string serverConnectionString = builder.ConnectionString;
            using (var connection = new MySqlConnection(serverConnectionString))
            {
                connection.Open();
                using (var command = new MySqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = $"SELECT COUNT(*) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = '{databaseName}';";

                    int count = Convert.ToInt32(command.ExecuteScalar());
                    if (count == 0)
                    {
                        // 数据库不存在，创建数据库
                        command.CommandText = $"CREATE DATABASE IF NOT EXISTS {databaseName} DEFAULT CHARACTER SET {charset};";
                        int result = command.ExecuteNonQuery();
                        return result > 0;
                    }
                    else
                    {
                        // 数据库已存在，不执行任何操作
                        return true;
                    }
                }
            }

        }
    }
}

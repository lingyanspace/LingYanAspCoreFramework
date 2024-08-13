﻿using LingYan.DynamicShardingDBT.DBTContext;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;

namespace LingYan.DynamicShardingDBT.DBTProvider
{
    public abstract class DynamicDBTProvider
    {       
        public abstract DbProviderFactory DbProviderFactory { get; }       
        public DataAdapter GetDataAdapter() => DbProviderFactory.CreateDataAdapter();
        public abstract IDynamicDBTService GetDynamicDBTService(DynamicDbContext baseDbContext);
        public DbCommand GetDbCommand() => DbProviderFactory.CreateCommand();      
        public DbConnection GetDbConnection() => DbProviderFactory.CreateConnection();
        public DbParameter GetDbParameter() => DbProviderFactory.CreateParameter();        
        public abstract ModelBuilder GetModelBuilder();
        public abstract void UseDatabase(DbContextOptionsBuilder dbContextOptionsBuilder, DbConnection dbConnection);
    }
}
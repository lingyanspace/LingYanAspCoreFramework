using LingYan.DynamicShardingDBT.DBTExtension;

namespace LingYan.DynamicShardingDBT.DBTModel
{
    internal class DynamicShardingRule
    {
        public Type EntityType { get; set; }
        public DynamicShardingType DynamicShardingType { get; set; }
        public string ShardingField { get; set; }
        public int Mod { get; set; }
        public DynamicExpandByDateMode? DynamicExpandByDateMode { get; set; } 
        public string GetTableSuffixByField(object fieldValue)
        {
            switch (DynamicShardingType)
            {
                case DynamicShardingType.HashMod:
                    {
                        long suffix;
                        if (fieldValue.GetType() == typeof(int) || fieldValue.GetType() == typeof(long))
                        {
                            long longValue = (long)fieldValue;
                            if (longValue < 0)
                                throw new Exception($"字段{ShardingField}不能小于0");

                            suffix = longValue % Mod;
                        }
                        else
                        {
                            suffix = Math.Abs(fieldValue.ToString().GetStableHashCode()) % Mod;
                        }

                        return suffix.ToString();
                    };
                case DynamicShardingType.Date:
                    {
                        string format = DynamicExpandByDateMode switch
                        {
                            LingYan.DynamicShardingDBT.DBTModel.DynamicExpandByDateMode.PerMinute => "yyyyMMddHHmm",
                            LingYan.DynamicShardingDBT.DBTModel.DynamicExpandByDateMode.PerHour => "yyyyMMddHH",
                            LingYan.DynamicShardingDBT.DBTModel.DynamicExpandByDateMode.PerDay => "yyyyMMdd",
                            LingYan.DynamicShardingDBT.DBTModel.DynamicExpandByDateMode.PerMonth => "yyyyMM",
                            LingYan.DynamicShardingDBT.DBTModel.DynamicExpandByDateMode.PerYear => "yyyy",
                            _ => throw new Exception("ExpandByDateMode无效")
                        };

                        return ((DateTime)fieldValue).ToString(format);
                    };
                default: throw new Exception("ShardingType无效");
            }
        }
        public string GetTableSuffixByEntity(object entity)
        {
            var property = entity.GetPropertyValue(ShardingField);

            return GetTableSuffixByField(property);
        }
    }
}

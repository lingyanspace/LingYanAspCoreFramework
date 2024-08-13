using LingYan.DynamicShardingDBT.DBTModel;
using Microsoft.Extensions.Options;

namespace LingYan.DynamicShardingDBT.DBTExtension
{
    public static class OptionExtension
    {
        public static DynamicDBTOption BuildOption(this IOptionsMonitor<DynamicDBTOption> doptMonior, string optionName)
        {
            if (optionName.IsNullOrEmpty())
            {
                return doptMonior.CurrentValue;
            }
            else
            {
                var selfOption = doptMonior.Get(optionName);
                var defaultOption = new DynamicDBTOption();
                var globalOption = doptMonior.CurrentValue;

                foreach (var aProperty in typeof(DynamicDBTOption).GetProperties())
                {
                    var selfValue = aProperty.GetValue(selfOption);
                    var defaultValue = aProperty.GetValue(defaultOption);
                    var globalValue = aProperty.GetValue(globalOption);

                    var value = Equals(selfValue, defaultValue) ? globalValue : selfValue;
                    aProperty.SetValue(selfOption, value);
                }

                return selfOption;
            }
        }
    }
}

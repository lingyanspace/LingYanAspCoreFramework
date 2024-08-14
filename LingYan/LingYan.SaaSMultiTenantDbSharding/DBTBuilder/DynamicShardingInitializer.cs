using LingYan.DynamicShardingDBT.DBTCache;
using LingYan.DynamicShardingDBT.DBTHelper;
using LingYan.DynamicShardingDBT.DBTModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace LingYan.DynamicShardingDBT.DBTBuilder
{
    public class DynamicShardingInitializer : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DynamicDBTOption _shardingOptions;
        /// <summary>
        /// 构造函数
        /// </summary> 
        /// <param name="serviceProvider"></param>
        public DynamicShardingInitializer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _shardingOptions = serviceProvider.GetService<IOptions<DynamicDBTOption>>().Value;

            DiagnosticListener.AllListeners.Subscribe(
                new DiagnosticObserver(serviceProvider.GetService<ILoggerFactory>(),
                _shardingOptions.MinCommandElapsedMilliseconds));

            DynamicDBTCache.ServiceProvider = serviceProvider;
        }
        /// <summary>
        /// 加载 
        /// </summary>
        /// <param name="stoppingToken"></param>
        /// <returns></returns>
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var scope = _serviceProvider.CreateScope();
            DynamicDBTOption.InitStartup?.Invoke(scope.ServiceProvider);

            //长时间未释放监控,5分钟
            JobHelper.SetIntervalJob(() =>
            {
                var list = DynamicDBTCache.DynamicDbContexts.Where(x => (DateTimeOffset.Now - x.CreateTime).TotalMinutes > 5).ToList();
                list.ForEach(x =>
                {
                    var logger = x.ServiceProvider.GetService<ILoggerFactory>()?.CreateLogger(GetType());
                    logger?.LogWarning("DbContext长时间({ElapsedMinutes}m)未释放 CreateStackTrace:{CreateStackTrace} FirstCallStackTrace:{FirstCallStackTrace}",
                        (long)(DateTimeOffset.Now - x.CreateTime).TotalMinutes, x.CreateStackTrace, x.FirstCallStackTrace);
                });
            }, TimeSpan.FromMinutes(5));

            return Task.CompletedTask;
        }
    }
}

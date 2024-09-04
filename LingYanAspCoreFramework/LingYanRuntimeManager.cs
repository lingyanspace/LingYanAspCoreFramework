using LingYanAspCoreFramework.Models;
using Microsoft.Extensions.Configuration;
namespace LingYanAspCoreFramework
{
    public class LingYanRuntimeManager
    {
        //跨域策略
        public static string CrossPolicy { get; set; }
        //鉴权方案
        public static string BearerScheme { get; set; }
        //授权方案
        public static string EmpowerPolicy { get; set; }
        //根目录
        public static string HostPhysicsRoot { get; set; }
        //动态路由前缀
        public static string DynamicHttpRoutePrefix { get; set; }
        //多租户路由前缀
        public static string TenantRoutePrefix { get; set; }
        //跨域名单集合
        public static string[] CrossDomains { get; set; }
        //启动监听端口集合
        public static string[] ListeningPorts { get; set; }
        //代码类的临时缓存
        public static RuntimeCacheModel RuntimeCacheModel { get; set; }
        //JWT实体
        public static JwtModel JwtModel { get; set; }
        //redis连接字符串配置
        public static RedisCofigModel RedisCofigModel { get; set; }
        //雪花ID配置
        public static IdGeneratorOptionConfigModel IdGeneratorOptionConfigModel { get; set; }
        //云视频流配置
        public static CloudVodLiveConfigModel CloudVodLiveConfigModel { get; set; }
        //动态路由规则配置
        public static List<HttpMethodConfigure> HttpMethodConfigure { get; set; }
        public static MysqlConfigModel MysqlConfigModel { get; set; }
        //FFMpegConfig配置
        public static CommonConfigModel CommonConfigModel { get; set; }
        //系统的Configumaanger
        public static ConfigurationManager SysConfiguration { get; set; }
        public static void Init()
        {
            CrossPolicy = "LYCrossPolicy";
            BearerScheme = "Bearer";
            EmpowerPolicy = "Empower";
            DynamicHttpRoutePrefix = "dynamicly";
            TenantRoutePrefix = "/api/tenant";
            CrossDomains = new string[] { "http://*:8888" };
            ListeningPorts = new string[] { "http://*:8888" };
            HostPhysicsRoot = AppDomain.CurrentDomain.BaseDirectory;
            RuntimeCacheModel = new RuntimeCacheModel();
            JwtModel = new JwtModel();
            RedisCofigModel = new RedisCofigModel();
            IdGeneratorOptionConfigModel = new IdGeneratorOptionConfigModel();
            CloudVodLiveConfigModel = new CloudVodLiveConfigModel();
            HttpMethodConfigure = new List<HttpMethodConfigure>();
            MysqlConfigModel = new MysqlConfigModel();
            CommonConfigModel = new CommonConfigModel();
            RuntimeCacheModel.LingYanConfiguration.AddJsonFile(Path.Combine(HostPhysicsRoot, "Environments", "Configurations", "LingYanSetting.json"));
            Config();
        }
        private static void Config()
        {
            //配置静态文件目录
            CrossDomains = RuntimeCacheModel.LingYanConfiguration.GetSection("CrossDomains").Get<string[]>();
            ListeningPorts = RuntimeCacheModel.LingYanConfiguration.GetSection("ListeningPorts").Get<string[]>();
            DynamicHttpRoutePrefix = RuntimeCacheModel.LingYanConfiguration.GetSection("DynamicHttpRoutePrefix").Get<string>();
            JwtModel = RuntimeCacheModel.LingYanConfiguration.GetSection("JwtModel").Get<JwtModel>();
            RedisCofigModel = RuntimeCacheModel.LingYanConfiguration.GetSection("RedisCofigModel").Get<RedisCofigModel>();
            TenantRoutePrefix = RuntimeCacheModel.LingYanConfiguration.GetSection("TenantRoutePrefix").Get<string>();
            IdGeneratorOptionConfigModel = RuntimeCacheModel.LingYanConfiguration.GetSection("IdGeneratorOptionConfigModel").Get<IdGeneratorOptionConfigModel>();
            CloudVodLiveConfigModel = RuntimeCacheModel.LingYanConfiguration.GetSection("CloudVodLiveConfigModel").Get<CloudVodLiveConfigModel>();
            HttpMethodConfigure = RuntimeCacheModel.LingYanConfiguration.GetSection("HttpMethodConfigure").Get<List<HttpMethodConfigure>>();
            MysqlConfigModel = RuntimeCacheModel.LingYanConfiguration.GetSection("MysqlConfigModel").Get<MysqlConfigModel>();
            CommonConfigModel = RuntimeCacheModel.LingYanConfiguration.GetSection("CommonConfigModel").Get<CommonConfigModel>();
        }
    }
}

using LingYan.Model.CloudVodLiveModel;
using LingYan.Model.CommonModel;
using LingYan.Model.DynamicHttpMethodModel;
using LingYan.Model.IdGeneratorOptionModel;
using LingYan.Model.JWTModel;
using LingYan.Model.MysqlModel;
using LingYan.Model.RedisModel;
using Microsoft.Extensions.Configuration;

namespace LingYan.Model
{
    public class LYExpose
    {
        //代码类的临时缓存
        public static LYBuilderRuntimeModel LYBuilderRuntimeManager { get; set; }
        //跨域策略
        public const string CrossPolicy = "LYCrossPolicy";
        //鉴权方案
        public const string BearerScheme = "Bearer";
        //授权方案
        public const string EmpowerPolicy = "Empower";
        //根目录
        public static string HostPhysicsRoot;

        ////权限文件
        //public static string HostPhysicsFileLimit;
        ////开放文件
        //public static string HostPhysicsFileOpenness;
        ////文件网络路由
        //public static string HostNetWorkFileLimit = "NetWorkFileLimit";
        //public static string HostNetWorkFileOpenness = "NetWorkFileOpenness";      
        //跨域名单集合
        public static string[] CrossDomains;
        //启动监听端口集合
        public static string[] ListeningPorts;
        //动态路由前缀
        public static string DynamicHttpRoutePrefix;
        //JWT实体
        public static JwtModel JwtModel { get; set; }
        //日志路径
        public static string NlogConfig { get; set; }
        //redis连接字符串配置
        public static RedisCofigModel RedisCofigModel { get; set; }
        //多租户路由前缀
        public static string TenantRoutePrefix { get; set; }
        //雪花ID配置
        public static IdGeneratorOptionConfigModel IdGeneratorOptionConfigModel { get; set; }
        //云视频流配置
        public static CloudVodLiveConfigModel CloudVodLiveConfigModel { get; set; }
        //动态路由规则配置
        public static List<HttpMethodConfigure> HttpMethodConfigure { get; set; }
        public static MysqlConfigModel MysqlConfigModel { get; set; }
        //FFMpegConfig配置
        public static CommonConfigModel CommonConfigModel { get; set; } 
        public static void Init()
        {
            LYExpose.LYBuilderRuntimeManager = new LYBuilderRuntimeModel();
            LYExpose.HostPhysicsRoot = AppDomain.CurrentDomain.BaseDirectory;
        }
        public static void Config()
        {
            //配置静态文件目录
            LYExpose.CrossDomains = LYBuilderRuntimeManager.ConfigurationManager.GetSection("CrossDomains").Get<string[]>();
            LYExpose.ListeningPorts = LYBuilderRuntimeManager.ConfigurationManager.GetSection("ListeningPorts").Get<string[]>();
            LYExpose.DynamicHttpRoutePrefix = LYBuilderRuntimeManager.ConfigurationManager.GetSection("DynamicHttpRoutePrefix").Get<string>();
            LYExpose.JwtModel = LYBuilderRuntimeManager.ConfigurationManager.GetSection("JwtModel").Get<JwtModel>();
            LYExpose.NlogConfig = LYBuilderRuntimeManager.ConfigurationManager.GetSection("NlogConfig").Get<string>();
            LYExpose.RedisCofigModel = LYBuilderRuntimeManager.ConfigurationManager.GetSection("RedisCofigModel").Get<RedisCofigModel>();
            LYExpose.TenantRoutePrefix = LYBuilderRuntimeManager.ConfigurationManager.GetSection("TenantRoutePrefix").Get<string>();
            LYExpose.IdGeneratorOptionConfigModel = LYBuilderRuntimeManager.ConfigurationManager.GetSection("IdGeneratorOptionConfigModel").Get<IdGeneratorOptionConfigModel>();
            LYExpose.CloudVodLiveConfigModel = LYBuilderRuntimeManager.ConfigurationManager.GetSection("CloudVodLiveConfigModel").Get<CloudVodLiveConfigModel>();
            LYExpose.HttpMethodConfigure = LYBuilderRuntimeManager.ConfigurationManager.GetSection("HttpMethodConfigure").Get<List<HttpMethodConfigure>>();
            LYExpose.MysqlConfigModel = LYBuilderRuntimeManager.ConfigurationManager.GetSection("MysqlConfigModel").Get<MysqlConfigModel>();
            LYExpose.CommonConfigModel= LYBuilderRuntimeManager.ConfigurationManager.GetSection("CommonConfigModel").Get<CommonConfigModel>();
        }
      
    }
}

using LingYan.Model.CloudVodLiveModel;
using LingYan.Model.CommonModel;
using LingYan.Model.DynamicHttpMethodModel;
using LingYan.Model.IdGeneratorOptionModel;
using LingYan.Model.JWTModel;
using LingYan.Model.MysqlModel;
using LingYan.Model.RedisModel;
using Microsoft.Extensions.Configuration;
using System.Runtime.CompilerServices;
namespace LingYan.Model
{
    public class LYExpose
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
        public static LYBuilderRuntimeModel LYBuilderRuntimeManager { get; set; }
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
            LYExpose.CrossPolicy = "LYCrossPolicy";
            LYExpose.BearerScheme = "Bearer";
            LYExpose.EmpowerPolicy = "Empower";
            LYExpose.DynamicHttpRoutePrefix = "dynamicly";
            LYExpose.TenantRoutePrefix = "/api/tenant";
            LYExpose.CrossDomains = new string[] { "http://*:8888" };
            LYExpose.ListeningPorts = new string[] { "http://*:8888" };
            LYExpose.HostPhysicsRoot = AppDomain.CurrentDomain.BaseDirectory;
            LYExpose.LYBuilderRuntimeManager = new LYBuilderRuntimeModel();
            LYExpose.JwtModel = new JwtModel();
            LYExpose.RedisCofigModel = new RedisCofigModel();
            LYExpose.IdGeneratorOptionConfigModel = new IdGeneratorOptionConfigModel();
            LYExpose.CloudVodLiveConfigModel = new CloudVodLiveConfigModel();
            LYExpose.HttpMethodConfigure = new List<HttpMethodConfigure>();
            LYExpose.MysqlConfigModel = new MysqlConfigModel();
            LYExpose.CommonConfigModel = new CommonConfigModel();
            LYExpose.LYBuilderRuntimeManager.LingYanConfiguration.AddJsonFile(Path.Combine(LYExpose.HostPhysicsRoot, "Environments", "Configurations", "LingYanSetting.json"));
            LYExpose.Config();
        }
        private static void Config()
        {
            //配置静态文件目录
            LYExpose.CrossDomains = LYBuilderRuntimeManager.LingYanConfiguration.GetSection("CrossDomains").Get<string[]>();
            LYExpose.ListeningPorts = LYBuilderRuntimeManager.LingYanConfiguration.GetSection("ListeningPorts").Get<string[]>();
            LYExpose.DynamicHttpRoutePrefix = LYBuilderRuntimeManager.LingYanConfiguration.GetSection("DynamicHttpRoutePrefix").Get<string>();
            LYExpose.JwtModel = LYBuilderRuntimeManager.LingYanConfiguration.GetSection("JwtModel").Get<JwtModel>();        
            LYExpose.RedisCofigModel = LYBuilderRuntimeManager.LingYanConfiguration.GetSection("RedisCofigModel").Get<RedisCofigModel>();
            LYExpose.TenantRoutePrefix = LYBuilderRuntimeManager.LingYanConfiguration.GetSection("TenantRoutePrefix").Get<string>();
            LYExpose.IdGeneratorOptionConfigModel = LYBuilderRuntimeManager.LingYanConfiguration.GetSection("IdGeneratorOptionConfigModel").Get<IdGeneratorOptionConfigModel>();
            LYExpose.CloudVodLiveConfigModel = LYBuilderRuntimeManager.LingYanConfiguration.GetSection("CloudVodLiveConfigModel").Get<CloudVodLiveConfigModel>();
            LYExpose.HttpMethodConfigure = LYBuilderRuntimeManager.LingYanConfiguration.GetSection("HttpMethodConfigure").Get<List<HttpMethodConfigure>>();
            LYExpose.MysqlConfigModel = LYBuilderRuntimeManager.LingYanConfiguration.GetSection("MysqlConfigModel").Get<MysqlConfigModel>();
            LYExpose.CommonConfigModel= LYBuilderRuntimeManager.LingYanConfiguration.GetSection("CommonConfigModel").Get<CommonConfigModel>();
        }
      
    }
}

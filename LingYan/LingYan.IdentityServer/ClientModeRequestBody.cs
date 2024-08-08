namespace LingYan.IdentityServer
{
    //发起请求的实体
    public class ClientModeRequestBody
    {
        //客户端Id 
        public string Client_Id { get; set; }
        //授权服务器密钥
        public string Client_Secret { get; set; }     
        //模式类型
        public string Grant_Type { get; set; }
    }
}

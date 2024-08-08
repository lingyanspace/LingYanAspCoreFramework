namespace LingYan.IdentityServer
{
    public class ClientModeResponceBody
    {
        //身份令牌
        public string AccessToken { get; set; }
        //令牌类型
        public string TokenType { get; set; }     
        //有效期限
        public long ExpiresIn { get; set; }
        //权限作用域
        public string Scope  { get; set; }     
    }
}

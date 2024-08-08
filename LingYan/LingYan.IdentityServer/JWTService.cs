using LingYan.Extension;
using LingYan.Model;

namespace LingYan.IdentityServer
{
    public class JWTService : IJWTService
    {
        public ClientModeResponceBody GenerateTokenByClientCredentials(string clientId, string clientSecret)
        {
            // 验证 clientId 和 clientSecret

            // 生成令牌
            Dictionary<string, string> tokenField = new Dictionary<string, string>();
            tokenField.Add("clientId", clientId);
            tokenField.Add("clientSecret", clientSecret);
            tokenField.Add("Scope", "read write");
            var backBody = tokenField.CreateJwtToken(DefaultCont.PublicKey);
            return new ClientModeResponceBody
            {
                AccessToken = backBody.Data,
                TokenType = "Bearer",
                ExpiresIn = 3600,
                Scope = "read write"
            };
        }
    }
}

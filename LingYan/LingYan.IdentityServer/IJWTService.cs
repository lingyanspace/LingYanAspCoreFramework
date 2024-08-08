namespace LingYan.IdentityServer
{
    public interface IJWTService
    {
        ClientModeResponceBody GenerateTokenByClientCredentials(string clientId, string clientSecret);
    }
}
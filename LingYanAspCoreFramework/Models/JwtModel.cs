namespace LingYanAspCoreFramework.Models
{
    public class JwtModel
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public long Expres { get; set; }
        public string SecretKey { get; set; }       
    }
}

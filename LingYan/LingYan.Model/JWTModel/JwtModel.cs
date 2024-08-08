using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingYan.Model.JWTModel
{
    public class JwtModel
    {
        public string Issuer { get; set; }
        public string Audience { get; set; }
        public long Expres { get; set; }
        public string SecretKey { get; set; }
        public JwtModel()
        {
            this.Issuer = "LingYanIssuer";
            this.Audience = "LingYanAudience";
            this.Expres = 7;
            this.SecretKey = "IEFIEWPFEWFOUEWHGIWGOIDSMFNCJSDNFJdhfidsghsdnvjdsvsnvsd";
        }
    }
}

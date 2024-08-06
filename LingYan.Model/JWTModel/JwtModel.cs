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
        public double Expres { get; set; }
        public string SecretKey { get; set; } 
    }
}

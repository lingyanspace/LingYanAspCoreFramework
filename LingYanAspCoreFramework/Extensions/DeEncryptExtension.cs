using LingYanAspCoreFramework.Models;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LingYanAspCoreFramework.Extensions
{
    public static class DeEncryptExtension
    {
        /// <summary>
        /// 加密字符串
        /// </summary>
        /// <param name="plaintext"></param>
        /// <param name="publicKey"></param>
        /// <returns></returns>
        public static ResponceBody<string> EncryptWithRSA(this string plaintext, string publicKey)
        {
            try
            {
                byte[] publicKeyBytes = Convert.FromBase64String(publicKey);
                AsymmetricKeyParameter rsaPublicKey = PublicKeyFactory.CreateKey(publicKeyBytes);
                IBufferedCipher cipher = CipherUtilities.GetCipher("RSA/None/PKCS1Padding");
                cipher.Init(true, rsaPublicKey);
                byte[] data = Encoding.UTF8.GetBytes(plaintext);
                byte[] encryptedData = cipher.DoFinal(data);
                return new ResponceBody<string>(20000, "成功", Convert.ToBase64String(encryptedData));
            }
            catch (Exception ex)
            {
                return new ResponceBody<string>(40000, "失败", ex.Message);
            };
        }
        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <param name="privateKey"></param>
        /// <returns></returns>
        public static ResponceBody<string> DecryptWithRSA(this string encryptedData, string privateKey)
        {
            try
            {
                byte[] privateKeyBytes = Convert.FromBase64String(privateKey);
                AsymmetricKeyParameter rsaPrivateKey = PrivateKeyFactory.CreateKey(privateKeyBytes);
                IBufferedCipher cipher = CipherUtilities.GetCipher("RSA/ECB/PKCS1Padding");
                cipher.Init(false, rsaPrivateKey);
                byte[] encryptedBytes = Convert.FromBase64String(encryptedData.Replace("%", "").Replace(",", "").Replace(" ", "+"));
                byte[] decryptedData = cipher.DoFinal(encryptedBytes);
                return new ResponceBody<string>(20000, "成功", Encoding.UTF8.GetString(decryptedData).Replace("\0", ""));
            }
            catch (Exception ex)
            {
                return new ResponceBody<string>(40000, "失败", ex.Message);
            };
        }
        /// <summary>
        /// AES加密
        /// </summary>
        /// <param name="plainText"></param>
        /// <returns></returns>
        public static string EncryptWithAES(this string plainText)
        {
            try
            {
                if (string.IsNullOrEmpty(plainText)) return null;
                Byte[] toEncryptArray = Encoding.UTF8.GetBytes(plainText);

                RijndaelManaged rm = new RijndaelManaged
                {
                    Key = Encoding.UTF8.GetBytes("1234567890123abc"),
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };

                ICryptoTransform cTransform = rm.CreateEncryptor();
                Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
                return Convert.ToBase64String(resultArray);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        /// <summary>
        /// AES解密
        /// </summary>
        /// <param name="cipherText"></param>
        /// <returns></returns>
        public static string DecryptWithAES(this string cipherText)
        {
            try
            {
                if (string.IsNullOrEmpty(cipherText)) return null;
                cipherText = cipherText.Replace("%", "").Replace(",", "").Replace(" ", "+");
                Byte[] toEncryptArray = Convert.FromBase64String(cipherText);

                RijndaelManaged rm = new RijndaelManaged
                {
                    Key = Encoding.UTF8.GetBytes("1234567890123abc"),
                    Mode = CipherMode.ECB,
                    Padding = PaddingMode.PKCS7
                };

                ICryptoTransform cTransform = rm.CreateDecryptor();
                Byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

                return Encoding.UTF8.GetString(resultArray);
            }
            catch
            {
                return "";
            }
        }
        /// <summary>
        /// 创建Token
        /// </summary>
        /// <param name="productTokenData"></param>
        /// <param name="WebApplicationPrivateKey"></param>
        /// <returns></returns>
        public static ResponceBody<string> CreateJwtToken(this Dictionary<string, string> productTokenData, string publicKey)
        {
            try
            {
                //定义需要使用到的Claims
                List<Claim> Claims = new List<Claim>();
                foreach (var data in productTokenData)
                {
                    Claims.Add(new Claim(data.Key, data.Value.EncryptWithRSA(publicKey).Data));
                }
                if (!Claims.Select(s => s.Type).ToList().Contains(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti))
                {
                    Claims.Add(new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")));
                }
                // 2. 从 appsettings.json 中读取SecretKey
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(LingYanRuntimeManager.JwtModel.SecretKey));
                // 3. 选择加密算法
                var algorithm = SecurityAlgorithms.HmacSha256;
                // 4. 生成Credentials(凭证
                var signingCredentials = new SigningCredentials(secretKey, algorithm);
                // 5. 根据以上，生成token
                var jwtSecurityToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                  issuer: LingYanRuntimeManager.JwtModel.Issuer,
                   audience: LingYanRuntimeManager.JwtModel.Audience,
                    claims: Claims,
                    notBefore: DateTime.Now,
                    expires: DateTime.Now.AddDays(LingYanRuntimeManager.JwtModel.Expres),
                    signingCredentials: signingCredentials
                );
                // 6. 将token变为string
                var token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                return new ResponceBody<string>(20000, "成功", "Bearer " + token);
            }
            catch (Exception ex)
            {
                return new ResponceBody<string>(40000, "失败", ex.Message);
            }
        }
    }
}

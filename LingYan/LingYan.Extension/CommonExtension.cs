﻿using LingYan.Model;
using LingYan.Model.BodyModel;
using LingYan.Model.RouteModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Runtime.Loader;
using System.Security.Claims;
using System.Text;

namespace LingYan.Extension
{
    public static class CommonExtension
    {
        public static void ConsoleLogger(this ConsoleColor color,string str)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(str);
            Console.ResetColor(); 
        }
        public static ILogger ResolveFilterLogger(this AuthorizationFilterContext authorizationFilterContext)
        {
            var controllerType = (authorizationFilterContext.ActionDescriptor as ControllerActionDescriptor).ControllerTypeInfo;
            Type loggerType = typeof(ILogger<>).MakeGenericType(controllerType);
            var actionLogger = (ILogger)authorizationFilterContext.HttpContext.RequestServices.GetService(loggerType);
            return actionLogger;
        }
        public static ILogger ResolveFilterLogger(this ActionExecutedContext actionExecutedContext)
        {
            Type loggerType = typeof(ILogger<>).MakeGenericType(actionExecutedContext.Controller.GetType());
            var actionLogger = (ILogger)actionExecutedContext.HttpContext.RequestServices.GetService(loggerType);
            return actionLogger;
        }
        public static ILogger ResolveFilterLogger(this ActionExecutingContext actionExecutingContext)
        {
            Type loggerType = typeof(ILogger<>).MakeGenericType(actionExecutingContext.Controller.GetType());
            var actionLogger = (ILogger)actionExecutingContext.HttpContext.RequestServices.GetService(loggerType);
            return actionLogger;
        }       

        public static string GetLocalPath(this string path, params string[] paths)
        {
            var targetPath = Path.Combine(paths);
            var completePath = Path.Combine(path, targetPath);
            if (!Directory.Exists(completePath))
            {
                Directory.CreateDirectory(completePath);
            }
            return completePath;
        }
        public static string GetLocalUrl(this string path, string fileName)
        {
            var completePath = Path.Combine(path, fileName);
            return completePath;
        }
        public static async Task<string> SaveLocalFileAsync(this string filePath, IFormFile formFile)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(fileStream);
            }
            return filePath;
        }

        public static string GetWebPath(this string path, params string[] paths)
        {
            foreach (var item in paths)
            {
                path += "/" + item;
            }
            return path;
        }
        public static string GetWebUrl(this string path, string fileName)
        {
            var url = path + "/" + fileName;
            return url;
        }
        /// <summary>
        /// 获取项目所有路由
        /// </summary>
        /// <param name="provider"></param>
        /// <returns></returns>
        [Obsolete("这是旧的http接口获取，最新请看GetProjectRouteHttpApi")]
        public static List<string> GetProjectRouteApi(this object provider)
        {
            List<string> RouteApis = new List<string>();
            //获取项目
            var compilationLibrary = DependencyContext.Default.CompileLibraries
            .Where(x => !x.Serviceable && x.Type != "package" && x.Type == "project");
            //遍历项目获取路由
            foreach (var _compilation in compilationLibrary)
            {
                //所有控制器
                var AllControllers = AssemblyLoadContext.Default.LoadFromAssemblyName(new AssemblyName(_compilation.Name))
                    .GetTypes().Where(x => typeof(ControllerBase).IsAssignableFrom(x) && !x.IsAbstract && !x.IsInterface).ToList();
                foreach (var indexRouteValue in AllControllers)
                {
                    //获取路由模版
                    var routeTemplate = "/" + indexRouteValue.GetCustomAttribute(typeof(RouteAttribute), true).GetPropertyValue<string>("Template");
                    //替换控制器
                    routeTemplate = routeTemplate.Replace("[controller]", indexRouteValue.Name.Replace("Controller", ""));
                    //替换方法
                    indexRouteValue.GetMethods()
                       .Where(method => method.IsPublic && !method.IsVirtual && !method.IsAbstract && method.Module == indexRouteValue.Module)
                       .ToList().ForEach(method =>
                       {
                           var restfulApi = routeTemplate.Replace("[action]", method.Name);
                           RouteApis.Add(restfulApi);
                       });
                }
            }
            return RouteApis;
        }
        public static List<LYRouteModel> GetProjectRouteHttpApi(this IActionDescriptorCollectionProvider actionDescriptorCollectionProvider)
        {
            var routes = actionDescriptorCollectionProvider.ActionDescriptors.Items.Select(x =>
            {
                var lyRouteModel = new LYRouteModel();
                lyRouteModel.ActionName = x.RouteValues["Action"];
                lyRouteModel.ControllerName = x.RouteValues["Controller"];
                lyRouteModel.HttpMethod = x.ActionConstraints?.OfType<HttpMethodActionConstraint>().FirstOrDefault()?.HttpMethods.First();
                lyRouteModel.TemplatePath = "/" + x.AttributeRouteInfo.Template;
                lyRouteModel.PrefixName = lyRouteModel.TemplatePath.Replace(lyRouteModel.ActionName, "").Replace(lyRouteModel.ControllerName, "").Replace("/", "");
                return lyRouteModel;
            }).ToList();
            return routes;
        }
        public static List<LYRouteModel> GetProjectRouteHttpApi(this IServiceProvider provider)
        {
            var actionDescriptorCollectionProvider = provider.GetService<IActionDescriptorCollectionProvider>();
            var routes = actionDescriptorCollectionProvider.GetProjectRouteHttpApi();
            return routes;
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
                var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(LYExpose.JwtModel.SecretKey));
                // 3. 选择加密算法
                var algorithm = SecurityAlgorithms.HmacSha256;
                // 4. 生成Credentials(凭证
                var signingCredentials = new SigningCredentials(secretKey, algorithm);
                // 5. 根据以上，生成token
                var jwtSecurityToken = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                  issuer: LYExpose.JwtModel.Issuer,
                   audience: LYExpose.JwtModel.Audience,
                    claims: Claims,
                    notBefore: DateTime.Now,
                    expires: DateTime.Now.AddDays(LYExpose.JwtModel.Expres),
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
        /// 验证座机
        /// </summary>
        /// <param name="str_telephone"></param>
        /// <returns></returns>
        public static bool IsTelephone(this string str_telephone)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_telephone, @"^(\d{3,4}-)?\d{6,8}$");
        }
        /// <summary>
        /// 验证手机
        /// </summary>
        /// <param name="str_handset"></param>
        /// <returns></returns>
        public static bool IsHandset(this string str_handset)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_handset, @"^1[3456789]\d{9}$");
        }
        /// <summary>
        /// 验证身份证
        /// </summary>
        /// <param name="str_idcard"></param>
        /// <returns></returns>
        public static bool IsIDcard(this string str_idcard)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_idcard, @"(^\d{18}$)|(^\d{15}$)");
        }
        /// <summary>
        /// 是否只包含数字
        /// </summary>
        /// <param name="str_number"></param>
        /// <returns></returns>
        public static bool IsNumber(this string str_number)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_number, @"^[0-9]*$");
        }
        /// <summary>
        /// 邮政编码
        /// </summary>
        /// <param name="str_postalcode"></param>
        /// <returns></returns>
        public static bool IsPostalcode(this string str_postalcode)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_postalcode, @"^\d{6}$");
        }
        /// <summary>
        /// 验证是否是6位的数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsSixDigitNumber(this string str)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str, @"^\d{6}$");
        }
        /// <summary>
        /// 验证密码长度是否大于6位
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool IsPasswordValid(this string password)
        {
            return password.Length >= 6;
        }
    }
}

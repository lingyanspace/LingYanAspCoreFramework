using System.Text.RegularExpressions;

namespace LingYanAspCoreFramework.Helpers
{
    public class ValidateHelper
    {
        /// <summary>
        /// 验证座机
        /// </summary>
        /// <param name="str_telephone"></param>
        /// <returns></returns>
        public static bool IsTelephone(string str_telephone)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_telephone, @"^(\d{3,4}-)?\d{6,8}$");
        }
        /// <summary>
        /// 验证手机
        /// </summary>
        /// <param name="str_handset"></param>
        /// <returns></returns>
        public static bool IsHandset(string str_handset)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_handset, @"^1[3456789]\d{9}$");
        }
        /// <summary>
        /// 验证身份证
        /// </summary>
        /// <param name="str_idcard"></param>
        /// <returns></returns>
        public static bool IsIDcard(string str_idcard)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_idcard, @"(^\d{18}$)|(^\d{15}$)");
        }
        /// <summary>
        /// 是否只包含数字
        /// </summary>
        /// <param name="str_number"></param>
        /// <returns></returns>
        public static bool IsNumber(string str_number)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_number, @"^[0-9]*$");
        }
        /// <summary>
        /// 邮政编码
        /// </summary>
        /// <param name="str_postalcode"></param>
        /// <returns></returns>
        public static bool IsPostalcode(string str_postalcode)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str_postalcode, @"^\d{6}$");
        }
        /// <summary>
        /// 验证是否是6位的数字
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool IsSixDigitNumber(string str)
        {
            return System.Text.RegularExpressions.Regex.IsMatch(str, @"^\d{6}$");
        }
        /// <summary>
        /// 验证密码长度是否大于6位
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool IsPasswordValid(string password)
        {
            return password.Length >= 6;
        }
        /// <summary>
        /// 验证是否是合法的URL
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static bool IsValidRegexUrl(string url)
        {
            string pattern = @"^(http|https)://[\w-]+(\.[\w-]+)*(:[\d]+)?([\w.,@?^=%&:/~+#-]*[\w@?^=%&/~+#-])?$";
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            return regex.IsMatch(url);
        }
    }
}

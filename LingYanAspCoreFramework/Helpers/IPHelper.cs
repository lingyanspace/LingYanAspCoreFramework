using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace LingYanAspCoreFramework.Helpers
{
    public class IPHelper
    {
        /// <summary>
        /// 获取本机内网IPv4地址
        /// </summary>
        /// <returns></returns>
        public static string GetInternalIPv4Address()
        {
            // 获取所有网络接口
            var host = Dns.GetHostEntry(Dns.GetHostName());

            // 查找第一个内网 IPv4 地址
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    // 检查是否是内网地址
                    if (IsInternalIP(ip))
                    {
                        return ip.ToString();
                    }
                }
            }
            return null;
        }
        private static bool IsInternalIP(IPAddress ip)
        {
            byte[] addressBytes = ip.GetAddressBytes();
            // 检查是否是私有地址
            if (addressBytes[0] == 10 ||
                (addressBytes[0] == 172 && addressBytes[1] >= 16 && addressBytes[1] <= 31) ||
                (addressBytes[0] == 192 && addressBytes[1] == 168))
            {
                return true;
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LingYanAspCoreFramework.Helpers
{
    public class RandomHelper
    {
        private static readonly Random _random = new Random();

        // 生成一个随机的整数
        public static int Next()
        {
            return _random.Next();
        }

        // 生成一个指定范围内的随机整数（包含最小值，不包含最大值）
        public static int Next(int minValue, int maxValue)
        {
            return _random.Next(minValue, maxValue);
        }

        // 生成一个指定最大值的随机整数（不包含最大值）
        public static int Next(int maxValue)
        {
            return _random.Next(maxValue);
        }

        // 生成一个随机的双精度浮点数
        public static double NextDouble()
        {
            return _random.NextDouble();
        }

        // 生成一个随机的布尔值
        public static bool NextBoolean()
        {
            return _random.Next(2) == 0;
        }

        // 生成一个随机的字节数组
        public static byte[] NextBytes(int length)
        {
            byte[] buffer = new byte[length];
            _random.NextBytes(buffer);
            return buffer;
        }

        // 生成一个随机的字符串（仅包含字母和数字）
        public static string NextString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] result = new char[length];
            for (int i = 0; i < length; i++)
            {
                result[i] = chars[_random.Next(chars.Length)];
            }
            return new string(result);
        }
    }
}

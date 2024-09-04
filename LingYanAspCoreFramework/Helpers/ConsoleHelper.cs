namespace LingYanAspCoreFramework.Helpers
{
    public class ConsoleHelper
    {
        /// <summary>
        /// 成功
        /// </summary>
        /// <param name="str"></param>
        public static void SuccessLog(string str)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.WriteLine(str);
            Console.WriteLine();
            Console.ResetColor();
        }
        /// <summary>
        /// 失败
        /// </summary>
        /// <param name="str"></param>
        public static void ErrorLog(string str)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(str);
            Console.WriteLine();
            Console.ResetColor();
        }
        /// <summary>
        /// 警告
        /// </summary>
        /// <param name="str"></param>
        public static void WarnrningLog(string str)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(str);
            Console.WriteLine();
            Console.ResetColor();
        }
        /// <summary>
        /// 默认
        /// </summary>
        /// <param name="str"></param>
        public static void DefaultLog(string str)
        {
            Console.ForegroundColor = ConsoleColor.DarkBlue;
            Console.WriteLine(str);
            Console.WriteLine();
            Console.ResetColor();
        }
    }
}

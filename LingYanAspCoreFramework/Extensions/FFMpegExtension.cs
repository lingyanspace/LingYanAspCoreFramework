using LingYanAspCoreFramework.Models;
using LingYanAspCoreFramework.Models.FFMpeg;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace LingYanAspCoreFramework.Extensions
{
    public static class FFMpegExtension
    {
        public static string FFmpegLibPath = "";
        //-ss 指定时间格式-ss 00:00:03
        //-i 指定文件参数
        //-vframes​ 指定处理视频帧数。-vframes 1​ 表示只处理一个视频帧
        //ffmpeg -ss 00:00:03 -i input.mp4 output.mp4
        //​ffmpeg -ss 00:00:03 -i input.mp4 -vframes 5 output.mp4
        //ffmpeg -ss 00:00:03 -i input.mp4 -vframes 1 output1.jpg
        public static ResponceBody<FFMpegRoot> GetVideoInfomation(this string videopath)
        {
            try
            {
                string successOutput = "";
                string errorOutput = "";
                Console.WriteLine($"执行ffmprobe");
                using (Process ffprobeProcess = new Process())
                {
                    ffprobeProcess.StartInfo.FileName = GetSystemRuntimeFFmprobe();
                    if (!ffprobeProcess.StartInfo.FileName.EndsWith(".exe"))
                    {
                        ffprobeProcess.StartInfo.EnvironmentVariables["LD_LIBRARY_PATH"] = "/usr/local/ffmpeg/lib";
                    }
                    Console.WriteLine($"文件路径:{ffprobeProcess.StartInfo.FileName}");
                    ffprobeProcess.StartInfo.RedirectStandardOutput = true;
                    ffprobeProcess.StartInfo.UseShellExecute = false;
                    ffprobeProcess.StartInfo.CreateNoWindow = true;
                    ffprobeProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                    ffprobeProcess.StartInfo.RedirectStandardError = true;
                    ffprobeProcess.StartInfo.RedirectStandardInput = true;
                    ffprobeProcess.StartInfo.RedirectStandardOutput = true;
                    ffprobeProcess.StartInfo.Arguments = $" -v quiet -print_format json -show_format -show_streams {videopath}";
                    Console.WriteLine($"命令参数:{ffprobeProcess.StartInfo.Arguments}");
                    // 启动进程
                    var started = ffprobeProcess.Start();
                    Thread.Sleep(100);
                    if (!started)
                    {
                        Console.WriteLine($"ffmprobe未启动");
                    }
                    // 等待进程退出
                    bool exited = ffprobeProcess.WaitForExit(1000); // 增加等待时间到10000毫秒（10秒）
                    if (!exited)
                    {
                        Console.WriteLine($"ffmprobe未结束");
                    }
                    // 读取ffprobe的标准输出流
                    StreamReader outputReader = new StreamReader(ffprobeProcess.StandardOutput.BaseStream, Encoding.UTF8);
                    // 读取ffprobe的标准错误输出流
                    StreamReader errorReader = new StreamReader(ffprobeProcess.StandardError.BaseStream, Encoding.UTF8);
                    // 等待ffprobe进程结束直到最多1000毫秒（即1秒）
                    ffprobeProcess.WaitForExit(1000);
                    // 从输出流中读取所有输出内容
                    successOutput = outputReader.ReadToEnd();
                    // 从错误流中读取所有错误内容
                    errorOutput = errorReader.ReadToEnd();
                    Console.WriteLine($"成功日志:{successOutput}");
                    Console.WriteLine($"错误日志:{errorOutput}");
                }
                //解析JSON输出
                var videoInfo = Newtonsoft.Json.JsonConvert.DeserializeObject<FFMpegRoot>(successOutput);
                if (videoInfo == null)
                {
                    return new ResponceBody<FFMpegRoot>(40000, "解析不成功", null);
                }
                return new ResponceBody<FFMpegRoot>(obj: videoInfo);

            }
            catch (Exception ex)
            {
                return new ResponceBody<FFMpegRoot>(40000, ex.Message, null);
            }

        }
        public static String GetSystemRuntimeFFmpeg()
        {
            string ffmpegPath = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                ffmpegPath = FFmpegLibPath.GetLocalPath(LingYanRuntimeManager.CommonConfigModel.FFMpegLinuxConfig).GetLocalUrl("ffmpeg");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ffmpegPath = FFmpegLibPath.GetLocalPath(LingYanRuntimeManager.CommonConfigModel.FFMpegWinConfig).GetLocalUrl("ffmpeg.exe");
            }
            return ffmpegPath;
        }
        public static String GetSystemRuntimeFFmprobe()
        {
            string ffmpegPath = "";
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                //ffmpegPath = "/usr/local/ffmpeg/ffmpeg";
                ffmpegPath = FFmpegLibPath.GetLocalPath(LingYanRuntimeManager.CommonConfigModel.FFMpegLinuxConfig).GetLocalUrl("ffprobe");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                ffmpegPath = FFmpegLibPath.GetLocalPath(LingYanRuntimeManager.CommonConfigModel.FFMpegWinConfig).GetLocalUrl("ffprobe.exe");
            }
            return ffmpegPath;

        }
        /// <summary>
        /// 从视频地址返回首页图
        /// </summary>
        /// <param name="videoFilePath"></param>
        /// <returns></returns>
        /// <exception cref="FileNotFoundException"></exception>
        public static ResponceBody<string> GetImageSourceFromVideo(this string videoFilePath, string thumbPath)
        {
            try
            {
                string successOutput = "";
                string errorOutput = "";
                // 检查视频文件是否存在
                if (!File.Exists(videoFilePath))
                {
                    return new ResponceBody<string>(40000, "视频文件不存在", null);
                }
                Console.WriteLine($"执行ffmpeg");
                using (var process = new Process())
                {
                    // 设置启动信息
                    process.StartInfo.FileName = GetSystemRuntimeFFmpeg();
                    if (!process.StartInfo.FileName.EndsWith(".exe"))
                    {
                        process.StartInfo.EnvironmentVariables["LD_LIBRARY_PATH"] = "/usr/local/ffmpeg/lib";
                    }                  
                    Console.WriteLine($"文件路径:{process.StartInfo.FileName}");
                    process.StartInfo.Arguments = $"-v quiet -ss 00:00:03 -i {videoFilePath} -frames:v 1 {thumbPath}";
                    Console.WriteLine($"命令参数:{process.StartInfo.Arguments}");
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardError = true;
                    process.StartInfo.RedirectStandardInput = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    // 启动进程
                    var started = process.Start();
                    Thread.Sleep(100);
                    if (!started)
                    {
                        Console.WriteLine($"ffmpeg未启动");
                    }
                    // 等待进程退出
                    bool exited = process.WaitForExit(1000); // 增加等待时间到10000毫秒（10秒）
                    if (!exited)
                    {
                        Console.WriteLine($"ffmpeg未结束");
                    }
                    //读取ffprobe的标准输出流
                    StreamReader outputReader = new StreamReader(process.StandardOutput.BaseStream, Encoding.UTF8);
                    //读取ffprobe的标准错误输出流
                    StreamReader errorReader = new StreamReader(process.StandardError.BaseStream, Encoding.UTF8);
                    //从输出流中读取所有输出内容
                    successOutput = outputReader.ReadToEnd();
                    // 从错误流中读取所有错误内容
                    errorOutput = errorReader.ReadToEnd();
                    Console.WriteLine($"成功日志:{successOutput}");
                    Console.WriteLine($"错误日志:{errorOutput}");
                }
                // 检查视频文件是否存在
                if (!File.Exists(thumbPath))
                {
                    return new ResponceBody<string>(40000, "缩略图未生成", null);
                }
                return new ResponceBody<string>(obj: thumbPath);
            }
            catch (Exception ex)
            {
                return new ResponceBody<string>(40000, ex.Message, null);
            }
        }
    }
}

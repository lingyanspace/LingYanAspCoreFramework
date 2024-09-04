using Microsoft.AspNetCore.Http;

namespace LingYanAspCoreFramework.Extensions
{
    public static class PathExtension
    {
        /// <summary>
        /// 获取本地过程路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
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
        /// <summary>
        /// 获取本地文件地址
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetLocalUrl(this string path, string fileName)
        {
            var completePath = Path.Combine(path, fileName);
            return completePath;
        }
        /// <summary>
        /// 保存本地文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="formFile"></param>
        /// <returns></returns>
        public static async Task<string> SaveLocalFileAsync(this string filePath, IFormFile formFile)
        {
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await formFile.CopyToAsync(fileStream);
            }
            return filePath;
        }
        /// <summary>
        /// 获取网络过程路径
        /// </summary>
        /// <param name="path"></param>
        /// <param name="paths"></param>
        /// <returns></returns>
        public static string GetWebPath(this string path, params string[] paths)
        {
            foreach (var item in paths)
            {
                path += "/" + item;
            }
            return path;
        }
        /// <summary>
        /// 获取网络文件地址
        /// </summary>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetWebUrl(this string path, string fileName)
        {
            var url = path + "/" + fileName;
            return url;
        }
    }
}

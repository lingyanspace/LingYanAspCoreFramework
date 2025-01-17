﻿namespace LingYan.Model.DynamicHttpMethodModel
{
    public class HttpMethodConfigure
    {
        /// 方法键
        /// </summary>
        public string MethodKey { get; set; }
        /// <summary>
        /// 方法类型名
        /// </summary>
        public List<string> MethodVal { get; set; }
        public HttpMethodConfigure()
        {
            this.MethodVal = new List<string>();
            this.MethodKey = "Get";
            this.MethodVal.Add("GET");
            this.MethodVal.Add("QUERY");
        }
    }
}

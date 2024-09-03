namespace LingYanAspCoreFramework.Models 
{
    public class ResponceBody
    {
        public int Code { get; set; }
        public string? Message { get; set; }
        public object? Data { get; set; }
        public ResponceBody(int code = 20000, string message = "成功", object obj = null)
        {
            this.Code = code;
            this.Message = message;
            this.Data = obj;
        }
    }
    public class ResponceBody<T>
    {
        public int Code { get; set; }
        public string? Message { get; set; }
        public T? Data { get; set; }
        public ResponceBody(int code = 20000, string message = "成功", T obj = default)
        {
            this.Code = code;
            this.Message = message;
            this.Data = obj;
        }
    }
}

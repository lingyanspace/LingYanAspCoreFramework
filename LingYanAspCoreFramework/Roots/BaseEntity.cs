namespace LingYanAspCoreFramework.Roots
{
    public class BaseEntity
    {
        public long Id { get; set; }
        //10新建、20删除
        public bool IsDeleted { get; set; }
        public long CreateTimeStamp { get; set; }  
        public BaseEntity()
        {
            this.Id = YitIdHelper.NextId();
            this.IsDeleted = false;
            this.CreateTimeStamp = new DateTimeOffset(DateTime.Now).ToUnixTimeMilliseconds();
        }
    }
}

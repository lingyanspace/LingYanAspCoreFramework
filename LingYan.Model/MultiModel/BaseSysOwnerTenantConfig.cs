namespace LingYan.Model.MultiModel
{
    public class BaseSysOwnerTenantConfig<Tid>
    {
        public virtual Tid Id { get; set; }
        //企业关联字段
        public virtual long CompanyId { get; set; }
        //数据库连接字符串
        public virtual string ConfigJson { get; set; }
        public virtual DateTime CreationTime { get; set; }
        public virtual bool IsDeleted { get; set; }
    }
}

namespace LingYanAspCoreFramework.Roots
{
    public class BaseSysOwner<Tid>
    {
        public virtual Tid Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string Password { get; set; }
        public virtual DateTime CreationTime { get; set; }
        public virtual bool IsDeleted { get; set; }
    }
}

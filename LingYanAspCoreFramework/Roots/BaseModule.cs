namespace LingYanAspCoreFramework.Roots
{
    public abstract class BaseModule<TCollection, TProvider>
          where TCollection : class
          where TProvider : class
    {
        public virtual int PageIndex { get; set; }
        public abstract void ARegisterModule(TCollection services);
        public abstract void BInitializationModule(TProvider provider);
    }
    public abstract class BaseModule<TCollection, TConfiguration, TProvider>
       where TCollection : class
        where TConfiguration : class
       where TProvider : class
    { 
        public virtual int PageIndex { get; set; }
        public abstract void ARegisterModule(TCollection services, TConfiguration configuration);
        public abstract void BInitializationModule(TProvider provider);
    }
}

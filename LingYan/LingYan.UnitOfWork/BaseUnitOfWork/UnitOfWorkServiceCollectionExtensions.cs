using LingYan.UnitOfWork.BaseIUnitOfWork;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LingYan.UnitOfWork.BaseUnitOfWork
{
    public static class UnitOfWorkServiceCollectionExtensions
    {
        public static IServiceCollection AddUnitOfWork<TContext>(this IServiceCollection services) where TContext : DbContext
        {
            services.AddScoped<IUnitOfWork<TContext>, UnitOfWork<TContext>>();
            return services;
        }
        public static IServiceCollection AddUnitOfWorks(this IServiceCollection services, params Type[] types)
        {
            foreach (var item in types)
            {
                if (item.IsSubclassOf(typeof(DbContext)))
                {
                    var unitOfWorkType = typeof(UnitOfWork<>).MakeGenericType(item);
                    var unitOfWorkInterfaceType = typeof(IUnitOfWork<>).MakeGenericType(item);
                    services.AddScoped(unitOfWorkInterfaceType, unitOfWorkType);
                }
            }
            return services;
        }

        public static IServiceCollection AddRepository<TContext, TEntity>(this IServiceCollection services)
            where TContext : DbContext
            where TEntity : class
        {
            services.AddScoped<IRepository<TEntity>, Repository<TContext, TEntity>>();
            return services;
        }
        public static void AddRepositorys(this IServiceCollection services, Type TContext, params Type[] entityTypes)
        {
            foreach (var entityType in entityTypes)
            {
                Type repositoryType = typeof(Repository<,>).MakeGenericType(TContext, entityType);
                Type repositoryInterfaceType = typeof(IRepository<>).MakeGenericType(entityType);
                services.AddScoped(repositoryInterfaceType, repositoryType);
            }
        }
    }
}

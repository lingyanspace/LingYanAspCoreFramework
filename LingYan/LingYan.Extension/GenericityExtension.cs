using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace LingYan.Extension
{
    public static class GenericityExtension
    {
        /// <summary>
        /// 反射获取属性的值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public static T GetPropertyValue<T>(this object obj, string propertyName)
        {
            PropertyInfo propertyType = obj.GetType().GetProperty(propertyName);
            var peropertyValue = propertyType.GetValue(obj);
            return (T)peropertyValue;
        }
        /// <summary>
        /// 判断泛型方法
        /// </summary>
        /// <param name="givenType"></param>
        /// <param name="genericType"></param>
        /// <returns></returns>
        public static bool IsAssignableToGenericType(this Type givenType, Type genericType)
        {
            if (givenType == null || genericType == null)
                return false;

            if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
                return true;

            foreach (var interfaceType in givenType.GetInterfaces())
            {
                if (interfaceType.IsGenericType && interfaceType.GetGenericTypeDefinition() == genericType)
                    return true;
            }
            var baseType = givenType.BaseType;
            if (baseType == null)
                return false;
            return baseType.IsAssignableToGenericType(genericType);
        }
        /// <summary>
        /// 获取实例需要传入的参数
        /// </summary>
        /// <typeparam name="TWebApplicationBuilder"></typeparam>
        /// <param name="builder"></param>
        /// <param name="parameterInfos"></param>
        /// <returns></returns>
        public static object[] GetInstanceParameters<TWebApplicationBuilder>(this TWebApplicationBuilder builder, ParameterInfo[] parameterInfos) where TWebApplicationBuilder : class
        {
            object[] objects = new object[parameterInfos.Length];
            for (int i = 0; i < objects.Length; i++)
            {
                if (parameterInfos[i].ParameterType == builder.GetType())
                {
                    objects[i] = builder;
                }
                else
                {
                    var matchingProperties = builder.GetType().GetProperties()
                        .Where(f => f.PropertyType == parameterInfos[i].ParameterType || parameterInfos[i].ParameterType.IsAssignableFrom(f.PropertyType));
                    if (matchingProperties?.Count() > 0)
                    {
                        objects[i] = matchingProperties.FirstOrDefault()?.GetValue(builder);
                    }
                }
            }
            return objects;
        }
        /// <summary>
        /// 获取容器
        /// </summary>
        /// <param name="objects"></param>
        /// <returns></returns>
        public static IServiceCollection GetFirstIServiceCollection(this object[] objects)
        {
            foreach (var objs in objects)
            {
                var result = objs.GetType().GetProperties()
                    .FirstOrDefault(w => typeof(IServiceCollection).IsAssignableFrom(w.PropertyType));

                if (result != null)
                {
                    return (IServiceCollection)result.GetValue(objs);
                }
            }
            return null;
        }
        /// <summary>
        /// 获取Builder的容器
        /// </summary>
        /// <typeparam name="Builder"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IServiceCollection GetBuilderServiceCollection<Builder>(this Builder builder) where Builder : class
        {
            if (builder.GetType() == typeof(IServiceCollection))
            {
                return (IServiceCollection)builder;
            }
            else
            {
                var matchingProperties = builder.GetType().GetProperties().Where(f => f.PropertyType == typeof(IServiceCollection));
                return (IServiceCollection)matchingProperties.FirstOrDefault()?.GetValue(builder);
            }
        }
        /// <summary>
        /// 获取TWeb的容器管理员
        /// </summary>
        /// <typeparam name="TApp"></typeparam>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IServiceProvider GetBuilderServiceProvider<TApp>(this TApp app) where TApp : class
        {
            if (app.GetType() == typeof(IServiceProvider))
            {
                return (IServiceProvider)app;
            }
            else
            {
                var matchingProperties = app.GetType().GetProperties().Where(f => f.PropertyType == typeof(IServiceProvider));
                return (IServiceProvider)matchingProperties.FirstOrDefault()?.GetValue(app);
            }
        }
    }
}

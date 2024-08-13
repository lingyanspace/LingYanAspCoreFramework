using Castle.DynamicProxy;
using Dynamitey;
using System.ComponentModel;
using System.Data;
using System.Linq.Expressions;
using System.Reflection;

namespace LingYan.DynamicShardingDBT.DBTExtension
{
    /// <summary>
    /// 继承ExpressionVisitor类，实现参数替换统一
    /// </summary>
    class ParameterReplaceVisitor : ExpressionVisitor
    {
        public ParameterReplaceVisitor(ParameterExpression paramExpr)
        {
            _parameter = paramExpr;
        }

        //新的表达式参数
        private ParameterExpression _parameter { get; set; }

        protected override Expression VisitParameter(ParameterExpression p)
        {
            if (p.Type == _parameter.Type)
                return _parameter;
            else
                return p;
        }
    }
    public static class CommonExtension
    {
        #region 拓展And和Or方法

        /// <summary>
        /// 连接表达式与运算
        /// </summary>
        /// <typeparam name="T">参数</typeparam>
        /// <param name="one">原表达式</param>
        /// <param name="another">新的表达式</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> And<T>(this Expression<Func<T, bool>> one, Expression<Func<T, bool>> another)
        {
            //创建新参数
            var newParameter = Expression.Parameter(typeof(T), "parameter");

            var parameterReplacer = new ParameterReplaceVisitor(newParameter);
            var left = parameterReplacer.Visit(one.Body);
            var right = parameterReplacer.Visit(another.Body);
            var body = Expression.And(left, right);

            return Expression.Lambda<Func<T, bool>>(body, newParameter);
        }

        /// <summary>
        /// 连接表达式或运算
        /// </summary>
        /// <typeparam name="T">参数</typeparam>
        /// <param name="one">原表达式</param>
        /// <param name="another">新表达式</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Or<T>(this Expression<Func<T, bool>> one, Expression<Func<T, bool>> another)
        {
            //创建新参数
            var newParameter = Expression.Parameter(typeof(T), "parameter");

            var parameterReplacer = new ParameterReplaceVisitor(newParameter);
            var left = parameterReplacer.Visit(one.Body);
            var right = parameterReplacer.Visit(another.Body);
            var body = Expression.Or(left, right);

            return Expression.Lambda<Func<T, bool>>(body, newParameter);
        }

        #endregion


        /// <summary>
        /// 给IEnumerable拓展ForEach方法
        /// </summary>
        /// <typeparam name="T">模型类</typeparam>
        /// <param name="iEnumberable">数据源</param>
        /// <param name="func">方法</param>
        public static void ForEach<T>(this IEnumerable<T> iEnumberable, Action<T> func)
        {
            foreach (var item in iEnumberable)
            {
                func(item);
            }
        }

        /// <summary>
        /// 将IEnumerable'T'转为对应的DataTable
        /// </summary>
        /// <typeparam name="T">数据模型</typeparam>
        /// <param name="iEnumberable">数据源</param>
        /// <returns>DataTable</returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> iEnumberable)
        {
            PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(typeof(T));
            DataTable table = new DataTable();
            foreach (PropertyDescriptor prop in properties)
                table.Columns.Add(prop.Name, Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType);
            foreach (T item in iEnumberable)
            {
                DataRow row = table.NewRow();
                foreach (PropertyDescriptor prop in properties)
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                table.Rows.Add(row);
            }
            return table;
        }

        /// <summary>
        /// 获取稳定的HashCode（原HashCode不稳定，会改变）
        /// </summary>
        /// <param name="str">字符串</param>
        /// <returns></returns>
        public static int GetStableHashCode(this string str)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i + 1] == '\0')
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
        private static readonly BindingFlags _bindingFlags
            = BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static;
        private static readonly ProxyGenerator Generator = new ProxyGenerator();

        /// <summary>
        /// 判断是否为Null或者空
        /// </summary>
        /// <param name="obj">对象</param>
        /// <returns></returns>
        public static bool IsNullOrEmpty(this object obj)
        {
            if (obj == null)
                return true;
            else
            {
                string objStr = obj.ToString();
                return string.IsNullOrEmpty(objStr);
            }
        }

        /// <summary>
        /// 获取某属性值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="propertyName">属性名</param>
        /// <returns></returns>
        public static object GetPropertyValue(this object obj, string propertyName)
        {
            var property = obj.GetType().GetProperty(propertyName, _bindingFlags);
            if (property != null)
            {
                return obj.GetType().GetProperty(propertyName, _bindingFlags).GetValue(obj);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 设置某属性值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="propertyName">属性名</param>
        /// <param name="value">属性值</param>
        /// <returns></returns>
        public static void SetPropertyValue(this object obj, string propertyName, object value)
        {
            obj.GetType().GetProperty(propertyName, _bindingFlags).SetValue(obj, value);
        }

        /// <summary>
        /// 获取某字段值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">字段名</param>
        /// <returns></returns>
        public static object GetFieldValue(this object obj, string fieldName) 
        {
            return obj.GetType().GetField(fieldName, _bindingFlags).GetValue(obj);
        }

        /// <summary>
        /// 设置某字段值
        /// </summary>
        /// <param name="obj">对象</param>
        /// <param name="fieldName">字段名</param>
        /// <param name="value">值</param>
        /// <returns></returns>
        public static void SetFieldValue(this object obj, string fieldName, object value)
        {
            obj.GetType().GetField(fieldName, _bindingFlags).SetValue(obj, value);
        }

        /// <summary>
        /// 改变类型
        /// </summary>
        /// <param name="obj">原对象</param>
        /// <param name="targetType">目标类型</param>
        /// <returns></returns>
        public static object ChangeType_ByConvert(this object obj, Type targetType)
        {
            object resObj;
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                NullableConverter newNullableConverter = new NullableConverter(targetType);
                resObj = newNullableConverter.ConvertFrom(obj);
            }
            else
            {
                resObj = Convert.ChangeType(obj, targetType);
            }

            return resObj;
        }

        /// <summary>
        /// 生成代理
        /// </summary>
        /// <typeparam name="T">代理类型</typeparam>
        /// <param name="obj">实际类型</param>
        /// <returns></returns>
        public static T ActLike<T>(this object obj) where T : class
        {
            return Generator.CreateInterfaceProxyWithoutTarget<T>(new ActLikeInterceptor(obj));
        }

        private class ActLikeInterceptor : IInterceptor
        {
            public ActLikeInterceptor(object obj)
            {
                _obj = obj;
            }
            private object _obj;
            public void Intercept(IInvocation invocation)
            {
                var method = invocation.Method;

                //属性处理
                if (method.Name.StartsWith("get_"))
                {
                    invocation.ReturnValue = Dynamic.InvokeGet(_obj, method.Name.Substring(4));

                    return;
                }
                else if (method.Name.StartsWith("set_"))
                {
                    Dynamic.InvokeSet(_obj, method.Name.Substring(4), invocation.Arguments[0]);

                    return;
                }

                //方法处理
                var name = new InvokeMemberName(method.Name, method.GetGenericArguments());
                if (invocation.Method.ReturnType != typeof(void))
                {
                    invocation.ReturnValue = Dynamic.InvokeMember(_obj, name, invocation.Arguments);
                }
                else
                {
                    Dynamic.InvokeMemberAction(_obj, name, invocation.Arguments);
                }
            }
        }
    }
}

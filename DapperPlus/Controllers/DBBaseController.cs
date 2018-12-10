using DapperPlus.Attributes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DapperPlus.Controllers
{
    /// <summary>
    /// 数据库操作工厂
    /// </summary>
    public abstract class DBBaseController
    {
        /// <summary>
        /// 添加一个实体
        /// </summary>
        /// <returns>受影响行数</returns>
        public virtual int Add<T>(IDbConnection conn, T t, IDbTransaction trans = null) where T : class
        {
            var listFieldName = new List<string>();//列名集合
            var listFieldValue = new List<string>();//列值集合
            var type = t.GetType();
            var tableName = GetTableName(type); //获取表名
            foreach (PropertyInfo item in type.GetProperties())//遍历类的公共属性
            {
                if (!IsExtraElement(item) && !IsIdentity(item) && !IsDateTimeAndNull(item, t)) //不是额外元素不是自增列并且不是空DateTime
                {
                    listFieldName.Add(item.Name);
                    listFieldValue.Add("@" + item.Name);
                }
            }
            //拼接SQL语句
            var sql = $"INSERT INTO {tableName} ({string.Join(",", listFieldName)})VALUES({string.Join(",", listFieldValue)})";
            return conn.Execute(sql, t, trans);
        }
        /// <summary>
        /// 添加多个实体
        /// </summary>
        /// <returns>受影响行数</returns>
        public virtual int AddList<T>(IDbConnection conn, IEnumerable<T> listT, IDbTransaction trans = null) where T : class
        {
            var count = 0;
            foreach (var item in listT)
            {
                count += Add(conn, item, trans);
            }
            return count;
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="param">WHERE条件 eg: new { Name="张三" }</param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public virtual int Delete<T>(IDbConnection conn, object param = null, IDbTransaction trans = null) where T : class
        {
            var type = typeof(T);
            var tableName = GetTableName(type);
            var sbSql = new StringBuilder($"DELETE FROM {tableName} ");
            if (param != null)
            {
                sbSql.Append("WHERE ");
                foreach (PropertyInfo item in param.GetType().GetProperties())
                {
                    sbSql.Append($"{item.Name}=@{item.Name} AND ");
                }
                sbSql = sbSql.Remove(sbSql.Length - 4, 4);//去掉最后AND
            }
            return conn.Execute(sbSql.ToString(), param, trans);
        }
        /// <summary>
        /// 删除一个实体 必须对实体标记主键[Key]或自增标识[Identity]
        /// </summary>
        /// <returns>受影响行数</returns>
        public virtual int Delete<T>(IDbConnection conn, T t, IDbTransaction trans = null) where T : class
        {
            var listKey = new List<string>(); //主键
            var identityFieldName = string.Empty; //自增
            var type = t.GetType();
            var tableName = GetTableName(type);
            foreach (PropertyInfo item in type.GetProperties())
            {
                if (IsKey(item))
                {
                    listKey.Add(item.Name);
                }
                if (string.IsNullOrEmpty(identityFieldName) && IsIdentity(item))
                {
                    identityFieldName = item.Name;
                }
            }
            if (listKey.Count == 0 && string.IsNullOrEmpty(identityFieldName))
            {
                throw new Exception($"{type.Name}对象未标识主键或自增");
            }
            var sbSql = new StringBuilder($"DELETE FROM {tableName} WHERE ");
            if (listKey.Count > 0) //有主键时先根据主键删除
            {
                listKey.ForEach(key => { sbSql.Append($"{key}=@{key} AND "); });
                sbSql = sbSql.Remove(sbSql.Length - 4, 4);//去掉最后AND
            }
            else //根据自增删除
            {
                sbSql.Append($"{identityFieldName}=@{identityFieldName}");
            }
            return conn.Execute(sbSql.ToString(), t, trans);
        }
        /// <summary>
        /// 删除多个实体 必须对实体标记主键[Key]或自增标识[Identity]
        /// </summary>
        /// <returns>受影响行数</returns>
        public virtual int DeleteList<T>(IDbConnection conn, IEnumerable<T> listT, IDbTransaction trans = null) where T : class
        {
            var count = 0;
            foreach (var item in listT)
            {
                count += Delete(conn, item, trans);
            }
            return count;
        }
        /// <summary>
        /// 更新一个实体 必须对实体标记主键[Key]或自增标识[Identity]
        /// </summary>
        /// <returns>受影响行数</returns>
        public virtual int Update<T>(IDbConnection conn, T t, IDbTransaction trans = null) where T : class
        {
            var listKey = new List<string>(); //主键
            var identityFieldName = string.Empty; //自增
            var type = t.GetType();
            var tableName = GetTableName(type);
            var sbSql = new StringBuilder($"UPDATE {tableName} SET ");
            foreach (PropertyInfo item in type.GetProperties())//遍历类的公共属性，找主键和自增列
            {
                if (IsKey(item))
                {
                    listKey.Add(item.Name);
                }
                if (string.IsNullOrEmpty(identityFieldName) && IsIdentity(item))
                {
                    identityFieldName = item.Name;
                }
            }
            foreach (PropertyInfo item in type.GetProperties())//遍历类的公共属性，设置SET列
            {
                if (!IsExtraElement(item) && !IsIdentity(item) && !IsDateTimeAndNull(item, t)) //不是额外元素不是自增列并且不是空DateTime
                {
                    sbSql.Append($"{item.Name}=@{item.Name},");
                }
            }
            sbSql = sbSql.Remove(sbSql.Length - 1, 1);//去掉最后一个逗号
            if (listKey.Count == 0 && string.IsNullOrEmpty(identityFieldName))
            {
                throw new Exception($"{type.Name}对象未标识主键或自增");
            }
            sbSql.Append(" WHERE ");
            if (listKey.Count > 0) //有主键时先根据主键删除
            {
                listKey.ForEach(key => { sbSql.Append($"{key}=@{key} AND "); });
                sbSql = sbSql.Remove(sbSql.Length - 4, 4);//去掉最后AND
            }
            else //根据自增删除
            {
                sbSql.Append($"{identityFieldName}=@{identityFieldName}");
            }
            return conn.Execute(sbSql.ToString(), t, trans);
        }
        /// <summary>
        /// 更新多个实体 必须对实体标记主键[Key]或自增标识[Identity]
        /// </summary>
        /// <returns>受影响行数</returns>
        public virtual int UpdateList<T>(IDbConnection conn, IEnumerable<T> listT, IDbTransaction trans = null) where T : class
        {
            var count = 0;
            foreach (var item in listT)
            {
                count += Update(conn, item, trans);
            }
            return count;
        }

        #region abstract
        /// <summary>
        /// 获取一个实体 
        /// </summary>
        /// <param name="param">WHERE条件 eg: new { Name="张三" }</param>
        /// <returns>T</returns>
        public abstract T GetModel<T>(IDbConnection conn, object param = null, IDbTransaction trans = null) where T : class;
        /// <summary>
        /// 获取一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="whereParamName"></param>
        /// <param name="whereParamValue"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public abstract T GetModel<T>(IDbConnection conn, string whereParamName = null, object whereParamValue = null, IDbTransaction trans = null) where T : class;
        /// <summary>
        /// 获取实体集合
        /// </summary>
        /// <param name="param">WHERE条件 eg: new { Name="张三" }</param>
        /// <param name="funcSort">排序 eg: d=>d.CreateTime </param>
        /// <param name="isAsc">升序/降序 默认升序 eg: true </param>
        /// <param name="top">top 条数</param>
        /// <returns>IEnumerable T </returns>
        public abstract IEnumerable<T> GetList<T>(IDbConnection conn, object param = null, Expression<Func<T, object>> funcSort = null, bool isAsc = true, int? top = null, IDbTransaction trans = null);
        /// <summary>
        /// 获取实体集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="whereParamName"></param>
        /// <param name="whereParamValue"></param>
        /// <param name="funcSort"></param>
        /// <param name="isAsc"></param>
        /// <param name="top"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public abstract IEnumerable<T> GetList<T>(IDbConnection conn, string whereParamName = null, object whereParamValue = null, Expression<Func<T, object>> funcSort = null, bool isAsc = true, int? top = null, IDbTransaction trans = null);
        /// <summary>
        /// 获取数据 支持联表查询
        /// </summary>
        /// <param name="fields">要查询的字段 联表eg: A.Field1,B.Field2,...</param>
        /// <param name="tableName">表名 联表eg: TableA INNER JOIN TableB ON TableA.Id=TableB.Id</param>
        /// <param name="whereParamName">where条件 可以参数化 联表eg: TableA.Field1=@Field1 AND TableB.Field2=@Field2</param>
        /// <param name="whereParamValue">where条件中参数化的值 联表eg: new { Field1=Value1,Field2=Value2 }</param>
        /// <param name="sort">排序 联表eg: A.Field1 DESC,B.Field2,...</param>
        /// <param name="top">top 条数</param>
        /// <returns>IEnumerable T </returns>
        public abstract IEnumerable<T> GetSqlList<T>(IDbConnection conn, string fields = "*", string tableName = null, string whereParamName = null, object whereParamValue = null, string sort = null, int? top = null, IDbTransaction trans = null);
        /// <summary>
        /// 分页获取实体集合
        /// </summary>
        /// <param name="pageNumber">第几页 从1开始</param>
        /// <param name="pageSize">每页显示数</param>
        /// <param name="totalCounts">总条数</param>
        /// <param name="param">WHERE条件 eg: new { Name="张三" }</param>
        /// <param name="funcSort">排序 eg: d=>d.CreateTime </param>
        /// <param name="isAsc">升序/降序 默认升序 eg: true </param>
        /// <returns>IEnumerable T </returns>
        public abstract IEnumerable<T> GetPage<T>(IDbConnection conn, int pageNumber, int pageSize, out int totalCounts, object param = null, Expression<Func<T, object>> funcSort = null, bool isAsc = true, IDbTransaction trans = null);
        /// <summary>
        /// 分页获取数据
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalCounts"></param>
        /// <param name="whereParamName"></param>
        /// <param name="whereParamValue"></param>
        /// <param name="funcSort"></param>
        /// <param name="isAsc"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public abstract IEnumerable<T> GetPage<T>(IDbConnection conn, int pageNumber, int pageSize, out int totalCounts, string whereParamName = null, object whereParamValue = null, Expression<Func<T, object>> funcSort = null, bool isAsc = true, IDbTransaction trans = null);
        /// <summary>
        /// 分页获取数据 支持联表分页查询
        /// </summary>
        /// <param name="pageNumber">第几页 从1开始</param>
        /// <param name="pageSize">每页显示数</param>
        /// <param name="totalCounts">总条数</param>
        /// <param name="fields">要查询的字段 联表eg: A.Field1,B.Field2,... </param>
        /// <param name="tableName">表名 联表eg: TableA INNER JOIN TableB ON TableA.Id=TableB.Id</param>
        /// <param name="whereParamName">where条件 可以参数化 联表eg: TableA.Field1=@Field1 AND TableB.Field2=@Field2</param>
        /// <param name="whereParamValue">where条件中参数化的值 联表eg: new { Field1=Value1,Field2=Value2 }</param>
        /// <param name="sort">排序 联表eg: A.Field1 DESC,B.Field2,...</param>
        /// <returns>IEnumerable dynamic </returns>
        public abstract IEnumerable<T> GetSqlPage<T>(IDbConnection conn, int pageNumber, int pageSize, out int totalCounts, string fields = "*", string tableName = null, string whereParamName = null, object whereParamValue = null, string sort = null, IDbTransaction trans = null);
        /// <summary>
        /// 根据SQL查询数据
        /// </summary>
        /// <returns>DataTable</returns>
        public abstract DataTable GetDataTable(IDbConnection conn, string sql, object param = null);
        /// <summary>
        /// 批量插入 
        /// </summary>
        public abstract bool InsertBulk<T>(IDbConnection conn, IEnumerable<T> listT, IDbTransaction trans = null);
        #endregion

        /// <summary>
        /// 将List转为DataTable
        /// </summary>
        /// <param name="dt">dt不为空时用dt的表结构，否者将反射T作为表结构</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(IEnumerable<T> listT, DataTable dt = null)
        {
            var type = typeof(T);
            PropertyInfo[] arrPropertyInfo = type.GetProperties();
            if (dt == null)
            {
                dt = new DataTable();
                foreach (PropertyInfo item in arrPropertyInfo)//遍历类的公共属性
                {
                    dt.Columns.Add(item.Name, Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType);
                }
            }
            foreach (T t in listT)
            {
                DataRow row = dt.NewRow();
                foreach (PropertyInfo item in arrPropertyInfo)
                {
                    var val = item.GetValue(t, null);
                    row[item.Name] = val ?? DBNull.Value;
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
        /// <summary>
        /// 是否唯一键
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsKey(PropertyInfo item)
        {
            KeyAttribute attrKey = Attribute.GetCustomAttribute(item, typeof(KeyAttribute)) as KeyAttribute;
            return attrKey != null;
        }
        /// <summary>
        /// 是否自增列
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsIdentity(PropertyInfo item)
        {
            IdentityAttribute attrIdentity = Attribute.GetCustomAttribute(item, typeof(IdentityAttribute)) as IdentityAttribute;
            return attrIdentity != null;
        }
        /// <summary>
        /// 是否额外元素 如果标记则不对该属性进行数据库操作
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsExtraElement(PropertyInfo item)
        {
            var attr = Attribute.GetCustomAttribute(item, typeof(ExtraElementAttribute)) as ExtraElementAttribute;
            return attr != null;
        }
        /// <summary>
        /// 获取lambda表达式的值 仅用于排序
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetExpresssKey(LambdaExpression expression)
        {
            var key = string.Empty;
            var exp = expression.Body.ToString();//"Convert(d.Id)"
            if (exp.Contains("."))
            {
                key = exp.Substring(exp.IndexOf(".") + 1).Replace(")", "");
            }
            return key;
        }
        /// <summary>
        /// 获取表名 如果类有TableAttribute特性，采用特性说明的类名，否则用类名作为表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableName(Type type)
        {
            TableAttribute attr = Attribute.GetCustomAttribute(type, typeof(TableAttribute)) as TableAttribute;
            return attr != null ? attr.TableName : type.Name;
        }
        /// <summary>
        /// 判断是否为空日期类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsDateTimeAndNull<T>(PropertyInfo item, T t)
        {
            if (item.PropertyType.Name == "DateTime" || (IsNullableType(item) && GetNullableType(item).Name == "DateTime"))
            {
                var date = item.GetValue(t, null);
                return date != null && date.ToString().StartsWith("0001");
            }
            return false;
        }
        /// <summary>
        /// 判断是否是可空类型
        /// </summary>
        /// <returns></returns>
        public static bool IsNullableType(PropertyInfo item)
        {
            return (item.PropertyType.IsGenericType && item.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }
        /// <summary>
        /// 获取可空类型的基类型
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Type GetNullableType(PropertyInfo item)
        {
            if (IsNullableType(item)) //如果是可空类型
            {
                return item.PropertyType.GetGenericArguments()[0]; //返回基类型
            }
            return item.PropertyType;
        }
        /// <summary>
        /// 判断是否是IEnumerable类型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="t"></param>
        /// <returns></returns>
        public static bool IsIEnumerable<T>(T t)
        {
            var type = t.GetType().GetInterface("IEnumerable");
            return type != null;
        }
        public static string GetWhere(object param)
        {
            var sb = new StringBuilder();
            foreach (PropertyInfo item in param.GetType().GetProperties())
            {
                sb.Append($"{item.Name}=@{item.Name} AND ");
            }
            sb = sb.Remove(sb.Length - 4, 4);//去掉最后AND
            return sb.ToString();
        }
    }
}

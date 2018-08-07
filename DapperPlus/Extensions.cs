using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Reflection;
using DapperPlus.Attributes;
using System.Linq.Expressions;
using System.Data.SqlClient;

namespace Dapper
{
    /// <summary>
    /// 扩展
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// 添加一个实体
        /// </summary>
        /// <returns>受影响行数</returns>
        public static int Add<T>(this IDbConnection conn, T t, IDbTransaction trans = null) where T : class
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
        public static int Add<T>(this IDbConnection conn, List<T> listT, IDbTransaction trans = null) where T : class
        {
            var count = 0;
            foreach (var item in listT)
            {
                count += conn.Add(item, trans);
            }
            return count;
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="param">WHERE条件 eg: new { Name="张三" }</param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static int Delete<T>(this IDbConnection conn, object param = null, IDbTransaction trans = null) where T : class
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
        public static int Delete<T>(this IDbConnection conn, T t, IDbTransaction trans = null) where T : class
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
        public static int Delete<T>(this IDbConnection conn, List<T> listT, IDbTransaction trans = null) where T : class
        {
            var count = 0;
            foreach (var item in listT)
            {
                count += conn.Delete(item, trans);
            }
            return count;
        }
        /// <summary>
        /// 更新一个实体 必须对实体标记主键[Key]或自增标识[Identity]
        /// </summary>
        /// <returns>受影响行数</returns>
        public static int Update<T>(this IDbConnection conn, T t, IDbTransaction trans = null) where T : class
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
        public static int Update<T>(this IDbConnection conn, List<T> listT, IDbTransaction trans = null) where T : class
        {
            var count = 0;
            foreach (var item in listT)
            {
                count += conn.Update(item, trans);
            }
            return count;
        }
        /// <summary>
        ///获取一个实体 
        /// </summary>
        /// <param name="param">WHERE条件 eg: new { Name="张三" }</param>
        /// <returns>T</returns>
        public static T GetModel<T>(this IDbConnection conn, object param = null, IDbTransaction trans = null) where T : class
        {
            return GetList<T>(conn, param, trans: trans).FirstOrDefault();
        }
        /// <summary>
        /// 获取实体集合
        /// </summary>
        /// <param name="param">WHERE条件 eg: new { Name="张三" }</param>
        /// <param name="funcSort">排序 eg: d=>d.CreateTime </param>
        /// <param name="isAsc">升序/降序 默认升序 eg: true </param>
        /// <param name="top">top 条数</param>
        /// <returns>IEnumerable T </returns>
        public static IEnumerable<T> GetList<T>(this IDbConnection conn, object param = null, Expression<Func<T, object>> funcSort = null, bool isAsc = true, int? top = null, IDbTransaction trans = null) where T : class
        {
            var type = typeof(T);
            var tableName = GetTableName(type);
            var sbSql = new StringBuilder($"SELECT {(top.HasValue ? $"TOP {top.Value} " : "")} * FROM {tableName} ");
            if (param != null)
            {
                sbSql.Append("WHERE ");
                foreach (PropertyInfo item in param.GetType().GetProperties())
                {
                    sbSql.Append($"{item.Name}=@{item.Name} AND ");
                }
                sbSql = sbSql.Remove(sbSql.Length - 4, 4);//去掉最后AND
            }
            if (funcSort != null)
            {
                var sortKey = GetExpresssKey(funcSort);
                sbSql.Append($"ORDER BY {sortKey}");
                if (!isAsc)
                {
                    sbSql.Append(" DESC");
                }
            }
            return conn.Query<T>(sbSql.ToString(), param, trans);
        }
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
        public static IEnumerable<T> GetSqlList<T>(this IDbConnection conn, string fields = "*", string tableName = null, string whereParamName = null, object whereParamValue = null, string sort = null, int? top = null, IDbTransaction trans = null)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                var type = typeof(T);
                if (type == typeof(object))
                {
                    throw new Exception($"未指定表名时必须传入正确的泛型类型");
                }
                tableName = GetTableName(type);
            }
            whereParamName = whereParamName ?? " 1=1 ";
            var sql = $"SELECT {(top.HasValue ? $"TOP {top.Value} " : "")} {fields} FROM {tableName} WHERE {whereParamName} ";
            sql += !string.IsNullOrEmpty(sort) ? " ORDER BY " + sort : "";
            return conn.Query<T>(sql, whereParamValue);
        }
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
        public static IEnumerable<T> GetPage<T>(this IDbConnection conn, int pageNumber, int pageSize, out int totalCounts, object param = null, Expression<Func<T, object>> funcSort = null, bool isAsc = true, IDbTransaction trans = null)
        {
            var type = typeof(T);
            var tableName = GetTableName(type);
            var sort = "(SELECT 0)";
            if (funcSort != null)
            {
                sort = GetExpresssKey(funcSort);
                if (!isAsc)
                {
                    sort += " DESC ";
                }
            }
            //SELECT * FROM (SELECT row_number() OVER(ORDER BY {0}) AS num,{1} FROM {2} WHERE {3}) AS t WHERE t.num>{4} AND t.num<={5} "
            var sbSql = new StringBuilder($"SELECT * FROM (SELECT row_number() OVER(ORDER BY {sort}) AS num,* FROM {tableName} ");
            var sbSqlTotalCount = new StringBuilder($"SELECT COUNT(*) FROM {tableName} ");
            if (param != null)
            {
                sbSql.Append("WHERE ");
                sbSqlTotalCount.Append("WHERE ");
                foreach (PropertyInfo item in param.GetType().GetProperties())
                {
                    sbSql.Append($"{item.Name}=@{item.Name} AND ");
                    sbSqlTotalCount.Append($"{item.Name}=@{item.Name} AND ");
                }
                sbSql = sbSql.Remove(sbSql.Length - 4, 4);//去掉最后AND
                sbSqlTotalCount = sbSqlTotalCount.Remove(sbSqlTotalCount.Length - 4, 4);//去掉最后AND
            }
            sbSql.Append($") AS t WHERE t.num>{(pageNumber - 1) * pageSize} AND t.num<={(pageNumber - 1) * pageSize + pageSize}");
            totalCounts = conn.ExecuteScalar<int>(sbSqlTotalCount.ToString(), param, trans);
            return conn.Query<T>(sbSql.ToString(), param, trans);
        }
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
        public static IEnumerable<T> GetSqlPage<T>(this IDbConnection conn, int pageNumber, int pageSize, out int totalCounts, string fields = "*", string tableName = null, string whereParamName = null, object whereParamValue = null, string sort = null, IDbTransaction trans = null)
        {
            if (string.IsNullOrEmpty(tableName))
            {
                var type = typeof(T);
                if (type == typeof(object))
                {
                    throw new Exception($"未指定表名时必须传入正确的泛型类型");
                }
                tableName = GetTableName(type);
            }
            sort = string.IsNullOrEmpty(sort) ? "(SELECT 0)" : sort;
            whereParamName = whereParamName ?? " 1=1 ";
            var sql = $@"SELECT * FROM (SELECT row_number() OVER(ORDER BY {sort}) AS num,{fields} FROM {tableName} WHERE {whereParamName} )
                        AS T WHERE T.num>{(pageNumber - 1) * pageSize} AND T.num<={(pageNumber - 1) * pageSize + pageSize} ";
            totalCounts = conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM {tableName} WHERE {whereParamName}", whereParamValue);
            return conn.Query<T>(sql, whereParamValue);
        }
        /// <summary>
        /// 根据SQL查询数据
        /// </summary>
        /// <returns>DataTable</returns>
        public static DataTable GetDataTable(this IDbConnection conn, string sql)
        {
            DataTable dt = new DataTable();
            if (conn is SqlConnection)
            {
                var sqlConn = conn as SqlConnection;
                SqlDataAdapter adapter = new SqlDataAdapter(sql, sqlConn);
                adapter.Fill(dt);
            }
            return dt;
        }
        /// <summary>
        /// 批量插入 
        /// </summary>
        public static bool InsertBulk<T>(this IDbConnection conn, List<T> listT, IDbTransaction trans = null) where T : class
        {
            if (conn is SqlConnection)
            {
                var type = typeof(T);
                var tableName = GetTableName(type);
                //SqlBulkCopy不是根据表的ColumnName来匹配的，而是根据ColumnIndex匹配，
                //也就是说你的表 字段必须跟数据库的表字段完全一致(Index的排序要跟数据表的一样)。
                var dtEmpty = conn.GetDataTable($"SELECT * FROM {tableName} WHERE 1=2");//返回空结构，用于对齐字段
                var dt = listT.ToDataTable(dtEmpty);
                var sqlConn = conn as SqlConnection;
                var sqlTrans = trans as SqlTransaction;
                using (SqlBulkCopy bulkCopy = new SqlBulkCopy(sqlConn, SqlBulkCopyOptions.Default, sqlTrans))
                {
                    bulkCopy.DestinationTableName = tableName;
                    bulkCopy.BatchSize = dt.Rows.Count;
                    bulkCopy.WriteToServer(dt);
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// 将List转为DataTable
        /// </summary>
        /// <param name="dt">dt不为空时用dt的表结构，否者将反射T作为表结构</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IList<T> listT, DataTable dt = null)
        {
            var type = typeof(T);
            PropertyInfo[] arrPropertyInfo = type.GetProperties();
            if (dt == null)
            {
                dt = new DataTable();
                foreach (PropertyInfo item in arrPropertyInfo)//遍历类的公共属性
                {
                    if (!IsExtraElement(item))//过滤额外元素
                    {
                        dt.Columns.Add(item.Name, Nullable.GetUnderlyingType(item.PropertyType) ?? item.PropertyType);
                    }
                }
            }
            foreach (T t in listT)
            {
                DataRow row = dt.NewRow();
                foreach (PropertyInfo item in arrPropertyInfo)
                {
                    if (!IsExtraElement(item) && !IsDateTimeAndNull(item, t)) //过滤额外元素、空DateTime
                    {
                        var val = item.GetValue(t, null);
                        row[item.Name] = val ?? DBNull.Value;
                    }
                }
                dt.Rows.Add(row);
            }
            return dt;
        }
        private static bool IsKey(PropertyInfo item)
        {
            KeyAttribute attrKey = Attribute.GetCustomAttribute(item, typeof(KeyAttribute)) as KeyAttribute;
            return attrKey != null;
        }
        private static bool IsIdentity(PropertyInfo item)
        {
            IdentityAttribute attrIdentity = Attribute.GetCustomAttribute(item, typeof(IdentityAttribute)) as IdentityAttribute;
            return attrIdentity != null;
        }
        private static bool IsExtraElement(PropertyInfo item)
        {
            var attr = Attribute.GetCustomAttribute(item, typeof(ExtraElementAttribute)) as ExtraElementAttribute;
            return attr != null;
        }
        private static string GetExpresssKey(LambdaExpression expression)
        {
            var key = string.Empty;
            var exp = expression.Body.ToString();//"Convert(d.Id)"
            if (exp.Contains("."))
            {
                key = exp.Substring(exp.IndexOf(".") + 1).Replace(")", "");
            }
            return key;
        }
        private static string GetTableName(Type type)
        {
            //如果类有TableAttribute特性，采用特性说明的类名，否则用类名作为表名
            TableAttribute attr = Attribute.GetCustomAttribute(type, typeof(TableAttribute)) as TableAttribute;
            return attr != null ? attr.TableName : type.Name;
        }
        private static bool IsDateTimeAndNull<T>(PropertyInfo item, T t)
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
        private static bool IsNullableType(PropertyInfo item)
        {
            return (item.PropertyType.IsGenericType && item.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
        }
        /// <summary>
        /// 获取可空类型的根类型
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static Type GetNullableType(PropertyInfo item)
        {
            if (IsNullableType(item)) //如果是可空类型
            {
                return item.PropertyType.GetGenericArguments()[0]; //返回根类型
            }
            return item.PropertyType;
        }
    }
}

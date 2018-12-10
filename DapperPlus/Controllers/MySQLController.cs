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
    /// MySql操作
    /// </summary>
    public class MySQLController : DBBaseController
    {
        /// <summary>
        /// 根据SQL查询数据
        /// </summary>
        /// <returns>DataTable</returns>
        public override DataTable GetDataTable(IDbConnection conn, string sql, object param = null)
        {
            DataTable dt = new DataTable();
            var reader = conn.ExecuteReader(sql, param);
            dt.Load(reader);
            return dt;
        }
        /// <summary>
        /// 获取实体集合
        /// </summary>
        /// <param name="param">WHERE条件 eg: new { Name="张三" }</param>
        /// <param name="funcSort">排序 eg: d=>d.CreateTime </param>
        /// <param name="isAsc">升序/降序 默认升序 eg: true </param>
        /// <param name="top">top 条数</param>
        /// <returns>IEnumerable T </returns>
        public override IEnumerable<T> GetList<T>(IDbConnection conn, object param = null, Expression<Func<T, object>> funcSort = null, bool isAsc = true, int? top = null, IDbTransaction trans = null)
        {
            string tableName = null, sqlWhere = null;
            return GetData(conn, out tableName, out sqlWhere, param, null, null, funcSort, isAsc, pageSize: top, trans: trans);
        }
        /// <summary>
        /// 获取实体集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="whereParamName">字符串WHERE条件</param>
        /// <param name="whereParamValue">字符串WHERE条件中的参数值</param>
        /// <param name="funcSort">排序 eg: d=>d.CreateTime</param>
        /// <param name="isAsc">升序/降序 默认升序 eg: true </param>
        /// <param name="top">top 条数</param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public override IEnumerable<T> GetList<T>(IDbConnection conn, string whereParamName = null, object whereParamValue = null, Expression<Func<T, object>> funcSort = null, bool isAsc = true, int? top = null, IDbTransaction trans = null)
        {
            string tableName = null, sqlWhere = null;
            return GetData(conn, out tableName, out sqlWhere, null, whereParamName, whereParamValue, funcSort, isAsc, pageSize: top, trans: trans);
        }
        /// <summary>
        ///获取一个实体 
        /// </summary>
        /// <param name="param">WHERE条件 eg: new { Name="张三" }</param>
        /// <returns>T</returns>
        public override T GetModel<T>(IDbConnection conn, object param = null, IDbTransaction trans = null)
        {
            return GetList<T>(conn, param, trans: trans).FirstOrDefault();
        }
        /// <summary>
        /// 获取一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="whereParamName">字符串WHERE条件</param>
        /// <param name="whereParamValue">字符串WHERE条件中的参数值</param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public override T GetModel<T>(IDbConnection conn, string whereParamName = null, object whereParamValue = null, IDbTransaction trans = null)
        {
            return GetList<T>(conn, whereParamName, whereParamValue, trans: trans).FirstOrDefault();
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
        public override IEnumerable<T> GetPage<T>(IDbConnection conn, int pageNumber, int pageSize, out int totalCounts, object param = null, Expression<Func<T, object>> funcSort = null, bool isAsc = true, IDbTransaction trans = null)
        {
            //SELECT COUNT(*) FROM Table
            //SELECT * FROM Table WHERE 1=1 ORDER BY Field DESC LIMIT 0,10
            string tableName = null, sqlWhere = null;
            var list = GetData(conn, out tableName, out sqlWhere, param, null, null, funcSort, isAsc, pageNumber, pageSize, trans);
            var sbSqlTotalCount = new StringBuilder($"SELECT COUNT(*) FROM {tableName} ");
            if (!string.IsNullOrEmpty(sqlWhere))
            {
                sbSqlTotalCount.Append(sqlWhere);
            }
            totalCounts = conn.ExecuteScalar<int>(sbSqlTotalCount.ToString(), param, trans);
            return list;
        }
        /// <summary>
        /// 分页获取实体集合
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="pageNumber">第几页 从1开始</param>
        /// <param name="pageSize">每页显示数</param>
        /// <param name="totalCounts">总条数</param>
        /// <param name="whereParamName">字符串WHERE条件</param>
        /// <param name="whereParamValue">字符串WHERE条件中的参数值</param>
        /// <param name="funcSort"></param>
        /// <param name="isAsc"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public override IEnumerable<T> GetPage<T>(IDbConnection conn, int pageNumber, int pageSize, out int totalCounts, string whereParamName = null, object whereParamValue = null, Expression<Func<T, object>> funcSort = null, bool isAsc = true, IDbTransaction trans = null)
        {
            string tableName = null, sqlWhere = null;
            var list = GetData(conn, out tableName, out sqlWhere, null, whereParamName, whereParamValue, funcSort, isAsc, pageNumber, pageSize, trans);
            var sbSqlTotalCount = new StringBuilder($"SELECT COUNT(*) FROM {tableName} ");
            if (!string.IsNullOrEmpty(sqlWhere))
            {
                sbSqlTotalCount.Append(sqlWhere);
            }
            totalCounts = conn.ExecuteScalar<int>(sbSqlTotalCount.ToString(), whereParamValue, trans);
            return list;
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
        public override IEnumerable<T> GetSqlList<T>(IDbConnection conn, string fields = "*", string tableName = null, string whereParamName = null, object whereParamValue = null, string sort = null, int? top = null, IDbTransaction trans = null)
        {
            return GetSqlData<T>(conn, fields, tableName, whereParamName, whereParamValue, sort, null, top, trans);
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
        public override IEnumerable<T> GetSqlPage<T>(IDbConnection conn, int pageNumber, int pageSize, out int totalCounts, string fields = "*", string tableName = null, string whereParamName = null, object whereParamValue = null, string sort = null, IDbTransaction trans = null)
        {
            var list = GetSqlData<T>(conn, fields, tableName, whereParamName, whereParamValue, sort, pageNumber, pageSize, trans);
            var sbSqlTotalCount = new StringBuilder($"SELECT COUNT(*) FROM {tableName} ");
            if (!string.IsNullOrEmpty(whereParamName))
            {
                sbSqlTotalCount.Append($"WHERE {whereParamName}");
            }
            totalCounts = conn.ExecuteScalar<int>(sbSqlTotalCount.ToString(), whereParamValue, trans);
            return list;
        }
        /// <summary>
        /// MySql批量插入 暂不支持
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="listT"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        [Obsolete("MySql批量插入 暂不支持")]
        public override bool InsertBulk<T>(IDbConnection conn, IEnumerable<T> listT, IDbTransaction trans = null)
        {
            throw new NotImplementedException();
        }
        private IEnumerable<T> GetData<T>(IDbConnection conn, out string tableName, out string sqlWhere, object param = null, string whereParamName = null, object whereParamValue = null, Expression<Func<T, object>> funcSort = null, bool isAsc = true, int? pageNumber = null, int? pageSize = null, IDbTransaction trans = null)
        {
            //SELECT * FROM Table WHERE 1=1 ORDER BY Field DESC LIMIT 0,10
            var type = typeof(T);
            tableName = GetTableName(type);
            var sbSql = new StringBuilder($"SELECT * FROM {tableName} ");
            object paramValue = null;
            var sbWhere = new StringBuilder();
            if (param != null)//追加条件
            {
                var where = $"WHERE {GetWhere(param)} ";
                sbSql.Append(where);
                sbWhere.Append(where);
                paramValue = param;
            }
            else if (!string.IsNullOrEmpty(whereParamName))
            {
                var where = $"WHERE {whereParamName} ";
                sbSql.Append(where);
                sbWhere.Append(where);
                paramValue = whereParamValue;
            }
            sqlWhere = sbWhere.ToString();
            if (funcSort != null)//追加排序
            {
                var sortKey = GetExpresssKey(funcSort);
                sbSql.Append($"ORDER BY {sortKey} ");
                if (!isAsc)
                {
                    sbSql.Append("DESC ");
                }
            }
            if (pageNumber.HasValue && pageSize.HasValue)//分页Limit
            {
                sbSql.Append($"LIMIT {(pageNumber.Value - 1) * pageSize},{pageSize.Value}");
            }
            else if (!pageNumber.HasValue && pageSize.HasValue)//Top Limit
            {
                sbSql.Append($"LIMIT {pageSize.Value}");
            }
            return conn.Query<T>(sbSql.ToString(), paramValue, trans);
        }
        private IEnumerable<T> GetSqlData<T>(IDbConnection conn, string fields = "*", string tableName = null, string whereParamName = null, object whereParamValue = null, string sort = null, int? pageNumber = null, int? pageSize = null, IDbTransaction trans = null)
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
            //SELECT * FROM Table WHERE 1=1 ORDER BY Field DESC LIMIT 0,10
            var sbSql = new StringBuilder($"SELECT {fields} FROM {tableName} WHERE {whereParamName = whereParamName ?? "1=1"} ");
            if (!string.IsNullOrEmpty(sort))
            {
                sbSql.Append($"ORDER BY {sort} ");
            }
            if (pageNumber.HasValue && pageSize.HasValue)
            {
                sbSql.Append($"LIMIT {(pageNumber.Value - 1) * pageSize},{pageSize.Value} ");
            }
            else if (!pageNumber.HasValue && pageSize.HasValue)
            {
                sbSql.Append($"LIMIT {pageSize.Value} ");
            }
            return conn.Query<T>(sbSql.ToString(), whereParamValue, trans);
        }
    }
}

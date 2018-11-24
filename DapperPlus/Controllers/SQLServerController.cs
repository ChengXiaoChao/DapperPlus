using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace DapperPlus.Controllers
{
    /// <summary>
    /// SQLServer操作
    /// </summary>
    public class SQLServerController : DBBaseController
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
        ///获取一个实体 
        /// </summary>
        /// <param name="param">WHERE条件 eg: new { Name="张三" }</param>
        /// <returns>T</returns>
        public override T GetModel<T>(IDbConnection conn, object param = null, IDbTransaction trans = null)
        {
            return GetList<T>(conn, param, trans: trans).FirstOrDefault();
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
            if (string.IsNullOrEmpty(tableName))
            {
                var type = typeof(T);
                if (type == typeof(object))
                {
                    throw new Exception($"未指定表名时必须传入正确的泛型类型");
                }
                tableName = GetTableName(type);
            }
            var sql = $"SELECT {(top.HasValue ? $"TOP {top.Value} " : "")} {fields} FROM {tableName} WHERE {whereParamName ?? " 1=1 "} ";
            sql += !string.IsNullOrEmpty(sort) ? " ORDER BY " + sort : "";
            return conn.Query<T>(sql, whereParamValue);
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
            var sql = $@"SELECT * FROM (SELECT row_number() OVER(ORDER BY {sort}) AS num,{fields} FROM {tableName} WHERE {whereParamName} )
                        AS T WHERE T.num>{(pageNumber - 1) * pageSize} AND T.num<={(pageNumber - 1) * pageSize + pageSize} ";
            totalCounts = conn.ExecuteScalar<int>($"SELECT COUNT(*) FROM {tableName} WHERE {whereParamName ?? " 1=1 "}", whereParamValue);
            return conn.Query<T>(sql, whereParamValue);
        }
        /// <summary>
        /// 批量插入 
        /// </summary>
        public override bool InsertBulk<T>(IDbConnection conn, IEnumerable<T> listT, IDbTransaction trans = null)
        {
            try
            {
                var type = typeof(T);
                var tableName = GetTableName(type);
                //SqlBulkCopy不是根据表的ColumnName来匹配的，而是根据ColumnIndex匹配，
                //也就是说你的表 字段必须跟数据库的表字段完全一致(Index的排序要跟数据表的一样)。
                var dtEmpty = conn.GetDataTable($"SELECT * FROM {tableName} WHERE 1=2");//返回空结构，用于对齐字段
                var dt = ToDataTable(listT, dtEmpty);
                var sqlConn = conn as SqlConnection;
                var sqlTrans = trans as SqlTransaction;
                using (var bulkCopy = new System.Data.SqlClient.SqlBulkCopy(sqlConn, SqlBulkCopyOptions.Default, sqlTrans))
                {
                    bulkCopy.DestinationTableName = tableName;
                    bulkCopy.BatchSize = dt.Rows.Count;
                    bulkCopy.WriteToServer(dt);
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

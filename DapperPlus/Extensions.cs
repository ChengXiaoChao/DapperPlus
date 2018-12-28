using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DapperPlus.Controllers;

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
            var controller = DBFactory.Create(conn);
            return controller.Add(conn, t, trans);
        }
        /// <summary>
        /// 添加多个实体
        /// </summary>
        /// <returns>受影响行数</returns>
        public static int AddList<T>(this IDbConnection conn, IEnumerable<T> listT, IDbTransaction trans = null) where T : class
        {
            var controller = DBFactory.Create(conn);
            return controller.AddList(conn, listT, trans);
        }
        /// <summary>
        /// 删除数据
        /// </summary>
        /// <param name="param">WHERE条件 eg: new { Name="张三" }</param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static int Delete<T>(this IDbConnection conn, object param = null, IDbTransaction trans = null) where T : class
        {
            var controller = DBFactory.Create(conn);
            return controller.Delete<T>(conn, param, trans);
        }
        /// <summary>
        /// 删除一个实体 必须对实体标记主键[Key]或自增标识[Identity]
        /// </summary>
        /// <returns>受影响行数</returns>
        public static int Delete<T>(this IDbConnection conn, T t, IDbTransaction trans = null) where T : class
        {
            var controller = DBFactory.Create(conn);
            return controller.Delete(conn, t, trans);
        }
        /// <summary>
        /// 删除多个实体 必须对实体标记主键[Key]或自增标识[Identity]
        /// </summary>
        /// <returns>受影响行数</returns>
        public static int DeleteList<T>(this IDbConnection conn, IEnumerable<T> listT, IDbTransaction trans = null) where T : class
        {
            var controller = DBFactory.Create(conn);
            return controller.DeleteList(conn, listT, trans);
        }
        /// <summary>
        /// 更新一个实体 必须对实体标记主键[Key]或自增标识[Identity]
        /// </summary>
        /// <returns>受影响行数</returns>
        public static int Update<T>(this IDbConnection conn, T t, IDbTransaction trans = null) where T : class
        {
            var controller = DBFactory.Create(conn);
            return controller.Update(conn, t, trans);
        }
        /// <summary>
        /// 更新多个实体 必须对实体标记主键[Key]或自增标识[Identity]
        /// </summary>
        /// <returns>受影响行数</returns>
        public static int UpdateList<T>(this IDbConnection conn, IEnumerable<T> listT, IDbTransaction trans = null) where T : class
        {
            var controller = DBFactory.Create(conn);
            return controller.UpdateList(conn, listT, trans);
        }
        /// <summary>
        ///获取一个实体 
        /// </summary>
        /// <param name="param">WHERE条件 eg: new { Name="张三" }</param>
        /// <returns>T</returns>
        public static T GetModel<T>(this IDbConnection conn, object param = null, IDbTransaction trans = null) where T : class
        {
            var controller = DBFactory.Create(conn);
            return controller.GetModel<T>(conn, param, trans);
        }
        /// <summary>
        /// 获取一个实体
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conn"></param>
        /// <param name="whereParamName">字符串WHERE条件</param>
        /// <param name="whereParamValue">字符串WHERE条件中参数的值</param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static T GetModel<T>(this IDbConnection conn, string whereParamName = null, object whereParamValue = null, IDbTransaction trans = null) where T : class
        {
            var controller = DBFactory.Create(conn);
            return controller.GetModel<T>(conn, whereParamName, whereParamValue, trans);
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
            var controller = DBFactory.Create(conn);
            return controller.GetList(conn, param, funcSort, isAsc, top, trans);
        }
        /// <summary>
        /// 获取实体集合
        /// </summary>
        /// <param name="whereParamName">字符串WHERE条件</param>
        /// <param name="whereParamValue">字符串WHERE条件中参数的值</param>
        /// <param name="funcSort">排序 eg: d=>d.CreateTime </param>
        /// <param name="isAsc">升序/降序 默认升序 eg: true </param>
        /// <param name="top">top 条数</param>
        /// <returns>IEnumerable T </returns>
        public static IEnumerable<T> GetList<T>(this IDbConnection conn, string whereParamName = null, object whereParamValue = null, Expression<Func<T, object>> funcSort = null, bool isAsc = true, int? top = null, IDbTransaction trans = null) where T : class
        {
            var controller = DBFactory.Create(conn);
            return controller.GetList(conn, whereParamName, whereParamValue, funcSort, isAsc, top, trans);
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
            var controller = DBFactory.Create(conn);
            return controller.GetSqlList<T>(conn, fields, tableName, whereParamName, whereParamValue, sort, top, trans);
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
            var controller = DBFactory.Create(conn);
            return controller.GetPage(conn, pageNumber, pageSize, out totalCounts, param, funcSort, isAsc, trans);
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
        /// <param name="whereParamValue">字符串WHERE条件中参数的值</param>
        /// <param name="funcSort">排序 eg: d=>d.CreateTime</param>
        /// <param name="isAsc">升序/降序 默认升序 eg: true</param>
        /// <param name="trans"></param>
        /// <returns></returns>
        public static IEnumerable<T> GetPage<T>(this IDbConnection conn, int pageNumber, int pageSize, out int totalCounts, string whereParamName = null, object whereParamValue = null, Expression<Func<T, object>> funcSort = null, bool isAsc = true, IDbTransaction trans = null)
        {
            var controller = DBFactory.Create(conn);
            return controller.GetPage(conn, pageNumber, pageSize, out totalCounts, whereParamName, whereParamValue, funcSort, isAsc, trans);
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
            var controller = DBFactory.Create(conn);
            return controller.GetSqlPage<T>(conn, pageNumber, pageSize, out totalCounts, fields, tableName, whereParamName, whereParamValue, sort, trans);
        }
        /// <summary>
        /// 根据SQL查询数据
        /// </summary>
        /// <returns>DataTable</returns>
        public static DataTable GetDataTable(this IDbConnection conn, string sql, object param = null)
        {
            var controller = DBFactory.Create(conn);
            return controller.GetDataTable(conn, sql, param);
        }
        /// <summary>
        /// 批量插入 
        /// </summary>
        public static bool InsertBulk<T>(this IDbConnection conn, IEnumerable<T> listT, IDbTransaction trans = null) where T : class
        {
            var controller = DBFactory.Create(conn);
            return controller.InsertBulk(conn, listT, trans);
        }
        /// <summary>
        /// 将List转为DataTable
        /// </summary>
        /// <param name="dt">dt不为空时用dt的表结构，否者将反射T作为表结构</param>
        /// <returns></returns>
        public static DataTable ToDataTable<T>(this IEnumerable<T> listT, DataTable dt = null)
        {
            return DBBaseController.ToDataTable(listT, dt);
        }
        /// <summary>
        /// 是否唯一键
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsKey(PropertyInfo item)
        {
            return DBBaseController.IsKey(item);
        }
        /// <summary>
        /// 是否自增列
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsIdentity(PropertyInfo item)
        {
            return DBBaseController.IsIdentity(item);
        }
        /// <summary>
        /// 是否额外元素 如果标记则不对该属性进行数据库操作
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static bool IsExtraElement(PropertyInfo item)
        {
            return DBBaseController.IsExtraElement(item);
        }
        /// <summary>
        /// 获取lambda表达式的值 仅用于排序
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public static string GetExpresssKey(LambdaExpression expression)
        {
            return DBBaseController.GetExpresssKey(expression);
        }
        /// <summary>
        /// 获取表名 如果类有TableAttribute特性，采用特性说明的类名，否则用类名作为表名
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static string GetTableName(Type type)
        {
            return DBBaseController.GetTableName(type);
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
            return DBBaseController.IsDateTimeAndNull(item, t);
        }
        /// <summary>
        /// 判断是否是可空类型
        /// </summary>
        /// <returns></returns>
        public static bool IsNullableType(PropertyInfo item)
        {
            return DBBaseController.IsNullableType(item);
        }
        /// <summary>
        /// 获取可空类型的根类型
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public static Type GetNullableType(PropertyInfo item)
        {
            return DBBaseController.GetNullableType(item);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DapperPlus.Controllers
{
    /// <summary>
    /// 实例数据库操作管理类
    /// </summary>
    public class DBFactory
    {
        private DBFactory() { }
        /// <summary>
        /// 创建实例
        /// </summary>
        /// <param name="conn"></param>
        /// <returns></returns>
        public static DBBaseController Create(IDbConnection conn)
        {
            switch (conn.GetType().Name)
            {
                case "SqlConnection":
                    return new SQLServerController();
                case "MySqlConnection":
                    return new MySQLController();
                default:
                    throw new Exception("暂时不支持该数据库");
            }
        }
    }
}

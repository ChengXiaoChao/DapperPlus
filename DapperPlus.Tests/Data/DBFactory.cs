using System.Data;
using System.Data.SqlClient;

namespace DapperPlus.Tests.Data
{
    public class DBFactory
    {
        private DBFactory() { }
        private static IDbConnection conn = null;
        static object obj = new object();
        public static IDbConnection GetInstance(SqlType sqlType = SqlType.MSSQL)
        {
            switch (sqlType)
            {
                case SqlType.MSSQL:
                    if (conn == null)
                    {
                        lock (obj)
                        {
                            if (conn == null)
                            {
                                conn = new SqlConnection("Server=.;DataBase=CC;uid=sa;pwd=chengchao");
                            }
                        }
                    }
                    if (conn.State == ConnectionState.Closed)
                    {
                        conn.Open();
                    }
                    return conn;
                default:
                    return null;
            }
        }
    }
    public enum SqlType
    {
        MSSQL
    }
}

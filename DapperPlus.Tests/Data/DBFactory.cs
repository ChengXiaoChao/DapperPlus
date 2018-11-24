using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;

namespace DapperPlus.Tests.Data
{
    public class DBFactory
    {
        private DBFactory() { }
        private static IDbConnection conn = null;
        static object obj = new object();
        public static IDbConnection GetInstance(DBType sqlType)
        {
            if (conn == null)
            {
                lock (obj)
                {
                    if (conn == null)
                    {
                        switch (sqlType)
                        {
                            case DBType.MSSQL:
                                conn = new SqlConnection("Server=.;DataBase=CC;uid=sa;pwd=chengchao");
                                break;
                            case DBType.MYSQL:
                                conn = new MySqlConnection("Server=localhost;DataBase=bf_9pay;uid=root;pwd=chengchao;pooling=false;CharSet=utf8;port=3306");
                                break;
                        }
                    }
                }
            }
            if (conn.State == ConnectionState.Closed)
            {
                conn.Open();
            }
            return conn;

        }
    }
    public enum DBType
    {
        MSSQL, MYSQL
    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using DapperPlus.Tests.Model;
using DapperPlus.Tests.Data;

namespace Dapper.Tests
{
    [TestClass()]
    public class ExtensionsTests
    {

        public System.Data.IDbConnection MySqlConn
        {
            get
            {
                return DBFactory.GetInstance(DBType.MYSQL);
            }
        }
        public System.Data.IDbConnection MsSqlConn
        {
            get
            {
                return DBFactory.GetInstance(DBType.MSSQL);
            }
        }
        public List<OrderHead> GenerateModel(int count)
        {
            var list = new List<OrderHead>();
            for (int i = 0; i < count; i++)
            {
                var model = new OrderHead()
                {
                    ID = Guid.NewGuid().ToString().Replace("-", "") + i,
                    BusinessDate = DateTime.Now,
                    PosSumbitTime = DateTime.Now,
                    CurrentEmployID = "User" + i,
                    CheckNumber = "CHK" + i,
                    StoreCode = "门店号" + i,//门店号
                    StoreName = "测试门店名" + i,
                    PayCode = "Pay" + i,//输入码
                    Ext1 = "MNO" + i,//商户号
                    Ext8 = "1234567890",//随机字符串
                    IPAddress = "127.0.0.1"
                };
                list.Add(model);
            }
            return list;
        }

        [TestMethod()]
        public void InsertBulkTest()
        {
            var list = GenerateModel(10000);
            //var count = MySqlConn.Add(list);
            //Assert.IsTrue(count == list.Count);

            var flg = MySqlConn.InsertBulk(list);
            Assert.IsTrue(flg);
        }

        [TestMethod()]
        public void AddModelTest()
        {
            var list = GenerateModel(1);
            var count = MySqlConn.Add(list[0]);
            Assert.IsTrue(count > 0);
        }

        [TestMethod()]
        public void AddListTest()
        {
            var trans = MySqlConn.BeginTransaction();
            var c = 0;
            var list = GenerateModel(10);
            try
            {
                c = MySqlConn.AddList(list, trans);
                if (c == list.Count)
                {
                    trans.Commit();
                    Console.WriteLine("成功，Commit");
                }
                else
                {
                    trans.Rollback();
                    Console.WriteLine("失败，Rollback");
                }
            }
            catch (Exception ex)
            {
                trans.Rollback();
                Console.WriteLine($"异常 {ex.Message}，Rollback");
            }
            Assert.IsTrue(c == list.Count);
        }

        [TestMethod()]
        public void DeleteWhereTest()
        {
            //var c = MySqlConn.Delete<WXMP_Config>(new { WXMPType = "订阅号3", Number = "Number3" });
            var c = MySqlConn.Delete<OrderHead>(new { ID = "3210ac8239094faab5824434447bbc7e1" });
            Assert.IsTrue(c > 0);
        }
        [TestMethod()]
        public void DeleteModelTest()
        {
            var model = MySqlConn.GetModel<OrderHead>(new { ID = "91555f2b8d9f431695933ddd34a0c54e0" });
            var c = MySqlConn.Delete(model);
            Assert.IsTrue(c > 0);
        }
        [TestMethod()]
        public void DeleteListTest()
        {
            var trans = MySqlConn.BeginTransaction();
            var list = MySqlConn.GetList<OrderHead>(new { IPAddress = "127.0.0.1" }).Take(5).ToList();
            var c = 0;
            try
            {
                c = MySqlConn.DeleteList(list, trans);
                if (c == list.Count())
                {
                    trans.Commit();
                    Console.WriteLine("成功，Commit");
                }
                else
                {
                    trans.Rollback();
                    Console.WriteLine("失败，Rollback");
                }
            }
            catch (Exception ex)
            {
                trans.Rollback();
                Console.WriteLine($"异常 {ex.Message}，Rollback");
            }
            Assert.IsTrue(c == list.Count());
        }

        [TestMethod()]
        public void UpdateModelTest()
        {
            var model = MySqlConn.GetModel<OrderHead>(new { ID = "df0a26f960d6410986cf2147f6b0a90b0" });
            model.CurrentEmployID = "USER0000000";
            var c = MySqlConn.Update(model);
            Assert.IsTrue(c > 0);
        }

        [TestMethod()]
        public void UpdateListTest()
        {
            var list = MySqlConn.GetList<OrderHead>(new { IPAddress = "127.0.0.1" }, d => d.BusinessDate, true, 5);
            foreach (var item in list)
            {
                item.PaymentType = "TEST";
            }
            var c = MySqlConn.UpdateList(list);
            Assert.IsTrue(c == list.Count());
        }

        [TestMethod()]
        public void GetModelTest()
        {
            var model = MySqlConn.GetModel<OrderHead>(null);
            Assert.IsTrue(model != null);
        }

        [TestMethod()]
        public void GetListTest()
        {
            //var list = MySqlConn.GetList<WXMP_Config>(new { WXMPType = "服务号", IsDelete = 1 }, d => d.Id, false).ToList();
            var list = MySqlConn.GetList<OrderHead>(null);
            var dt = list.ToDataTable();
            Assert.IsTrue(list.Count() > 0);
        }

        [TestMethod()]
        public void GetPageTest()
        {
            var totalCount = 0;
            var list = MySqlConn.GetPage<OrderHead>(1, 5, out totalCount, new { IPAddress = "127.0.0.1" }, d => d.BusinessDate, true);
            Assert.IsTrue(list.Count() > 0);
        }

        [TestMethod()]
        public void GetSqlListTest()
        {
            //var where = "ArticleTypeId!=17";
            //var listTOP = MySqlConn.GetSqlList<Article>(whereParamValue: where, top: 5).ToList();

            //联表
            //SELECT A.*,B.UserName,B.UserPhoto FROM Article A INNER JOIN Users B ON A.CreateUser = B.Id
            //WHERE A.ArticleTypeId = 17
            //ORDER BY CreateTime DESC
            var fields = "ID,CheckID,CheckNumber";
            var tableName = "orderhead";
            var where = "PaymentType=@PaymentType";
            var whereVal = new { PaymentType = "TEST" };
            var list = MySqlConn.GetSqlList<dynamic>(fields, tableName, where, whereVal, "BusinessDate DESC", 5);
            Assert.IsTrue(list.Count() > 0);
        }

        [TestMethod()]
        public void GetSqlPageTest()
        {
            var t = 0;
            var fields = "ID,CheckID,CheckNumber";
            var tableName = "orderhead";
            var where = "PaymentType='TEST'";
            var list = MySqlConn.GetSqlPage<dynamic>(1, 4, out t, fields, tableName, where, null, "BusinessDate");
            Assert.IsTrue(list.Count() > 0);
        }

        [TestMethod()]
        public void GetDataTableTest()
        {
            //var dt = MsSqlConn.GetDataTable("SELECT * FROM Users");
            var dt = MySqlConn.GetDataTable("select * from orderhead limit 10");
            Assert.IsTrue(dt != null && dt.Rows.Count > 0);
        }
    }
}
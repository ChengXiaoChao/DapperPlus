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

        public System.Data.IDbConnection Conn
        {
            get
            {
                return DBFactory.GetInstance();
            }
        }
        public List<WXMP_Config> GenerateModel(int count)
        {
            var list = new List<WXMP_Config>();
            for (int i = 0; i < count; i++)
            {
                var model = new WXMP_Config()
                {
                    WXMPType = "订阅号" + (i + 1),
                    Number = "Number" + (i + 1),
                    NumberID = "NumberID" + (i + 1),
                    AppID = "AppID" + (i + 1),
                    AppSecret = "AppSecret" + (i + 1),
                    CusToken = "CusToken" + (i + 1),
                    CusServerUrl = "//345cc.cn",
                    EncodingAESKey = "",
                    AccessToken = "",
                    ExpiresIn = DateTime.MinValue,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    GUID = Guid.NewGuid()
                };
                list.Add(model);
            }
            return list;
        }

        [TestMethod()]
        public void InsertBulkTest()
        {
            var list = GenerateModel(10000);
            //var count = Conn.Add(list);
            //Assert.IsTrue(count == list.Count);

            var flg = Conn.InsertBulk(list);
            Assert.IsTrue(flg);
        }

        [TestMethod()]
        public void AddModelTest()
        {
            //var list = GenerateModel(1);
            var model = new Users()
            {
                Age = 5,
                CreateTime = DateTime.Now,
                LastLoginTime = DateTime.Now
            };
            var count = Conn.Add(model);
            Assert.IsTrue(count > 0);
        }

        [TestMethod()]
        public void AddListTest()
        {
            var trans = Conn.BeginTransaction();
            var c = 0;
            var list = GenerateModel(10);
            try
            {
                list[1].WXMPType = "1234567890";
                c = Conn.Add(list, trans);
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
            var c = Conn.Delete<WXMP_Config>(new { WXMPType = "订阅号3", Number = "Number3" });
            Assert.IsTrue(c > 0);
        }
        [TestMethod()]
        public void DeleteModelTest()
        {
            var model = Conn.GetModel<WXMP_Config>(new { GUID = Guid.Parse("4221A7D4-3294-4C84-B03E-1815FAC26F1D") });
            var c = Conn.Delete(model);
            Assert.IsTrue(c > 0);
        }
        [TestMethod()]
        public void DeleteListTest()
        {
            var trans = Conn.BeginTransaction();
            var list = new List<WXMP_Config>() {
                new WXMP_Config() { GUID=Guid.Parse("909D6E50-77CE-467D-99D6-9B4C01A747B3") },
                new WXMP_Config() { GUID=Guid.Parse("4774888B-A2F8-4C36-BF18-D5ECCE04AE35") },
            };
            var c = 0;
            try
            {
                c = Conn.Delete(list, trans);
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
        public void UpdateModelTest()
        {
            var model = Conn.GetModel<test>(new { uid = "84DE1F7C-9091-4D3D-850D-FB98CF56B0B1" });
            //model.uid = "111111111111";
            model.name = "test";
            var c = Conn.Update(model);
            Assert.IsTrue(c > 0);
        }

        [TestMethod()]
        public void UpdateListTest()
        {
            var list = Conn.GetList<WXMP_Config>(new { guid = Guid.Parse("A6F25BAB-C84E-431C-83E6-F674B0F479A6") }).ToList();
            foreach (var item in list)
            {
                item.WXMPType = "服务号";
                item.UpdateDate = DateTime.Now;
                item.IsDelete = true;
            }
            var c = Conn.Update(list);
            Assert.IsTrue(c == list.Count);
        }

        [TestMethod()]
        public void GetModelTest()
        {
            var model = Conn.GetModel<WXMP_Config>(new { WXMPType = "服务号", IsDelete = true });
            Assert.IsTrue(model != null);
        }

        [TestMethod()]
        public void GetListTest()
        {
            var list = Conn.GetList<WXMP_Config>(new { WXMPType = "服务号", IsDelete = 1 }, d => d.Id, false).ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [TestMethod()]
        public void GetPageTest()
        {
            var totalCount = 0;
            var list = Conn.GetPage<WXMP_Config>(1, 10, out totalCount, new { WXMPType = "服务号", IsDelete = 1 }, d => d.Id, false).ToList();
            Assert.IsTrue(list.Count > 0);
        }

        [TestMethod()]
        public void GetSqlListTest()
        {
            //var where = "ArticleTypeId!=17";
            //var listTOP = Conn.GetSqlList<Article>(whereParamValue: where, top: 5).ToList();

            //联表
            //SELECT A.*,B.UserName,B.UserPhoto FROM Article A INNER JOIN Users B ON A.CreateUser = B.Id
            //WHERE A.ArticleTypeId = 17
            //ORDER BY CreateTime DESC
            var fields = "A.*,B.UserName,B.UserPhoto";
            var tableName = "Article A INNER JOIN Users B ON A.CreateUser = B.Id";
            var where = "A.ArticleTypeId = 17";
            var listTOP = Conn.GetSqlList<dynamic>(fields, tableName, where, null, "CreateTime DESC", 20).ToList();
            Assert.IsTrue(listTOP.Count > 0);
        }

        [TestMethod()]
        public void GetSqlPageTest()
        {
            var t = 0;
            var where = "ArticleTypeId!=17";
            //var list = Conn.GetSqlPage<Article>(1, 5, out t, whereParamName: where, sort: "CreateTime DESC").ToList();

            //联表
            //SELECT A.*,B.UserName,B.UserPhoto FROM Article A INNER JOIN Users B ON A.CreateUser = B.Id
            //WHERE A.ArticleTypeId = 17
            //ORDER BY CreateTime DESC
            var fields = "A.*,B.UserName,B.UserPhoto";
            var tableName = "Article A INNER JOIN Users B ON A.CreateUser = B.Id";
            var list = Conn.GetSqlPage<dynamic>(1, 8, out t, fields, tableName, where, null, "A.CreateTime DESC").ToList();
            Assert.IsTrue(list.Count > 0);
        }
    }
}
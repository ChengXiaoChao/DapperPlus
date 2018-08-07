using DapperPlus.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DapperPlus.Tests.Model
{
    public class test
    {
        [Identity]
        public int id { get; set; }
        [DapperPlus.Attributes.Key]
        public string uid { get; set; }
        public string name { get; set; }
    }
    public class Users
    {
        [DapperPlus.Attributes.Identity]
        public int Id { get; set; }
        public string UserName { get; set; }
        public string UserLoginName { get; set; }
        public string UserPwd { get; set; }
        public string UserPhoto { get; set; }
        public bool IsAdmin { get; set; }
        public string UserType { get; set; }
        public string OpenId { get; set; }
        public bool? IsEnabled { get; set; }
        public string Sex { get; set; }
        public int? Age { get; set; }
        public string Email { get; set; }
        public string Signature { get; set; }
        public string AddressId { get; set; }
        public string AddressDetial { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime? LastLoginTime { get; set; }
        public string LastLoginIp { get; set; }
    }
    public class Article
    {
        [DapperPlus.Attributes.Identity]

        public int Id { get; set; }
        public int ArticleTypeId { get; set; }
        public string HeaderImg { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int CreateUser { get; set; }
        public DateTime CreateTime { get; set; }
        public int UpdateUser { get; set; }
        public DateTime UpdateTime { get; set; }
        public bool IsEnabled { get; set; }
        public int ReadCount { get; set; }
        public bool IsTop { get; set; }
        public int CommentCount { get; set; }
        public int PointCount { get; set; }
    }

    [Table("WXMP_Config")]
    public class WXMP_Config
    {
        [Identity]
        [Key]
        public int Id { get; set; }
        [Key]
        public Guid GUID { get; set; }
        public string WXMPType { get; set; }
        public string Number { get; set; }
        public string NumberID { get; set; }
        public string AppID { get; set; }
        public string AppSecret { get; set; }
        public string CusToken { get; set; }
        public string CusServerUrl { get; set; }
        public string EncodingAESKey { get; set; }
        public string AccessToken { get; set; }
        public DateTime? ExpiresIn { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public bool IsDelete { get; set; }
    }

}

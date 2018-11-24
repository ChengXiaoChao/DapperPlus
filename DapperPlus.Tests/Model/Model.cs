using DapperPlus.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DapperPlus.Tests.Model
{
    public class OrderHead
    {
        [Key]
        public string ID { get; set; }
        public DateTime BusinessDate { get; set; }
        public string CurrentEmployID { get; set; }
        public string CheckID { get; set; }
        public string CheckNumber { get; set; }
        public string TermId { get; set; }
        public string TableId { get; set; }
        public string IPAddress { get; set; }
        public string PaymentType { get; set; }
        public string RequestType { get; set; }
        public string ErrorMsg { get; set; }
        public string PayCode { get; set; }
        public string StoreName { get; set; }
        public string StoreCode { get; set; }
        public string SubmitAmount { get; set; }
        public string DiscountAmount { get; set; }
        public string PaymentAmount { get; set; }
        public DateTime PosSumbitTime { get; set; }
        public DateTime ServerReceivePosTime { get; set; }
        public DateTime ServerSumbitePaymentTime { get; set; }
        public DateTime ServerReceivePaymentResult { get; set; }
        public DateTime ServerSumbitePaymentResultToPos { get; set; }
        public DateTime PosReceiveTime { get; set; }
        public DateTime CloseCheckTime { get; set; }
        public string Statue { get; set; }
        public string CreateBy { get; set; }
        public string CreateDate { get; set; }
        public string TradeNo { get; set; }
        public string Ext1 { get; set; }
        public string Ext2 { get; set; }
        public string Ext3 { get; set; }
        public string Ext4 { get; set; }
        public string Ext5 { get; set; }
        public string Ext6 { get; set; }
        public string Ext7 { get; set; }
        public string Ext8 { get; set; }
    }
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

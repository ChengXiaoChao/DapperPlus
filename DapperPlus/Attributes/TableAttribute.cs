using System;

namespace DapperPlus.Attributes
{
    /// <summary>
    /// 表名标识
    /// </summary>
    public class TableAttribute : Attribute
    {
        public TableAttribute(string tableName)
        {
            TableName = tableName;
        }
        public string TableName { get; set; }
    }
}

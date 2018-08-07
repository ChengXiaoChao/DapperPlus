using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DapperPlus.Attributes
{
    /// <summary>
    /// 额外元素 将不对该字段进行数据库操作
    /// </summary>
    public class ExtraElementAttribute : Attribute
    {
        public ExtraElementAttribute() { }
    }
}

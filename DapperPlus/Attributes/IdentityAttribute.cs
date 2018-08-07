using System;

namespace DapperPlus.Attributes
{
    /// <summary>
    /// 自增标识
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class IdentityAttribute : Attribute
    {
        public IdentityAttribute() { }
    }
}

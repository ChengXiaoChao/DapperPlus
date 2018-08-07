using System;

namespace DapperPlus.Attributes
{

    /// <summary>
    /// 唯一标识
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class KeyAttribute : Attribute
    {
        public KeyAttribute() { }
    }
}

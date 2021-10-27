namespace LunarBind
{
    using System;

    /// <summary>
    /// Adding this to a type appends a prefix to all LunarBindFunctions contained within
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class LunarBindPrefixAttribute : Attribute
    {
        readonly string prefix;
        public string Prefix => prefix;
        public LunarBindPrefixAttribute(string prefix)
        {
            this.prefix = prefix;
        }
    }

}

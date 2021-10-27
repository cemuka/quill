namespace LunarBind
{
    using System;

    /// <summary>
    /// Adding this to a class makes it a static type accessible by all scripts
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    internal class LunarBindTypeAttribute : Attribute
    {
        readonly string name;
        readonly bool newable;
        public string Prefix => name;
        public bool Newable => newable;
        public LunarBindTypeAttribute(string name, bool newable = false)
        {
            this.name = name;
            this.newable = newable;
        }

        public LunarBindTypeAttribute(bool newable, string name = null)
        {
            this.name = name;
            this.newable = newable;
        }
    }


}

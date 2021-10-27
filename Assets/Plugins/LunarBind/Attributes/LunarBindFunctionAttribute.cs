namespace LunarBind
{
    using System;
    [System.AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class LunarBindFunctionAttribute : Attribute
    {

        readonly string name;
        readonly bool autoYield;

        public string Name => name;

        public bool AutoYield => autoYield;

        public LunarBindFunctionAttribute(string name = null, bool autoYield = true)
        {
            this.name = name;
            this.autoYield = autoYield;
        }
    }
}

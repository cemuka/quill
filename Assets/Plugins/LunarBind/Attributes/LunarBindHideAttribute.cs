namespace LunarBind
{
    using System;
    /// <summary>
    /// Hide from being added
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Struct | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class LunarBindHideAttribute : Attribute
    {
        public LunarBindHideAttribute()
        {
        }
    }
}

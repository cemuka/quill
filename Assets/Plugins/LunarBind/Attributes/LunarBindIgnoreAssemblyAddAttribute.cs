using System;
namespace LunarBind
{
    /// <summary>
    /// Hide class from being added through assembly
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class LunarBindIgnoreAssemblyAddAttribute : Attribute
    {
        public LunarBindIgnoreAssemblyAddAttribute()
        {
        }
    }
}

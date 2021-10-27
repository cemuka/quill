namespace LunarBind
{
    using System;

    /// <summary>
    /// Marks a class that is derived from <see cref="Yielder"/> for assembly binding.
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Struct | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class LunarBindYielderAttribute : Attribute
    {
        public LunarBindYielderAttribute()
        {
            
        }
    }
}
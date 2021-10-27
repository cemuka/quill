using System;
using System.Collections.Generic;
using System.Text;

namespace LunarBind
{
    using System;

    /// <summary>
    /// Use this attribute to mark classes for instantiation
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class LunarBindStaticAttribute : Attribute
    {
        readonly string path;
        public string Path => path;

        /// <summary>
        /// The path this class will be instantiated at. Classes marked by this will be instantiated with their parameterless constructor.
        /// </summary>
        /// <param name="path">Path to add static type reference to. If left null, the type name will be used</param>
        public LunarBindStaticAttribute(string path = null)
        {
            this.path = path;
        }
    }
}

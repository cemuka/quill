namespace LunarBind
{
    using System;
    /// <summary>
    /// Use this attribute to mark classes for instantiation
    /// </summary>
    [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class LunarBindInstanceAttribute : Attribute
    {
        readonly string path;
        public string Path => path;

        /// <summary>
        /// The path this class will be instantiated at. Classes marked by this will be instantiated with their parameterless constructor.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="usePrivateMembers"></param>
        public LunarBindInstanceAttribute(string path)
        {
            this.path = path;
        }
    }


}

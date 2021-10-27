using System;
using System.Collections.Generic;
using System.Text;

namespace LunarBind.Exceptions
{

    /// <summary>
    /// An exception thrown when a standard is not met
    /// </summary>
    [Serializable]
    public class LunarBindStandardException : Exception
    {
        public LunarBindStandardException() { }
        public LunarBindStandardException(string message) : base(message) { }
        public LunarBindStandardException(string message, Exception inner) : base(message, inner) { }
        protected LunarBindStandardException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}

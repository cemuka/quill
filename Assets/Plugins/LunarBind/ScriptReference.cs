using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Text;

namespace LunarBind
{

    /// <summary>
    /// Used for holding a reference to a <see cref="MoonSharp.Interpreter.Script"/> in Lua without adding <see cref="MoonSharp.Interpreter.Script"/> type to userdata
    /// </summary>
    [MoonSharpUserData]
    public class ScriptReference
    {
        [MoonSharpHidden]
        public Script Script;
        [MoonSharpHidden]
        public ScriptReference(Script s)
        {
            Script = s;
        }

        public static implicit operator Script(ScriptReference r)
        {
            return r.Script;
        }
    }
}

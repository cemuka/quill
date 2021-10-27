namespace LunarBind
{
    using MoonSharp.Interpreter;
    using System;
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// The base class of all ScriptRunners. Contains a MoonSharp script
    /// </summary>
    public abstract class ScriptRunnerBase
    {
        public Script Lua { get; protected set; }

        public Guid Guid { get; private set; } = Guid.NewGuid();
    }
}

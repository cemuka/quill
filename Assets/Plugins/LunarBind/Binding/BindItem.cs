namespace LunarBind
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MoonSharp.Interpreter;

    //TODO: implement name set in base constructor
    internal abstract class BindItem
    {
        public string Name { get; internal protected set; }
        public string Example { get; internal set; }
        public string Documentation { get; internal set; }

        //Todo: limit to table and function
        internal string YieldableString { get; set; }
        internal abstract void AddToScript(Script script);
    }
}

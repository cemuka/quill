namespace LunarBind
{
    using MoonSharp.Interpreter;
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class BindUserObject : BindItem
    {
        public object UserObject { get; private set; }

        public BindUserObject(string name, object obj)
        {
            Name = name;
            UserObject = obj;
        }

        internal override void AddToScript(Script script)
        {
            script.Globals[Name] = UserObject;
        }
    }

    internal class BindUserType : BindItem
    {
        public Type UserType { get; private set; }

        public BindUserType(string name, Type type)
        {
            Name = name;
            UserType = type;
        }

        internal override void AddToScript(Script script)
        {
            script.Globals[Name] = UserType;
        }
    }

}

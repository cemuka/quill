namespace LunarBind
{
    using MoonSharp.Interpreter;
    using System;

    internal class BindFunc : BindItem
    {
        /// <summary>
        /// The C# callback function
        /// </summary>
        public Delegate Callback { get; private set; } 

        public bool IsYieldable { get; private set; }

        public int NumParams => Callback.Method.GetParameters().Length;

        internal const string COROUTINE_YIELD_ = "COROUTINE_YIELD_";

        public BindFunc(string name, Delegate callback, bool autoYield = true, string documentation = "", string example = "")
        {
            this.Callback = callback;
            this.Documentation = documentation;
            this.Example = example;
            if (autoYield)
            {
                IsYieldable = typeof(Yielder).IsAssignableFrom(callback.Method.ReturnType);
                if (IsYieldable && GlobalScriptBindings.AutoYield) { Name = COROUTINE_YIELD_ + name; }
                else { Name = name; }
            }
            else
            {
                IsYieldable = false;
                Name = name;
            }
            YieldableString = "";
        }

        public void GenerateYieldableString(string path)
        {
            string argString = "";

            string[] pathSplit = path.Split('.');
            pathSplit[pathSplit.Length - 1] = Name;
            string adjustedPath = string.Join(".", pathSplit);

            int len = Callback.Method.GetParameters().Length;
            for (int j = 0; j < len; j++)
            {
                if (j == 0)
                {
                    argString += "a0";
                }
                else
                {
                    argString += $",a{j}";
                }
            }

            YieldableString = $"{path} = function({argString}) return coroutine.yield({adjustedPath}({argString})) end \r\n";
        }

        //Do not call from BindTable
        internal override void AddToScript(Script script)
        {
            script.Globals[Name] = Callback;// DynValue.FromObject(script, Callback);
            if (IsYieldable && GlobalScriptBindings.AutoYield)
            {
                script.DoString(YieldableString);
            }
        }
    }
}

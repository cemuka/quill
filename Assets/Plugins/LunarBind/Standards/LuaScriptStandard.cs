namespace LunarBind.Standards
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MoonSharp.Interpreter;

    /// <summary>
    /// A class for defining and extracting a standard for lua functions
    /// </summary>
    public class LuaScriptStandard
    {
        public List<LuaFuncStandard> Standards { get; private set; } = new List<LuaFuncStandard>();

        /// <summary>
        /// Use this to initialize a script with custom code
        /// </summary>
        public string CustomInitializerString { get; set; } = null;

        public LuaScriptStandard()
        {
        }

        public LuaScriptStandard(params LuaFuncStandard[] standards)
        {
            if (standards != null)
            {
                Standards.AddRange(standards);
            }
        }

        /// <summary>
        /// Constructor for a single function
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isCoroutine"></param>
        /// <param name="autoResetCoroutine"></param>
        public LuaScriptStandard(string path, bool isCoroutine = false, bool autoResetCoroutine = false)
        {
            Standards.Add(new LuaFuncStandard(path, isCoroutine, autoResetCoroutine));
        }

        public void AddStandard(string path, bool isCoroutine = false, bool autoResetCoroutine = false)
        {
            Standards.Add(new LuaFuncStandard(path, isCoroutine, autoResetCoroutine));
        }

        public void AddStandard(LuaFuncStandard standard)
        {
            Standards.Add(standard);
        }
        public void AddStandards(params LuaFuncStandard[] standards)
        {
            if (standards != null)
            {
                Standards.AddRange(standards);
            }
        }

        public void AddStandards(IEnumerable<LuaFuncStandard> standards)
        {
            if (standards != null)
            {
                Standards.AddRange(standards);
            }
        }

        public void Scrub(HookedScriptContainer container)
        {
            if (container != null)
            {
                foreach (var item in container.ScriptFunctions)
                {
                    item.Value.ScriptRef.Globals.Remove(item.Key);
                }
            }
        }

        public void Scrub(Script script, HookedScriptContainer container)
        {
            if (script != null && container != null)
            {
                foreach (var item in container.ScriptFunctions)
                {
                    script.Globals.Remove(item.Key);
                }
            }
        }

        /// <summary>
        /// Extracts functions from a <see cref="MoonSharp"/> <see cref="Script"/> that are contained in the standard
        /// </summary>
        /// <param name="script"></param>
        /// <param name="errors"></param>
        /// <returns></returns>
        public Dictionary<string, ScriptFunction> ExtractFunctions(Script script, List<string> errors)
        {
            Dictionary<string, ScriptFunction> hooks = new Dictionary<string, ScriptFunction>();
            var g = script.Globals;
            for (int i = 0; i < Standards.Count; i++)
            {
                try
                {
                    var globalItem = g.Get(Standards[i].Path);
                    if (globalItem.Type != DataType.Function)
                    {
                        errors?.Add($"Script contains [{Standards[i].Path}], but it is not a lua function");
                    }
                    else
                    {
                        hooks[Standards[i].Path] = new ScriptFunction(script, globalItem, 
                            !Standards[i].FuncType.HasFlag(LuaFuncType.Function), 
                            Standards[i].FuncType.HasFlag(LuaFuncType.AutoCoroutine));
                    }
                }
                catch
                {
                    errors?.Add($"Script does not contain [{Standards[i].Path}]");
                }
            }
            return hooks;
        }

        /// <summary>
        /// Extracts functions from a <see cref="MoonSharp"/> <see cref="Script"/> that that are contained in the standard as raw DynValues
        /// </summary>
        /// <param name="script">The <see cref="MoonSharp"/> <see cref="Script"/> to extract functions from</param>
        /// <param name="errors">An optional list containing any error messages</param>
        /// <returns></returns>
        public Dictionary<string, DynValue> ExtractFunctionsRaw(Script script, List<string> errors = null)
        {
            Dictionary<string, DynValue> hooks = new Dictionary<string, DynValue>();
            var g = script.Globals;
            for (int i = 0; i < Standards.Count; i++)
            {
                try
                {
                    var globalItem = g.Get(Standards[i].Path);
                    if (globalItem.Type != DataType.Function)
                    {
                        errors?.Add($"Script contains [{Standards[i].Path}], but it is not a lua function");
                    }
                    else
                    {
                        hooks[Standards[i].Path] = globalItem;
                    }
                }
                catch
                {
                    errors?.Add($"Script does not contain [{Standards[i].Path}]");
                } 
            }
            return hooks;
        }

        /// <summary>
        /// Checks if a script container complys with the standard, and automatically creates and adds <see cref="ScriptFunction"/>s to the container
        /// </summary>
        /// <param name="script"></param>
        /// <param name="container"></param>
        /// <param name="messages"></param>
        /// <returns></returns>
        public bool ApplyStandard(Script script, HookedScriptContainer container, List<string> messages = null)
        {
            var g = script.Globals;
            bool ok = true;
            for (int i = 0; i < Standards.Count; i++)
            {
                try
                {
                    //Try to see a manually added one exists
                    var hook = container.GetHook(Standards[i].Path);
                    if (hook == null)
                    {
                        //It was not already added
                        var globalItem = g.Get(Standards[i].Path);
                        if (globalItem.Type == DataType.Nil)
                        {
                            if (Standards[i].Required)
                            {
                                //It is required, add error
                                ok = false;
                                messages?.Add($"Script does not contain [{Standards[i].Path}]");
                            }
                            else
                            {
                                //It is not required, add warning
                                messages?.Add($"WARNING: Script does not contain [{Standards[i].Path}], but it is not required");
                                continue;
                            }
                        }
                        else if (globalItem.Type != DataType.Function)
                        {
                            //It is required, and is not a lua function
                            if (Standards[i].Required)
                            {
                                ok = false;
                                messages?.Add($"Script contains [{Standards[i].Path}], but it is not a lua function");
                            }
                        }
                        else
                        {
                            var funcType = Standards[i].FuncType;
                            if (funcType.HasFlag(LuaFuncType.Function))
                            {
                                container.SetHook(Standards[i].Path, new ScriptFunction(script, globalItem, false));
                            }
                            else if (funcType.HasFlag(LuaFuncType.SingleUseCoroutine))
                            {
                                container.SetHook(Standards[i].Path, new ScriptFunction(script, globalItem, true, false));

                            }
                            else if (funcType.HasFlag(LuaFuncType.AutoCoroutine))
                            {
                                container.SetHook(Standards[i].Path, new ScriptFunction(script, globalItem, true, true));
                            }
                        }

                    }
                    else
                    {
                        var funcType = Standards[i].FuncType;
                        if (funcType.HasFlag(LuaFuncType.AllowAny))
                        {
                            continue;
                        }
                        else if (funcType.HasFlag(LuaFuncType.AllowAnyCoroutine))
                        {
                            if (hook.IsCoroutine) { continue; }
                            else
                            {
                                ok = false;
                                messages?.Add($"User defined hook contains [{Standards[i].Path}], but it is not a coroutine");
                            }
                        }
                        else if (funcType == LuaFuncType.Function)
                        {
                            if (!hook.IsCoroutine) { continue; }
                            else
                            {
                                ok = false;
                                messages?.Add($"User defined hook contains [{Standards[i].Path}], but it is not a normal function");
                            }
                        }
                        else if (funcType == LuaFuncType.SingleUseCoroutine)
                        {
                            if (!hook.IsCoroutine || hook.AutoResetCoroutine)
                            {
                                ok = false;
                                messages?.Add($"User defined hook contains [{Standards[i].Path}], but it is not a single-use coroutine");
                            }
                            else { continue; }
                        }
                        else if (funcType == LuaFuncType.AutoCoroutine)
                        {
                            if (!hook.IsCoroutine || !hook.AutoResetCoroutine)
                            {
                                ok = false;
                                messages?.Add($"User defined hook contains [{Standards[i].Path}], but it is not an automatic coroutine");
                            }
                        }
                    }
                }
                catch
                {
                    ok = false;
                    messages?.Add($"Script does not contain [{Standards[i].Path}]");
                }

                //TODO: Eventually do standard for Tables too  

            }
            return ok;
        }
    }


    public class LuaFuncStandard
    {
        public string Path;
        public bool Required { get; set; }

        internal LuaFuncType FuncType { get; private set; }

        /// <summary>
        /// Creates a <see cref="LuaFuncStandard"/>
        /// </summary>
        /// <param name="path">The global function name. Paths not yet implemented, but this can be bypassed by registering the function manually</param>
        /// <param name="isCoroutine">Is it a coroutine</param>
        /// <param name="autoResetCoroutine">Should the coroutine automatically reset? (create a new coroutine when it dies?)</param>
        /// <param name="required">Does this function have to be implemented?</param>
        public LuaFuncStandard(string path, bool isCoroutine = false, bool autoResetCoroutine = false, bool required = true)
        {
            Path = path;
            Required = required;

            if (isCoroutine)
            {
                if (autoResetCoroutine)
                {
                    FuncType = LuaFuncType.AutoCoroutine;
                }
                else
                {
                    FuncType = LuaFuncType.SingleUseCoroutine;
                }
            }
            else
            {
                FuncType = LuaFuncType.Function;
            }

        }

        public LuaFuncStandard(string path, LuaFuncType type, bool required = true)
        {
            Path = path;
            FuncType = type;
            Required = required;

            if(FuncType.HasFlag(LuaFuncType.AllowAny) && (((int)FuncType & 0b0111) < 1))
            {
                throw new ArgumentException("Type must also contain FuncType.Function, FuncType.SingleUseCoroutine, or FuncType.AutoCoroutine", "type");
            }

            if (FuncType.HasFlag(LuaFuncType.AllowAnyCoroutine) && (((int)FuncType & 0b0110) < 1))
            {
                throw new ArgumentException("Type must also contain FuncType.SingleUseCoroutine or FuncType.AutoCoroutine", "type");
            }
        }

    }

}

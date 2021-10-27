namespace LunarBind
{
    using System;
    using System.Collections.Generic;
    using MoonSharp.Interpreter;
    using LunarBind.Yielding;
    using LunarBind.Standards;

    /// <summary>
    /// Allows multiple script strings sharing one Script object
    /// </summary>
    public class MultiHookedScriptRunner : ScriptRunnerBase
    {
        private readonly Dictionary<string, HookedScriptContainer> GlobalScripts = new Dictionary<string, HookedScriptContainer>();
        private HookedScriptContainer CurrentTempScript = null;

        private HookedScriptContainer runningScript = null;
        public LuaScriptStandard ScriptStandard { get; private set; } = null;
        public MultiHookedScriptRunner()
        {
            Initialize();
        }

        private void Initialize()
        {
            Lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            Lua.Globals["Script"] = new ScriptReference(Lua);
            Lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            Lua.Globals["RegisterCoroutine"] = (Action<DynValue, string, bool>)RegisterCoroutine;
            Lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            Lua.Globals["MakeGlobal"] = (Action<string>)MakeGlobal;
            Lua.Globals["RemoveGlobal"] = (Action<string>)RemoveGlobal;
            Lua.Globals["ResetGlobals"] = (Action)ResetGlobals;

            GlobalScriptBindings.Initialize(Lua);
        }

        public MultiHookedScriptRunner(ScriptBindings bindings, LuaScriptStandard standard = null) : this()
        {
            bindings.Initialize(Lua);
            ScriptStandard = standard;
        }

        public MultiHookedScriptRunner(LuaScriptStandard standard, ScriptBindings bindings = null) : this()
        {
            bindings.Initialize(Lua);
            ScriptStandard = standard;
        }

        public void LoadScript(string scriptString)
        {

            //Remove old hooks so they aren't counted
            if (ScriptStandard != null)
            {
                ScriptStandard.Scrub(Lua, CurrentTempScript);
            }

            CurrentTempScript?.ResetHooks();


            if (string.IsNullOrWhiteSpace(scriptString))
            {
                //No script
                CurrentTempScript = null;
                return;
            }
            var scr = new HookedScriptContainer();
            CurrentTempScript = scr;

            runningScript = scr;
            try
            {
                Lua.DoString(scriptString);

                if (ScriptStandard != null)
                {
                    List<string> errors = new List<string>();
                    bool res = ScriptStandard.ApplyStandard(Lua, scr, errors);
                    if (!res)
                    {
                        throw new Exception($"Script Standard was not met! Standards Not Met: [{string.Join(", ", errors)}]");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                runningScript = null;
            }
        }

        public void LoadGlobalScript(string scriptName, string scriptString)
        {
            scriptString += $"\r\nMakeGlobal(\"{scriptName}\")";
            LoadScript(scriptString);
        }


        #region callbacks

        void RegisterCoroutine(DynValue del, string name, bool autoResetCoroutine = false)
        {
            if (runningScript == null) { return; }
            //var coroutine = Lua.CreateCoroutine(del);
            runningScript.ScriptFunctions[name] = new ScriptFunction(Lua, del, true, autoResetCoroutine);
        }

        void RegisterHook(DynValue del, string name)
        {
            if (runningScript == null) { return; }
            runningScript.ScriptFunctions[name] = new ScriptFunction(Lua, del, false);
        }

        void RemoveHook(string name)
        {
            if (runningScript == null) { return; }
            runningScript.ScriptFunctions.Remove(name);
        }
        void MakeGlobal(string name)
        {
            if (runningScript == null) { return; }
            if (!GlobalScripts.ContainsKey(name))
            {
                //Unique global scripts
                GlobalScripts[name] = runningScript;
            }
            CurrentTempScript = null; //Remove temp so as to not dupe either way
        }
        void RemoveGlobal(string name)
        {
            if (runningScript == null) { return; }
            if (GlobalScripts.ContainsKey(name))
            {
                GlobalScripts[name].ResetHooks();
                GlobalScripts.Remove(name);
            }
        }
        void ResetGlobals()
        {
            if (runningScript == null) { return; }
            foreach (var scr in GlobalScripts)
            {
                scr.Value.ResetHooks();
            }
            GlobalScripts.Clear();
        }

        #endregion

        /// <summary>
        /// Call a hooked lua function.
        /// <para/>
        /// If you want to pass in a single array and want to access it in a single parameter in lua, convert it to a List first: 
        /// <para/>
        /// C#: runner.Execute("func", List&lt;string&gt;);
        /// <para/>
        /// Lua: function func(list)
        /// </summary>
        /// <param name="hookName"></param>
        /// <param name="args"></param>
        public void Execute(string hookName, params object[] args)
        {
            try
            {
                foreach (var script in GlobalScripts.Values)
                {
                    runningScript = script;
                    script.GetHook(hookName)?.Execute(args);
                    //RunLua(script, hookName, args);
                    runningScript = null;
                }

                if (CurrentTempScript != null)
                {
                    runningScript = CurrentTempScript;
                    CurrentTempScript.GetHook(hookName)?.Execute(args);
                    //RunLua(CurrentTempScript, hookName, args);
                    runningScript = null;
                }
            }
            catch (Exception ex)
            {
                if (ex is InterpreterException e)
                {
                    throw new Exception(e.DecoratedMessage);
                }

                throw ex;
            }
            finally
            {
                runningScript = null;
            }
        }

        public void Reset()
        {
            foreach (var scr in GlobalScripts)
            {
                scr.Value.ResetHooks();
            }
            GlobalScripts.Clear();
            CurrentTempScript = null;
            Initialize();
        }

        public void ResetCurrent()
        {
            CurrentTempScript = null;
        }

        //private void RunLua(HookedScriptContainer script, string hookName, params object[] args)
        //{
        //    var hook = script.GetHook(hookName);
        //    if (hook != null)
        //    {
        //        if (hook.IsCoroutine) 
        //        {
        //            if (hook.Coroutine.Coroutine.State == CoroutineState.Dead || !hook.CheckYieldStatus()) //Doesn't run check yield if coroutine is dead
        //            {
        //                return;
        //            }

        //            DynValue ret = hook.Coroutine.Coroutine.Resume(args);

        //            switch (hook.Coroutine.Coroutine.State)
        //            {
        //                case CoroutineState.Suspended:
        //                    if (ret.IsNotNil())
        //                    {
        //                        Yielder yielder = ret.ToObject<Yielder>();
        //                        hook.CurYielder = yielder;
        //                    }
        //                    else
        //                    {
        //                        hook.CurYielder = null;
        //                    }
        //                    break;
        //                case CoroutineState.Dead:
        //                    hook.ResetCoroutine();
        //                    break;
        //                default:
        //                    break;
        //            }
        //        }
        //        else
        //        {
        //             Lua.Call(hook.LuaFunc, args);
        //        }
        //    }
        //}

        public object this[string id]
        {
            get
            {
                return Lua.Globals.Get(id);
            }
            set
            {
                Lua.Globals.Set(id, DynValue.FromObject(Lua, value));
            }
        }

    }
}

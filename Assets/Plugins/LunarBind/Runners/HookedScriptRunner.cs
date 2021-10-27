namespace LunarBind
{
    using MoonSharp.Interpreter;
    using LunarBind.Yielding;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using LunarBind.Standards;
    public class HookedScriptRunner : ScriptRunnerBase
    {
        public string Name { get; set; } = nameof(HookedScriptRunner);

        private readonly HookedScriptContainer scriptContainer = new HookedScriptContainer();
        
        /// <summary>
        /// A standard all loaded scripts must follow
        /// </summary>
        public LuaScriptStandard ScriptStandard { get; private set; } = null;
        public bool AutoResetAllCoroutines { get; set; } = false;

        /// <summary>
        /// Get a reference to the 
        /// </summary>
        public Dictionary<string, ScriptFunction> Functions => scriptContainer.ScriptFunctions;

        public ScriptFunction GetFunction(string name)
        {
            return scriptContainer.GetHook(name);
        }

        public HookedScriptRunner()
        {
            Lua = new Script(CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time);
            Lua.Globals["Script"] = new ScriptReference(Lua);
            Lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            Lua.Globals["RegisterCoroutine"] = (Action<DynValue, string, bool>)RegisterCoroutine;
            Lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            //Global init
            GlobalScriptBindings.Initialize(Lua);
        }

        /// <summary>
        /// Creates a new HookedScriptRunner with the specified modules, throws exception if Coroutines flag is not included
        /// </summary>
        /// <param name="modules"></param>
        public HookedScriptRunner(CoreModules modules)
        {
            if (!modules.HasFlag(CoreModules.Coroutine))
            {
                throw new ArgumentException("Modules must contain the Coroutine Flag to be used in a HookedScriptRunner", "modules");
            }
            Lua = new Script(modules);
            Lua.Globals["Script"] = new ScriptReference(Lua);
            Lua.Globals["RegisterHook"] = (Action<DynValue, string>)RegisterHook;
            Lua.Globals["RegisterCoroutine"] = (Action<DynValue, string, bool>)RegisterCoroutine;
            Lua.Globals["RemoveHook"] = (Action<string>)RemoveHook;
            //Global init
            GlobalScriptBindings.Initialize(Lua);
        }

        public HookedScriptRunner(ScriptBindings bindings, LuaScriptStandard standard = null) : this()
        {
            bindings.Initialize(Lua);
            ScriptStandard = standard;
        }

        public HookedScriptRunner(LuaScriptStandard standard, ScriptBindings bindings = null) : this()
        {
            bindings?.Initialize(Lua);
            ScriptStandard = standard;
        }

        public void AddBindings(ScriptBindings bindings)
        {
            bindings.Initialize(Lua);
        }

        public void SetStandard(LuaScriptStandard standard)
        {
            ScriptStandard = standard;
        }

        public void LoadScript(string scriptString, string scriptName = "User Code")
        {
            //Remove old hooks so they aren't counted
            if(ScriptStandard != null)
            {
                ScriptStandard.Scrub(Lua, scriptContainer);
            }

            scriptContainer.ResetHooks();

            Lua.DoString(scriptString, null, scriptName);
                
            if (ScriptStandard != null)
            {
                List<string> errors = new List<string>();
                bool res = ScriptStandard.ApplyStandard(Lua, scriptContainer, errors);
                if (!res)
                {
                    //Todo: new type of exception with info
                    throw new Exception($"Script Standard was not met! Standards Not Met: [{string.Join(", ", errors)}]");
                }
            }
        }

        public void LoadScriptFile(string scriptFile, string scriptName = null)
        {
            if (ScriptStandard != null)
            {
                ScriptStandard.Scrub(Lua, scriptContainer);
            }

            scriptContainer.ResetHooks();
            Lua.DoFile(scriptFile, null, scriptName);
            
            if (ScriptStandard != null)
            {
                List<string> errors = new List<string>();
                bool res = ScriptStandard.ApplyStandard(Lua, scriptContainer, errors);
                if (!res)
                {
                    //Todo: new type of exception with info
                    throw new Exception($"Script Standard was not met! Standards Not Met: [{string.Join(", ", errors)}]");
                }
            }
        }


        #region callbacks
        void RegisterCoroutine(DynValue del, string name, bool autoReset = false)
        {
            //var coroutine = Lua.CreateCoroutine(del);
            scriptContainer.SetHook(name, new ScriptFunction(Lua, del, true, autoReset));
        }

        void RegisterHook(DynValue del, string name)
        {
            scriptContainer.SetHook(name, new ScriptFunction(Lua, del, false));
        }

        void RemoveHook(string name)
        {
            scriptContainer.RemoveHook(name);
        }
        #endregion

        public void ResetCoroutine(string name)
        {
            var hook = scriptContainer.GetHook(name);
            hook.ResetCoroutine();
        }

        #if LBNETFW
        /// <summary>
        /// Execute the function asynchronously.
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public async System.Threading.Tasks.Task<DynValue> ExecuteAsync(string functionName, params object[] args) {
           return await scriptContainer.GetHook(functionName)?.ExecuteAsync(args);
        }
        #endif

        public DynValue Execute(string functionName, params object[] args)
        {
            return scriptContainer.GetHook(functionName)?.Execute(args);
        }

        public void ExecuteWithCallback(string functionName, Action callback, params object[] args)
        {
            scriptContainer.GetHook(functionName)?.ExecuteWithCallback(callback, args);
        }

        

        public IEnumerator CreateUnityCoroutine(string functionName, params object[] args)
        {
            CoroutineState state = CoroutineState.NotStarted;
            var hook = scriptContainer.GetHook(functionName);
            if (hook == null)
            {
                throw new Exception($"Hooked function {functionName} does not exist on script");
            }
            //Create new hook with coroutine
            hook = new ScriptFunction(hook) { AutoResetCoroutine = false };
            while (state != CoroutineState.Dead)
            {
                state = RunAsUnityCoroutine(hook, null, args);
                yield return null;
            }
        }

        public IEnumerator CreateUnityCoroutine(string functionName, Action callback, params object[] args)
        {
            CoroutineState state = CoroutineState.NotStarted;
            var hook = scriptContainer.GetHook(functionName);
            if(hook == null)
            {
                throw new Exception($"Hook {functionName} does not exist on script");
            }
            hook = new ScriptFunction(hook) { AutoResetCoroutine = false };
            while (state != CoroutineState.Dead)
            {
                state = RunAsUnityCoroutine(hook, callback, args);
                yield return null;
            }
        }

        public T Query<T>(string functionName, params object[] args)
        {
            try
            {
                var ret = scriptContainer.GetHook(functionName)?.Execute(args);
                    //RunLua(scriptContainer, hookName, null, args);
                if (ret != null)
                {
                    return ret.ToObject<T>();
                }
                else
                {
                    return default;
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
        }

        private CoroutineState RunAsUnityCoroutine(ScriptFunction hook, Action callback, object[] args)
        {
            if (hook.IsCoroutine)
            {
                if (hook.Coroutine.Coroutine.State == CoroutineState.Dead)
                {
                    return CoroutineState.Dead;
                }

                if (!hook.CheckYieldStatus())
                {
                    return CoroutineState.Suspended;
                }
                var co = hook.Coroutine.Coroutine;

                //DynValue ret = hook.Coroutine.Coroutine.Resume(args);

                DynValue ret;// = co.Resume(args);

                if (co.State == CoroutineState.NotStarted)
                {
                    ret = co.Resume(args);
                }
                else
                {
                    ret = co.Resume();
                }

                switch (co.State)
                {
                    case CoroutineState.Suspended:

                        if (ret.IsNotNil())
                        {
                            Yielder yielder = ret.ToObject<Yielder>();
                            hook.CurYielder = yielder;
                        }
                        else
                        {
                            hook.CurYielder = null;
                        }
                        return CoroutineState.Suspended;
                    case CoroutineState.Dead:
                        hook.CurYielder = null;
                        callback?.Invoke();
                        if (AutoResetAllCoroutines || hook.AutoResetCoroutine)
                        {
                            hook.ResetCoroutine();
                        }
                        return CoroutineState.Dead;
                    default:
                        break;
                }
                return co.State;
            }
            else
            {
                var ret = Lua.Call(hook.LuaFunc, args);
                callback?.Invoke();
                return CoroutineState.Dead;
            }
        }

        /// <summary>
        /// Access the Global table of the contained script
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoonSharp;
using MoonSharp.Interpreter;
namespace LunarBind
{
    /// <summary>
    /// A basic script runner that binds a script and can only run lua strings
    /// </summary>
    public class BasicScriptRunner : ScriptRunnerBase
    {   
        public Table Globals => Lua.Globals;

        public BasicScriptRunner(CoreModules modules = CoreModules.Preset_HardSandbox | CoreModules.Coroutine | CoreModules.OS_Time) {
            Lua = new Script(modules);
            Lua.Globals["Script"] = new ScriptReference(Lua);
            GlobalScriptBindings.Initialize(Lua);
        }

        public BasicScriptRunner(ScriptBindings bindings) : this()
        {
            bindings.Initialize(Lua);
        }

        public BasicScriptRunner(params Delegate[] dels) : this()
        {
            ScriptBindings b = new ScriptBindings(dels);
            b.Initialize(Lua);
        }

        public BasicScriptRunner(params Action[] actions) : this()
        {
            ScriptBindings b = new ScriptBindings(actions);
            b.Initialize(Lua);
        }


        public void AddBindings(ScriptBindings bindings)
        {
            bindings.Initialize(Lua);
        }

        public void RemoveBindings(ScriptBindings bindings)
        {
            bindings.CleanFunctions(Lua);
        }
        
        public void Run(string script, string scriptName = "User Code")
        {
            try
            {
                Lua.DoString(script, null, scriptName);
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

        public DynValue Query(string script, string scriptName = "User Code")
        {
            try
            {
                return Lua.DoString(script, null, scriptName);
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
        public T Query<T>(string script, string scriptName = "User Code")
        {
            try
            {
                return Lua.DoString(script, null, scriptName).ToObject<T>();
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
    }
}

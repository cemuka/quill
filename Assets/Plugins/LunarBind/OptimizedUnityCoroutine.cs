using MoonSharp.Interpreter;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace LunarBind
{
    /// <summary>
    /// Creating coroutines is very expensive, so this mitigates that issue until a PR is submitted
    /// </summary>
    public class OptimizedUnityCoroutine
    {
        /// <summary>
        /// A reference to the script, is used to create coroutines and execute functions
        /// </summary>
        public Script ScriptRef { get; protected set; }
        /// <summary>
        /// A reference to the original MoonSharp function
        /// </summary>
        public DynValue LuaFunc { get; protected set; }

        /// <summary>
        /// A reference the internally created Coroutine dynval
        /// </summary>
        public DynValue Coroutine { get; protected set; }

        /// <summary>
        /// The current coroutine yielder. When running as a unity coroutine, this field will not be useful
        /// </summary>
        public Yielder CurYielder { get; set; } = null;
        public string ID { get; protected set; } = "Func_" + Guid.NewGuid().ToString().Replace('-', '_');
        public string UnityCoroutineState { get; protected set; }

        private bool started = false;

        private bool initialized = false;

        public OptimizedUnityCoroutine(Script scriptRef, string functionBody, params string[] parameters)
        {
            UnityCoroutineState = $"{ID}_UNITY_COROUTINE_STATE";
            this.ScriptRef = scriptRef;
            ScriptRef.Globals[UnityCoroutineState] = true;
            string nxtV = $"{ID}_NextVals";
            StringBuilder sb = new StringBuilder();
            //1 based lua indices, ugh
            for (int i = 1; i < parameters.Length + 1; i++)
            {
                sb.Append($"{parameters[i - 1]} = {nxtV}[{i}] ");
            }
            string NextParamsStr = sb.ToString();
            string funcString = $"function({string.Join(",", parameters)}) while (true) do{Environment.NewLine}{functionBody}{Environment.NewLine}{UnityCoroutineState} = false " +
                $"local {nxtV} = coroutine.yield() " +
                $"{NextParamsStr} " +
                $"end end";
            LuaFunc = ScriptRef.LoadFunction(funcString);
            Coroutine = ScriptRef.CreateCoroutine(LuaFunc);
        }
        public OptimizedUnityCoroutine(ScriptRunnerBase runner, string functionBody, params string[] parameters)
        {
            UnityCoroutineState = $"{ID}_UNITY_COROUTINE_STATE";
            this.ScriptRef = runner.Lua;
            ScriptRef.Globals[UnityCoroutineState] = true;
            string nxtV = $"{ID}_NextVals";
              StringBuilder sb = new StringBuilder();
            //1 based lua indices, ugh
            for (int i = 1; i < parameters.Length+1; i++)
            {
                sb.Append($"{parameters[i - 1]} = {nxtV}[{i}] ");
            }
            string NextParamsStr = sb.ToString();
            string funcString = $"function({string.Join(",", parameters)}) while (true) do{Environment.NewLine}{functionBody}{Environment.NewLine}{UnityCoroutineState} = false " +
                $"local {nxtV} = coroutine.yield() " +
                $"{NextParamsStr} " +
                $"end end";
            LuaFunc = ScriptRef.LoadFunction(funcString);
            Coroutine = ScriptRef.CreateCoroutine(LuaFunc);
        }

        public bool CheckYieldStatus()
        {
            if (CurYielder != null)
            {
                if (CurYielder.CheckStatus())
                {
                    CurYielder = null;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Returns an IEnumerator of this ScriptFunction that yield returns null every frame. 
        /// If it is a normal function it will call once and yield break.<para/>
        /// The <see cref="Yielder"/> attached to the ScriptFunction is checked every iteration
        /// </summary>
        /// <param name="args">The arguments to be passed into the first call of the coroutine</param>
        /// <returns></returns>
        public IEnumerator AsUnityCoroutine(Action callback, params object[] args)
        {
            ScriptRef.Globals[$"{ID}_UNITY_COROUTINE_STATE"] = true;
            //started = true;
            CoroutineState state = CoroutineState.NotStarted;
            Coroutine co = Coroutine.Coroutine;
            while (state != CoroutineState.Dead)
            {
                state = ExecuteAsRepeatableCoroutineForUnity(co, callback, args);
                yield return null;
            }
        }

        public void ForceFullReset()
        {
            Coroutine = ScriptRef.CreateCoroutine(LuaFunc);
            started = false;
            ScriptRef.Globals[$"{ID}_UNITY_COROUTINE_STATE"] = true;
        }

        protected virtual CoroutineState ExecuteAsRepeatableCoroutineForUnity(Coroutine co, Action callback, object[] args)
        {
            try
            {
                if (!CheckYieldStatus())
                {
                    return CoroutineState.Suspended;
                }
                else
                {
                    //Do coroutine
                    DynValue ret;
                    if (!initialized)
                    {
                        initialized = true;
                        ret = co.Resume(args);
                    }
                    else if(!started)
                    {
                        started = true;
                        //For some reason this is how it needs to be... very cool...
                        ret = co.Resume(new object[] { args });
                    }
                    else
                    {
                        ret = co.Resume();
                    }

                    bool unityCoroutineState = (bool)ScriptRef.Globals[$"{ID}_UNITY_COROUTINE_STATE"];

                    if (unityCoroutineState)
                    {
                        if (ret.IsNotNil())
                        {
                            try
                            {
                                Yielder yielder = ret.ToObject<Yielder>();
                                CurYielder = yielder;
                            }
                            catch
                            {
                                //Throw exception?
                            }
                        }
                        return CoroutineState.Suspended;
                    }
                    else
                    {
                        callback?.Invoke();
                        started = false;
                        return CoroutineState.Dead;
                    }
                }
            }
            catch (Exception ex)
            {
                if (ex is InterpreterException ex2)
                {
                    //For unity 
                    throw new Exception(ex2.DecoratedMessage, ex);
                }
                else
                {
                    throw ex;
                }
            }
        }


    }
}

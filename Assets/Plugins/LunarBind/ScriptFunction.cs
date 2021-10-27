namespace LunarBind
{
    using MoonSharp.Interpreter;
    using LunarBind.Yielding;
    using System;
    using System.Collections;
    using LunarBind.Standards;

    /// <summary>
    /// A class implementing advanced function call and coroutine behaviors
    /// </summary>
    public class ScriptFunction
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
        /// Is it a coroutine?
        /// </summary>
        public bool IsCoroutine { get; protected set; }

        public string ID { get; protected set; } = "Func_" + Guid.NewGuid().ToString().Replace('-', '_');

        public CoroutineState CoroutineState 
        { 
            get
            {
                if (!IsCoroutine)
                {
                    return CoroutineState.Dead;
                }
                else
                {
                    return Coroutine.Coroutine.State;
                }
            } 
        }


        protected bool _autoResetCoroutine;
        /// <summary>
        /// Automatically create a new coroutine when the current one is dead?
        /// </summary>
        public bool AutoResetCoroutine {
            get { return _autoResetCoroutine; }
            set {
                _autoResetCoroutine = value;
                FuncType = StandardHelpers.GetLuaFuncType(IsCoroutine, value);
            }
        }


        /// <summary>
        /// The current coroutine yielder. When running as a unity coroutine, this field will not be useful
        /// </summary>
        public Yielder CurYielder { get; set; } = null;

        public LuaFuncType FuncType { get; protected set; }

        /// <summary>
        /// Creates a script hook from a reference script and a string containing a function
        /// </summary>
        /// <param name="scriptRef">A MoonSharp script to associate this <see cref="ScriptFunction"/> with</param>
        /// <param name="singleFunctionString">an unnamed function string<para/>Example: "function() print('test') end"</param>
        /// <param name="coroutine">is it a coroutine?</param>
        /// <param name="autoreset">should the coroutine reset automatically?</param>
        public ScriptFunction(Script scriptRef, string singleFunctionString, bool coroutine = false, bool autoreset = true)
        {
            this.ScriptRef = scriptRef;
            LuaFunc = scriptRef.LoadFunction(singleFunctionString);
            IsCoroutine = coroutine;
            AutoResetCoroutine = autoreset;
            Coroutine = coroutine ? scriptRef.CreateCoroutine(LuaFunc) : null;
            FuncType = StandardHelpers.GetLuaFuncType(coroutine, autoreset);
        }
        public ScriptFunction(Script scriptRef, string singleFunctionString)
        {
            this.ScriptRef = scriptRef;
            LuaFunc = scriptRef.LoadFunction(singleFunctionString);
            IsCoroutine = false;
            AutoResetCoroutine = false;
            Coroutine = null;
            FuncType = StandardHelpers.GetLuaFuncType(false, false);
        }


        public ScriptFunction(Script scriptRef, DynValue function, bool coroutine = false, bool autoreset = true)
        {
            this.ScriptRef = scriptRef;
            LuaFunc = function;
            IsCoroutine = coroutine;
            AutoResetCoroutine = autoreset;
            Coroutine = coroutine ? scriptRef.CreateCoroutine(LuaFunc) : null;
            FuncType = StandardHelpers.GetLuaFuncType(coroutine, autoreset);
        }

        public ScriptFunction(Script scriptRef, DynValue function, LuaFuncType type)
        {
            this.ScriptRef = scriptRef;
            LuaFunc = function;
            IsCoroutine = !type.HasFlag(LuaFuncType.Function);
            AutoResetCoroutine = type.HasFlag(LuaFuncType.AutoCoroutine);
            Coroutine = IsCoroutine ? scriptRef.CreateCoroutine(LuaFunc) : null;
            FuncType = type;
        }


        /// <summary>
        /// Clone the other scripthook. Also creates a new coroutine with the same settings as the original
        /// </summary>
        /// <param name="other"></param>
        public ScriptFunction(ScriptFunction other)
        {
            this.ScriptRef = other.ScriptRef;
            LuaFunc = other.LuaFunc;
            IsCoroutine = other.IsCoroutine;
            AutoResetCoroutine = other.AutoResetCoroutine;
            Coroutine = IsCoroutine ? ScriptRef.CreateCoroutine(LuaFunc) : null;
            if (IsCoroutine) { Coroutine.Coroutine.AutoYieldCounter = other.Coroutine.Coroutine.AutoYieldCounter; }
            FuncType = other.FuncType;
        }


        /// <summary>
        /// Creates a script hook from a function from the script's global table
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="scriptRef"></param>
        /// <param name="coroutine"></param>
        /// <param name="autoreset"></param>
        public ScriptFunction(string funcName, Script scriptRef, bool coroutine = false, bool autoreset = true)
        {
            this.ScriptRef = scriptRef;
            LuaFunc = scriptRef.Globals.Get(funcName);
            if(LuaFunc.Type != DataType.Function)
            {
                throw new Exception($"Global key [{funcName}] was not a lua function");
            }
            IsCoroutine = coroutine;
            AutoResetCoroutine = autoreset;
            Coroutine = coroutine ? scriptRef.CreateCoroutine(LuaFunc) : null;
        }

        //public ScriptFunction(Script scriptRef, DynValue del, DynValue coroutine, bool autoResetCoroutine = false)
        //{
        //    this.ScriptRef = scriptRef;
        //    IsCoroutine = coroutine != null;
        //    LuaFunc = del;
        //    Coroutine = coroutine;
        //    AutoResetCoroutine = autoResetCoroutine;
        //}

        ~ScriptFunction()
        {
            CurYielder = null;
            LuaFunc = null;
            Coroutine = null;
            ScriptRef = null;
        }

        public bool CheckYieldStatus()
        {
            if(CurYielder != null)
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

        public void SetYielder(Yielder yielder)
        {
            CurYielder = yielder;
        }

        public virtual void ResetCoroutine()
        {
            if (IsCoroutine)
            {
                CurYielder = null;
                //It is expensive to create coroutines, also DynVal.Assign() function is more expensive than just assigning in C#
                if (CoroutineState != CoroutineState.NotStarted)
                {
                    Coroutine = ScriptRef.CreateCoroutine(LuaFunc);
                }
            }
        }

        #if LBNETFW
        public async System.Threading.Tasks.Task<DynValue> ExecuteAsync(params object[] args)
        {
            if (IsCoroutine)
            {
                var co = Coroutine.Coroutine;
                if (co.State == CoroutineState.Dead || !CheckYieldStatus()) //Doesn't run check yield if coroutine is dead
                {
                    return DynValue.Nil;
                }
                DynValue ret;

                if (co.State == CoroutineState.NotStarted)
                {
                    ret = await co.ResumeAsync(args);
                }
                else
                {
                    ret = await co.ResumeAsync();
                }

                switch (co.State)
                {
                    case CoroutineState.Suspended:
                        if (ret.IsNotNil())
                        {
                            try
                            {
                                CurYielder = ret.ToObject<Yielder>();
                            }
                            catch
                            {
                                //TODO: throw error?
                            }
                        }
                        break;
                    case CoroutineState.Dead:
                        CurYielder = null;
                        if (AutoResetCoroutine)
                        {
                            //Create new coroutine, assign it to our dynvalue
                            Coroutine = ScriptRef.CreateCoroutine(LuaFunc);
                        }
                        break;
                    default:
                        break;
                }
                return ret;
            }
            else
            {
                //Not coroutine, just call the function
                var ret = await ScriptRef.CallAsync(LuaFunc, args);
                return ret;
            }
        }
    #endif
        public T Query<T>(params object[] args)
        {
            return Execute(args).ToObject<T>();
        }
    #if LBNETFW
        public dynamic DynamicQuery(params object[] args)
        {
            return Execute(args).ToDynamic();
        }
    #endif


        /// <summary>
        /// Executes this function whether it is a normal function or a coroutine. Dead coroutines do not run and return DynValue.Nil
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public DynValue Execute(params object[] args)
        {
            try
            {
                if (IsCoroutine)
                {
                    var co = Coroutine.Coroutine;
                    if (co.State == CoroutineState.Dead || !CheckYieldStatus()) //Doesn't run check yield if coroutine is dead
                    {
                        return DynValue.Nil;
                    }
                    DynValue ret = co.Resume(args);
                    //if (co.State == CoroutineState.NotStarted)
                    //{
                    //    ret = co.Resume(args);
                    //}
                    //else
                    //{
                    //    ret = co.Resume();
                    //}

                    switch (co.State)
                    {
                        case CoroutineState.Suspended:
                            if (ret.IsNotNil())
                            {
                                try
                                {
                                    CurYielder = ret.ToObject<Yielder>();
                                }
                                catch //Todo: catch specific exception
                                {
                                    //Moonsharp does not have a good way of checking the userdata type
                                    //The way to check just throws an error anyways, so this is more efficient
                                }
                            }
                            break;
                        case CoroutineState.Dead:
                            CurYielder = null;
                            if (AutoResetCoroutine)
                            {
                                //Create new coroutine, assign it to our dynvalue
                                Coroutine = ScriptRef.CreateCoroutine(LuaFunc);
                            }
                            break;
                        default:
                            break;
                    }
                    return ret;
                }
                else
                {
                    //Not coroutine, just call the function
                    var ret = ScriptRef.Call(LuaFunc, args);
                    return ret;
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

        /// <summary>
        /// Executes this function with a callback. The callback will be called when the coroutine dies, or every time if it is a normal function
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public DynValue ExecuteWithCallback(Action callback, params object[] args)
        {
            if (IsCoroutine)
            {
                if (Coroutine.Coroutine.State == CoroutineState.Dead || !CheckYieldStatus()) //Doesn't run check yield if coroutine is dead
                {
                    return null;
                }
                DynValue ret = Coroutine.Coroutine.Resume(args);
                switch (Coroutine.Coroutine.State)
                {
                    case CoroutineState.Suspended:

                        if (ret.IsNotNil())
                        {
                            CurYielder = ret.ToObject<Yielder>();
                        }
                        else
                        {
                            CurYielder = null;
                        }
                        break;
                    case CoroutineState.Dead:
                        CurYielder = null;
                        callback?.Invoke();
                        if (AutoResetCoroutine)
                        {
                            Coroutine = ScriptRef.CreateCoroutine(LuaFunc);
                        }
                        break;
                    default:
                        break;
                }
                return ret;
            }
            else
            {
                var ret = ScriptRef.Call(LuaFunc, args);
                callback?.Invoke();
                return ret;
            }
        }

        /// <summary>
        /// Executes, returning the coroutine state. You may call <see cref="Execute(object[])"/> if you do not care whether it is a coroutine or not
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public CoroutineState ExecuteAsCoroutine(params object[] args)
        {
            Coroutine co = Coroutine.Coroutine;
            if (co.State == CoroutineState.Dead)
            {
                return CoroutineState.Dead;
            }
            else if (!CheckYieldStatus())
            {
                return CoroutineState.Suspended;
            }
            else
            {
                //Do coroutine
                DynValue ret = co.Resume(args);
                switch (co.State)
                {
                    case CoroutineState.Suspended:

                        if (ret.IsNotNil())
                        {
                            
                            try
                            {
                                Yielder yielder = ret.ToObject<Yielder>();
                                CurYielder = yielder;
                            }
                            catch //TODO: specific catches to ignore
                            {
                                //Moonsharp does not have a good way of testing the user data type without throwing errors so we have to use try/catch
                            }
                        }
                        return CoroutineState.Suspended;
                    case CoroutineState.Dead:
                        if (AutoResetCoroutine)
                        {
                            ResetCoroutine();
                        }
                        return CoroutineState.Dead;
                    default:
                        //ForceSuspended, Running, etc
                        return co.State;
                }
            }
        }




        /// <summary>
        /// Returns a dynvalue result every call. Does not support <see cref="LunarBind"/>'s extended coroutine functionality, except for auto reset<para/>
        /// Sets returnValue to null if coroutine is Dead 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="returnValue"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public CoroutineState QueryAsCoroutine(out DynValue returnValue, params object[] args)
        {
            Coroutine co = Coroutine.Coroutine;
            if (co.State == CoroutineState.Dead)
            {
                returnValue = default;
                return CoroutineState.Dead;
            }
            else
            {
                //Do coroutine
                DynValue ret = co.Resume(args);
                switch (co.State)
                {
                    case CoroutineState.Suspended:
                        returnValue = ret;
                        return CoroutineState.Suspended;
                    case CoroutineState.Dead:
                        if (AutoResetCoroutine)
                        {
                            ResetCoroutine();
                        }
                        returnValue = ret;
                        return CoroutineState.Dead;
                    default:
                        returnValue = default;
                        return co.State;
                }
            }
        }

        /// <summary>
        /// Returns a result cast to type T every call. Does not support <see cref="LunarBind"/>'s extended coroutine functionality, except for auto reset<para/>
        /// Sets returnValue to default(T) if coroutine is Dead 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="returnValue"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public CoroutineState QueryAsCoroutine<T>(out T returnValue, params object[] args)
        {
            Coroutine co = Coroutine.Coroutine;
            if (co.State == CoroutineState.Dead)
            {
                returnValue = default;
                return CoroutineState.Dead;
            }
            else
            {
                //Do coroutine
                DynValue ret = co.Resume(args);

                switch (co.State)
                {
                    case CoroutineState.Suspended:
                        if (ret.IsNotNil())
                        {
                            //Todo: try catch?
                            returnValue = ret.ToObject<T>();
                        }
                        else
                        {
                            returnValue = default;
                        }
                        return CoroutineState.Suspended;
                    case CoroutineState.Dead:
                        if (AutoResetCoroutine)
                        {
                            ResetCoroutine();
                        }

                        if (ret.IsNotNil())
                        {
                            //Todo: try catch?
                            returnValue = ret.ToObject<T>();
                        }
                        else
                        {
                            returnValue = default;
                        }
                        return CoroutineState.Dead;
                    default:
                        returnValue = default;
                        return co.State;
                }
            }
        }

        /// <summary>
        /// Returns an IEnumerator with a clone of this ScriptFunction that yield returns null every frame. 
        /// If it is a normal function it will call once and yield break.<para/>
        /// You cannot check the <see cref="Yielder"/> of this <see cref="ScriptFunction"/> instance if you use this function<para/>
        /// The <see cref="Yielder"/> attached to the ScriptFunction is checked every iteration
        /// </summary>
        /// <param name="args">The arguments to be passed into the first call of the coroutine</param>
        /// <returns></returns>
        public IEnumerator CloneUnityCoroutine(params object[] args)
        {
            if (!IsCoroutine)
            {
                Execute(args);
                yield break;
            }
            else
            {
                CoroutineState state = CoroutineState.NotStarted;
                var func = new ScriptFunction(this);
                func._autoResetCoroutine = false;
                func.FuncType = LuaFuncType.SingleUseCoroutine;
                Coroutine co = func.Coroutine.Coroutine;
                while (state != CoroutineState.Dead)
                {
                    state = ExecuteAsCoroutineForUnity(co, null, args);
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Returns an IEnumerator with a clone of this ScriptFunction that yield returns null every frame. 
        /// If it is a normal function it will call once and yield break.<para/>
        /// You cannot check the <see cref="Yielder"/> of this <see cref="ScriptFunction"/> instance if you use this function<para/>
        /// The <see cref="Yielder"/> attached to the ScriptFunction is checked every iteration
        /// </summary>
        /// <param name="callback">The action to be called when the coroutine is completed</param>
        /// <param name="args">The arguments to be passed into the first call of the coroutine</param>
        /// <returns></returns>
        public IEnumerator CloneUnityCoroutine(Action callback, params object[] args)
        {
            if (!IsCoroutine)
            {
                Execute(args);
                callback.Invoke();
                yield break;
            }
            else
            {
                CoroutineState state = CoroutineState.NotStarted;
                var func = new ScriptFunction(this);
                func._autoResetCoroutine = false;
                func.FuncType = LuaFuncType.SingleUseCoroutine;
                //Pull the coroutine reference first so we don't convert every time
                Coroutine co = func.Coroutine.Coroutine;
                while (state != CoroutineState.Dead)
                {
                    state = ExecuteAsCoroutineForUnity(co, callback, args);
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Returns an IEnumerator of this ScriptFunction that yield returns null every frame. 
        /// If it is a normal function it will call once and yield break.<para/>
        /// The <see cref="Yielder"/> attached to the ScriptFunction is checked every iteration
        /// </summary>
        /// <param name="args">The arguments to be passed into the first call of the coroutine</param>
        /// <returns></returns>
        public IEnumerator AsUnityCoroutine(params object[] args)
        {
            if (!IsCoroutine)
            {
                Execute(args);
                yield break;
            }
            else
            {
                CoroutineState state = CoroutineState.NotStarted;
                //var func = new ScriptFunction(this);
                //func._autoResetCoroutine = false;
                //func.FuncType = LuaFuncType.SingleUseCoroutine;
                this.ResetCoroutine();
                Coroutine co = Coroutine.Coroutine;
                while (state != CoroutineState.Dead)
                {
                    state = ExecuteAsCoroutineForUnity(co, null, args);
                    yield return null;
                }
            }
        }

        /// <summary>
        /// Returns an IEnumerator with a clone of this ScriptFunction that yield returns null every frame. 
        /// If it is a normal function it will call once and yield break.<para/>
        /// The <see cref="Yielder"/> attached to the ScriptFunction is checked every iteration
        /// </summary>
        /// <param name="callback">The action to be called when the coroutine is completed</param>
        /// <param name="args">The arguments to be passed into the first call of the coroutine</param>
        /// <returns></returns>
        public IEnumerator AsUnityCoroutine(Action callback, params object[] args)
        {
            if (!IsCoroutine)
            {
                Execute(args);
                callback.Invoke();
                yield break;
            }
            else
            {
                CoroutineState state = CoroutineState.NotStarted;
                var func = new ScriptFunction(this);
                func._autoResetCoroutine = false;
                func.FuncType = LuaFuncType.SingleUseCoroutine;
                //Pull the coroutine reference first so we don't convert every time
                Coroutine co = func.Coroutine.Coroutine;
                while (state != CoroutineState.Dead)
                {
                    state = ExecuteAsCoroutineForUnity(co, callback, args);
                    yield return null;
                }
            }
        }

        protected virtual CoroutineState ExecuteAsCoroutineForUnity(Coroutine co, Action callback, object[] args)
        {
            try
            {
                if (co.State == CoroutineState.Dead)
                {
                    return CoroutineState.Dead;
                }
                else if (!CheckYieldStatus())
                {
                    return CoroutineState.Suspended;
                }
                else
                {
                    //Do coroutine
                    DynValue ret;
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
                                try
                                {
                                    Yielder yielder = ret.ToObject<Yielder>();
                                    CurYielder = yielder;
                                }
                                catch
                                {

                                }
                            }
                            return CoroutineState.Suspended;
                        case CoroutineState.Dead:
                            callback?.Invoke();
                            if (AutoResetCoroutine)
                            {
                                ResetCoroutine();
                            }
                            return CoroutineState.Dead;
                        default:
                            //ForceSuspended, Running, etc
                            return co.State;
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

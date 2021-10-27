namespace LunarBind
{
    using MoonSharp.Interpreter;
    using MoonSharp.Interpreter.Interop;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    public class ScriptBindings
    {
        internal readonly Dictionary<string, BindItem> bindItems = new Dictionary<string, BindItem>();
        internal readonly Dictionary<string, Type> yieldableTypes = new Dictionary<string, Type>();
        internal readonly Dictionary<string, Type> newableTypes = new Dictionary<string, Type>();
        //private readonly Dictionary<string, Type> staticTypes = new Dictionary<string, Type>();

        private string bakedNewableTypeString = null;
        private string bakedYieldableTypeString = null;

        /// <summary>
        /// Use this to initialize any scripts initialized with this binder with custom Lua code. This runs after all bindings have been set up
        /// </summary>
        public string CustomInitializerString { get; set; } = null;

        /// <summary>
        /// Use this to initialize any scripts initialized with this binder with custom Lua code. This runs before any global bindings have been set up. (useful for modules, etc)
        /// </summary>
        public string CustomPreInitializerString { get; set; } = null;

        public ScriptBindings()
        {

        }

        public ScriptBindings(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAssemblyTypes(assembly);
            }
        }

        public ScriptBindings(params Type[] types)
        {
            foreach (var type in types)
            {
                RegisterTypeFuncs(type);
            }
        }

        public ScriptBindings(params object[] objs)
        {
            foreach (var obj in objs)
            {
                RegisterObjectFuncs(obj);
            }
        }

        public ScriptBindings(params Action[] actions)
        {
            BindActions(actions);
        }

        public ScriptBindings(params Delegate[] dels)
        {
            BindDelegates(dels);
        }

        /// <summary>
        /// Allows you to access static functions and members on the type by using the Lua global with the name<para/>
        /// Equivalent to script.Globals[t.Name] = t
        /// </summary>
        /// <param name="t"></param>
        public void AddGlobalType(Type t)
        {
            RegisterUserDataType(t);
            BindingHelpers.CreateBindType(bindItems, t.Name, t);
        }

        /// <summary>
        /// Allows you to access static functions and members on the type by using the Lua global with the name<para/>
        /// <para/>
        /// Equivalent to script.Globals[name] = t
        /// </summary>
        /// <param name="name"></param>
        /// <param name="t"></param>
        public void AddGlobalType(string name, Type t)
        {
            RegisterUserDataType(t);
            BindingHelpers.CreateBindType(bindItems, name, t);
        }

        public void AddGlobalObject(string path, object o)
        {
            if (!(o.GetType() == typeof(string) || o.GetType().IsPrimitive))
            {
                RegisterUserDataType(o.GetType());
            }
            BindingHelpers.CreateBindUserObject(bindItems, path, o);
            //globalObjects[name]
        }

        /// <summary>
        /// Adds an enum to a lua table named typeof(T).Name
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddEnum<T>() where T : Enum
        {
            BindingHelpers.CreateBindEnum(bindItems, typeof(T).Name, typeof(T));
        }
        /// <summary>
        /// Adds all fields of an enum not marked with <see cref="MoonSharpHiddenAttribute"/> or <see cref="LunarBindHideAttribute"/> to a lua table with the path
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public void AddEnum<T>(string path)
        {
            BindingHelpers.CreateBindEnum(bindItems, path, typeof(T));
        }

        /// <summary>
        /// Use <see cref="BindTypeFuncs(Type[])"/> instead
        /// </summary>
        /// <param name="types"></param>
        [Obsolete]
        public void AddTypes(params Type[] types)
        {
            foreach (var type in types)
            {
                RegisterTypeFuncs(type);
            }
        }

        /// <summary>
        /// Bind all static functions with the [<see cref="LunarBindFunctionAttribute"/>] attribute on each type
        /// </summary>
        /// <param name="types"></param>
        public void BindTypeFuncs(params Type[] types)
        {
            foreach (var type in types)
            {
                RegisterTypeFuncs(type);
            }
        }

        /// <summary>
        /// Adds all functions under a path
        /// </summary>
        /// <param name="pathPrefix">The path</param>
        /// <param name="types"></param>
        public void BindTypeFuncs(string pathPrefix, params Type[] types)
        {
            foreach (var type in types)
            {
                RegisterTypeFuncs(type, pathPrefix);
            }
        }

        /// <summary>
        /// Use <see cref="BindInstanceFuncs(object[])"/>
        /// </summary>
        /// <param name="objs"></param>
        [Obsolete]
        public void AddObjects(params object[] objs)
        {
            foreach (var obj in objs)
            {
                RegisterObjectFuncs(obj);
            }
        }
        /// <summary>
        /// Use <see cref="BindInstanceFuncs(object[])"/>
        /// </summary>
        /// <param name="objs"></param>
        [Obsolete]
        public void AddObjects<T>(params T[] objs)
        {
            foreach (var obj in objs)
            {
                RegisterObjectFuncs(obj);
            }
        }

        /// <summary>
        /// Bind all instance functions with the [<see cref="LunarBindFunctionAttribute"/>] attribute on each object, using that object as the instance.<para/>
        /// Recommended to use <see cref="AddGlobalObject(string, object)"/> instead
        /// </summary>
        /// <param name="objs"></param>
        public void BindInstanceFuncs(params object[] objs)
        {
            foreach (var obj in objs)
            {
                RegisterObjectFuncs(obj);
            }
        }

        /// <summary>
        /// Bind all instance functions with the [<see cref="LunarBindFunctionAttribute"/>] attribute on the object, using that object as the instance.<para/>
        /// Recommended to use <see cref="AddGlobalObject(string, object)"/> instead
        /// </summary>
        /// <param name="objs"></param>
        public void AddObject(object obj)
        {
            RegisterObjectFuncs(obj);
        }

        /// <summary>
        /// Use <see cref="BindAction(string, Action, string, string)"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        [Obsolete]
        public void AddAction(string name, Action action, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, name, action, false, documentation ?? "", example ?? "");
        }
        /// <summary>
        /// Use <see cref="BindAction(Action, string, string)"/> instead
        /// </summary>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        [Obsolete]
        public void AddAction(Action action, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, action.Method.Name, action, false, documentation ?? "", example ?? "");
        }

        /// <summary>
        /// Add a specific <see cref="Action"/> to the bindings.
        /// </summary>
        /// <param name="path">The path to bind to. Can contain "."</param>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public void BindAction(string path, Action action, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, path, action, false, documentation ?? "", example ?? "");
        }

        /// <summary>
        /// Add a specific <see cref="Action"/> to the bindings, using the method's Name as the name
        /// </summary>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public void BindAction(Action action, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, action.Method.Name, action, false, documentation ?? "", example ?? "");
        }

        /// <summary>
        /// Use <see cref="BindActions(Action[])"/> instead
        /// </summary>
        /// <param name="actions"></param>
        [Obsolete]
        public void AddActions(params Action[] actions)
        {
            foreach (var action in actions)
            {
                BindingHelpers.CreateBindFunction(bindItems, action.Method.Name, action, false, "", "");
            }
        }

        /// <summary>
        /// Add specific <see cref="Action"/>s to the bindings, using the method's Name as the name for each. Paths not supported
        /// </summary>
        /// <param name="actions"></param>
        public void BindActions(params Action[] actions)
        {
            foreach (var action in actions)
            {
                BindingHelpers.CreateBindFunction(bindItems, action.Method.Name, action, false, "", "");
            }
        }

        /// <summary>
        /// Use <see cref="BindDelegate(string, Delegate, string, string)"/> instead
        /// </summary>
        /// <param name="name"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        [Obsolete]
        public void AddDelegate(string name, Delegate del, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, name, del, false, documentation ?? "", example ?? "");
        }

        /// <summary>
        /// Use <see cref="BindDelegate(Delegate, string, string)"/> instead
        /// </summary>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        [Obsolete]
        public void AddDelegate(Delegate del, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, del.Method.Name, del, false, documentation ?? "", example ?? "");
        }

        /// <summary>
        /// Add a specific <see cref="Delegate"/> to the bindings
        /// </summary>
        /// <param name="path"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public void BindDelegate(string path, Delegate del, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, path, del, false, documentation ?? "", example ?? "");
        }

        /// <summary>
        /// Add a specific <see cref="Delegate"/> to the bindings using its Name as the name
        /// </summary>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public void BindDelegate(Delegate del, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, del.Method.Name, del, false, documentation ?? "", example ?? "");
        }


        /// <summary>
        /// Use <see cref="BindDelegates(Delegate[])"/> instead
        /// </summary>
        /// <param name="name"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        [Obsolete]
        public void AddDelegates(params Delegate[] dels)
        {
            foreach (var del in dels)
            {
                BindingHelpers.CreateBindFunction(bindItems, del.Method.Name, del, false, "", "");
            }
        }

        /// <summary>
        /// Add specific <see cref="Delegate"/>s to the bindings using the method Name as the name. Paths not supported
        /// </summary>
        /// <param name="name"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public void BindDelegates(params Delegate[] dels)
        {
            foreach (var del in dels)
            {
                BindingHelpers.CreateBindFunction(bindItems, del.Method.Name, del, false, "", "");
            }
        }


        /// <summary>
        /// Use <see cref="BindAssembly(Assembly[])"/> instead
        /// </summary>
        /// <param name="assemblies"></param>
        [Obsolete]
        public void AddAssemblies(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAssemblyTypes(assembly);
                //UserData.RegisterAssembly(assembly);
            }
        }

        /// <summary>
        ///  Bind all static functions with the [<see cref="LunarBindFunctionAttribute"/>] attribute on each type in each assembly
        /// </summary>
        /// <param name="assemblies"></param>
        public void BindAssembly(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAssemblyTypes(assembly);
            }
        }

        /// <summary>
        ///  Bind all static functions with the [<see cref="LunarBindFunctionAttribute"/>] attribute on each type in each assembly
        /// </summary>
        /// <param name="assemblies"></param>
        public void BindAssemblyFuncs(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAssemblyTypes(assembly);
                //UserData.RegisterAssembly(assembly);
            }
        }

        private void RegisterObjectFuncs(object target)
        {
            if (target == null) { throw new ArgumentNullException(nameof(target)); }
            Type type = target.GetType();
            MethodInfo[] mis = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var mi in mis)
            {
                var attr = (LunarBindFunctionAttribute)Attribute.GetCustomAttribute(mi, typeof(LunarBindFunctionAttribute));
                if (attr != null)
                {
                    var documentation = (LunarBindDocumentationAttribute)Attribute.GetCustomAttribute(mi, typeof(LunarBindDocumentationAttribute));
                    var example = (LunarBindExampleAttribute)Attribute.GetCustomAttribute(mi, typeof(LunarBindExampleAttribute));
                    var del = BindingHelpers.CreateDelegate(mi, target);
                    string name = attr.Name ?? mi.Name;
                    BindingHelpers.CreateBindFunction(bindItems, name, del,attr.AutoYield, documentation?.Data ?? "", example?.Data ?? "");
                }
            }
        }

        private void RegisterTypeFuncs(Type type, string prefix = null)
        {
            if (prefix != null) prefix = prefix.Trim('.', ' ');
            MethodInfo[] mis = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var mi in mis)
            {
                var attr = (LunarBindFunctionAttribute)Attribute.GetCustomAttribute(mi, typeof(LunarBindFunctionAttribute));
                if (attr != null)
                {
                    var prefixAttrib = (LunarBindPrefixAttribute)type.GetCustomAttribute(typeof(LunarBindPrefixAttribute));
                    var documentation = (LunarBindDocumentationAttribute)Attribute.GetCustomAttribute(mi, typeof(LunarBindDocumentationAttribute));
                    var example = (LunarBindExampleAttribute)Attribute.GetCustomAttribute(mi, typeof(LunarBindExampleAttribute));
                    var del = BindingHelpers.CreateDelegate(mi);
                    //string name = (prefix != null ? prefix + "." : "") + (attr.Name ?? mi.Name);
                    string name = $"{(prefixAttrib?.Prefix != null ? prefixAttrib.Prefix + "." : "")}{(prefix != null ? prefix + "." : "")}{(attr.Name ?? mi.Name)}";

                    BindingHelpers.CreateBindFunction(bindItems, name, del, attr.AutoYield, documentation?.Data ?? "", example?.Data ?? "");
                }
            }
        }

        private void RegisterAssemblyTypes(Assembly assembly)
        {
            var types = assembly.GetTypes().Where(x => x.GetCustomAttribute<LunarBindHideAttribute>() == null && x.GetCustomAttribute<LunarBindIgnoreAssemblyAddAttribute>() == null);
            foreach (var type in types)
            {
                if (type.IsEnum)
                {
                    
                    var enumAttr = (LunarBindEnumAttribute)type.GetCustomAttribute(typeof(LunarBindEnumAttribute));
                    if (enumAttr != null)
                    {
                        BindingHelpers.CreateBindEnum(bindItems, enumAttr.Name ?? type.Name, type);
                    }
                }
                else
                {
                    var instantiable = (LunarBindInstanceAttribute)type.GetCustomAttribute(typeof(LunarBindInstanceAttribute));
                    if (instantiable != null)
                    {
                        var constructor = type.GetConstructor(new Type[] { });
                        if (constructor != null)
                        {
                            object instance = constructor.Invoke(new object[] { });
                            RegisterUserDataType(type);
                            var bindObj = BindingHelpers.CreateBindUserObject(bindItems, instantiable.Path, instance);
                            var doc = type.GetCustomAttribute<LunarBindDocumentationAttribute>()?.Data ?? "";
                            var ex = type.GetCustomAttribute<LunarBindExampleAttribute>()?.Data ?? "";
                            bindObj.Documentation = doc;
                            bindObj.Example = ex;


                            //AddGlobalObject(instantiable.Path, instance);
                        }
                        else
                        {
                            //TODO: custom exception
                            throw new Exception($"LunarBind: No public empty constructor found on Type [{type.Name}] with LunarBindInstantiableAttribute");
                        }
                    }

                    var staticAttribute = (LunarBindStaticAttribute)type.GetCustomAttribute(typeof(LunarBindStaticAttribute));
                    if (staticAttribute != null)
                    {
                        RegisterUserDataType(type);
                        BindingHelpers.CreateBindType(bindItems, staticAttribute.Path ?? type.Name, type);
                    }

                    RegisterTypeFuncs(type);
                }
            }
        }
       
        public static void RegisterUserDataType(Type t)
        {
            if (!UserData.IsTypeRegistered(t))
            {
                UserData.RegisterType(t);
            }
        }

        public void AddYieldableType<T>(string name = null) where T : Yielder
        {
            if (name == null) { name = typeof(T).Name; }
            RegisterUserDataType(typeof(T));
            yieldableTypes[name] = typeof(T);
            BakeYieldables();
        }

        /// <summary>
        /// Also allows you to access static functions on the type by using _TypeName
        /// </summary>
        /// <param name="name"></param>
        /// <param name="t"></param>
        public void AddNewableType(string name, Type t)
        {
            RegisterUserDataType(t);
            newableTypes[name] = t;
            BakeNewables();
        }

        public void AddNewableType(Type t)
        {
            RegisterUserDataType(t);
            newableTypes[t.Name] = t;
            BakeNewables();
        }

        public void RemoveNewableType(string name)
        {
            newableTypes.Remove(name);
            BakeNewables();
        }


        public void InitializeRunner(ScriptRunnerBase scriptRunner)
        {
            Initialize(scriptRunner.Lua);
        }

        /// <summary>
        /// Exposed initialize function to initialize non-scriptcore moonsharp scripts
        /// </summary>
        /// <param name="lua"></param>
        public void Initialize(Script lua)
        {
            if (CustomPreInitializerString != null)
            {
                lua.DoString(CustomPreInitializerString);
            }

            foreach (var item in bindItems.Values)
            {
                item.AddToScript(lua);
            }
            //foreach (var type in staticTypes)
            //{
            //    lua.Globals[type.Key] = type.Value;
            //}

            InitializeNewables(lua);
            InitializeYieldables(lua);

            if (CustomInitializerString != null)
            {
                lua.DoString(CustomInitializerString);
            }
        }

        private static string Bake(Dictionary<string, Type> source)
        {
            StringBuilder s = new StringBuilder();

            foreach (var type in source)
            {
                string typeName = type.Key;
                string newFuncName = type.Key;
                HashSet<int> paramCounts = new HashSet<int>();
                var ctors = type.Value.GetConstructors();
                foreach (var ctor in ctors)
                {
                    var pars = ctor.GetParameters();
                    foreach (var item in pars)
                    {
                        if (!item.ParameterType.IsPrimitive && item.ParameterType != typeof(string) && !UserData.IsTypeRegistered(item.ParameterType))
                        {
                            throw new Exception("CLR type constructor parameters must be added to UserData or be a primitive type or string");
                        }
                    }

                    if (!paramCounts.Contains(pars.Length))
                    {
                        string parString = "";
                        paramCounts.Add(pars.Length);
                        for (int j = 0; j < pars.Length; j++)
                        {
                            if (j == 0) { parString += $"t{j}"; }
                            else { parString += $",t{j}"; }
                        }

                        //REVERT
                        //s.AppendLine($"function {newFuncName}({parString}) return {typeName}.__new({parString}) end");
                        s.AppendLine($"{GlobalScriptBindings.NewableTable}.{newFuncName} = function({parString}) return {typeName}.__new({parString}) end");
                    }
                }
            }
            return s.ToString();
        }

        private void BakeNewables()
        {
            bakedNewableTypeString = Bake(newableTypes);
        }

        private void BakeYieldables()
        {
            bakedYieldableTypeString = Bake(yieldableTypes);
        }

        /// <summary>
        /// Initializes a script with the newable types
        /// </summary>
        /// <param name="lua"></param>
        private void InitializeNewables(Script lua)
        {
            if (lua.Globals.Get(GlobalScriptBindings.NewableTable).Type != DataType.Table)
            {
                lua.Globals[GlobalScriptBindings.NewableTable] = new Table(lua);
            }

            foreach (var type in newableTypes)
            {
                lua.Globals[type.Key] = type.Value;
            }

            if (bakedNewableTypeString != null)
            {
                lua.DoString(bakedNewableTypeString);
            }
        }

        private void InitializeYieldables(Script lua)
        {
            foreach (var type in yieldableTypes)
            {
                lua.Globals[type.Key] = type.Value;
            }

            if (bakedYieldableTypeString != null)
            {
                lua.DoString(bakedYieldableTypeString);
            }
        }


        /// <summary>
        /// Removes all callback functions from a script
        /// </summary>
        /// <param name="lua"></param>
        internal void CleanFunctions(Script lua)
        {
            foreach (var func in bindItems)
            {
                lua.Globals.Remove(func.Value.Name);
            }
        }

        public List<string> GetAllRegisteredPaths()
        {
            List<string> ret = new List<string>();
            foreach (var item in newableTypes)
            {
                ret.Add("new." + item.Key);
            }
            foreach (var item in yieldableTypes)
            {
                ret.Add("new." + item.Key);
            }

            foreach (var item in bindItems)
            {
                if(item.Value is BindTable bTable)
                {
                    ret.AddRange(bTable.GetAllItemPaths());
                }
                else if(item.Value is BindEnum bEnum)
                {
                    ret.AddRange(bEnum.GetAllEnumPaths());
                }
                else
                {
                    ret.Add(item.Key);
                }
            }

            return ret;
        }

    }
}

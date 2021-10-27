namespace LunarBind
{
    using MoonSharp.Interpreter;
    using MoonSharp.Interpreter.Interop;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using Yielding;

    //TODO: Make GlobalScriptBindings contain and reference a static ScriptBindings to reduce code duplication and improve maintainability
    //TODO: Generate documentation
    public static class GlobalScriptBindings
    {
        internal static readonly Dictionary<string, BindItem> bindItems = new Dictionary<string, BindItem>();
        internal static readonly Dictionary<string, Type> yieldableTypes = new Dictionary<string, Type>();
        internal static readonly Dictionary<string, Type> newableTypes = new Dictionary<string, Type>();
        //private static readonly Dictionary<string, Type> staticTypes = new Dictionary<string, Type>();

        private static string bakedNewableTypeString = null;
        private static string bakedYieldableNewableTypeString = null;

        /// <summary>
        /// The table newable constructors are stored under. Only modify before binding anything for consistency.
        /// </summary>
        public static string NewableTable { get; set; } = "new";

        /// <summary>
        /// Should functions that return a Yielder subclass be automatically wrapped in coroutine.yield()? Defaults to true
        /// </summary>
        public static bool AutoYield { get; set; } = true;

        //TODO: Implement
        ///// <summary>
        ///// Should bound classes automatically be added to <see cref="MoonSharp"/>'s <see cref="UserData"/>? defaults to true
        ///// </summary>
        //public static bool AutoAddUserData { get; set; } = true;

        /// <summary>
        /// Use this to initialize all scripts with custom Lua code. This runs after all global bindings have been set up. 
        /// </summary>
        public static string CustomInitializerString { get; set; } = null;

        /// <summary>
        /// Use this to initialize all scripts with custom Lua code. This runs before any global bindings have been set up. (useful for modules, etc)
        /// </summary>
        public static string CustomPreInitializerString { get; set; } = null;

        //This must happen before any script is initialized, static constructor assures that this will happen
        static GlobalScriptBindings()
        {
            Script.WarmUp();
            RegisterDefaultGlobalBindings();
            InitializeDefaultYielders();
        }

        private static void RegisterDefaultGlobalBindings()
        {
            RegisterAssemblyTypes(typeof(GlobalScriptBindings).Assembly);
            UserData.RegisterAssembly(typeof(GlobalScriptBindings).Assembly);
        }

        private static void InitializeDefaultYielders()
        {
            GlobalScriptBindings.AddYieldableType<WaitFrames>();
            //Other internal yielder types are not meant to be created in Lua, and thus will not be added here
        }


        /// <summary>
        /// Initializes a script with C# callback functions, types, newable types, enums, and yieldables
        /// </summary>
        /// <param name="lua"></param>
        public static void Initialize(Script lua)
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

        public static void AddYieldableType<T>(string name = null) where T : Yielder
        {
            if (name == null) { name = typeof(T).Name; }
            RegisterUserDataType(typeof(T));
            yieldableTypes[name] = typeof(T);
            BakeYieldableNewables();
        }

        /// <summary>
        /// Allows you to use the type's name as the constructor, under the table <see cref="GlobalScriptBindings.NewableTable"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="t"></param>
        public static void AddNewableType(string name, Type t)
        {
            RegisterUserDataType(t);
            newableTypes[name] = t;
            BakeNewables();
        }

        public static void AddNewableType(Type t)
        {
            RegisterUserDataType(t);
            newableTypes[t.Name] = t;
            BakeNewables();
        }

        //Todo: remove prefix thing
        public static void RemoveNewableType(string name)
        {
            newableTypes.Remove(name);
            BakeNewables();
        }

        public static void AddEnum<T>() where T : Enum
        {
            BindingHelpers.CreateBindEnum(bindItems, typeof(T).Name, typeof(T));
        }

        public static void AddEnum<T>(string path)
        {
            BindingHelpers.CreateBindEnum(bindItems, path, typeof(T));
        }

        /// <summary>
        /// Allows you to access static functions and members on the type by using the Lua global with the name<para/>
        /// Equivalent to script.Globals[t.Name] = t
        /// </summary>
        /// <param name="t"></param>
        public static void AddGlobalType(Type t)
        {
            RegisterUserDataType(t);
            BindingHelpers.CreateBindType(bindItems, t.Name, t);
        }

        /// <summary>
        /// Allows you to access static functions and members on the type by using the Lua global with the name<para/>
        /// <para/>
        /// Equivalent to script.Globals[name] = t
        /// </summary>
        /// <param name="path"></param>
        /// <param name="t"></param>
        public static void AddGlobalType(string path, Type t)
        {
            RegisterUserDataType(t);
            BindingHelpers.CreateBindType(bindItems, path, t);
        }

        /// <summary>
        /// Add an object at the specified path
        /// </summary>
        /// <param name="path"></param>
        /// <param name="o"></param>
        public static void AddGlobalObject(string path, object o)
        {
            RegisterUserDataType(o.GetType());
            BindingHelpers.CreateBindUserObject(bindItems, path, o);
        }

        /// <summary>
        ///  Bind all static functions with the [<see cref="LunarBindFunctionAttribute"/>] attribute on each type in each assembly
        /// </summary>
        /// <param name="assemblies"></param>
        public static void BindAssemblyFuncs(params Assembly[] assemblies)
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
        public static void BindAssemblies(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                RegisterAssemblyTypes(assembly);
            }
        }

        //TODO: rename to differentiate from the AddGlobalType, etc
        /// <summary>
        /// Use <see cref="BindTypeFuncs(Type[])"/> instead
        /// </summary>
        /// <param name="assemblies"></param>
        [Obsolete("Use BindTypeFuncs instead")]
        public static void AddTypes(params Type[] types)
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
        public static void BindTypeFuncs(params Type[] types)
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
        public static void BindTypeFuncs(string pathPrefix, params Type[] types)
        {
            foreach (var type in types)
            {
                RegisterTypeFuncs(type, pathPrefix);
            }
        }

        /// <summary>
        /// Automatically register classes as user data (classes tagged with <see cref="MoonSharpUserDataAttribute"/>) in an assembly
        /// </summary>
        /// <param name="assemblies"></param>
        public static void RegisterAssemblyUserData(params Assembly[] assemblies)
        {
            foreach (var assembly in assemblies)
            {
                UserData.RegisterAssembly(assembly);
            }
        }

        /// <summary>
        /// Manually register a type to use in Lua, is ignored if type is already registered. Calls MoonSharp's <see cref="UserData.RegisterType(Type, InteropAccessMode)"/> Only a static form of registration is available.
        /// </summary>
        /// <param name="t"></param>
        public static void RegisterUserDataType(Type t)
        {
            if (!(t == typeof(string) || t.IsPrimitive) && !UserData.IsTypeRegistered(t))
            {
                UserData.RegisterType(t);
            }
        }

        /// <summary>
        /// Manually register a type to use in Lua. Calls MoonSharp's <see cref="UserData.RegisterType(Type, InteropAccessMode)"/> Only a static form of registration is available.
        /// </summary>
        /// <param name="t"></param>
        public static void RegisterUserDataType(Type t, InteropAccessMode mode)
        {
            UserData.RegisterType(t, mode);
        }
        /// <summary>
        /// Manually register a type to use in Lua. Calls MoonSharp's <see cref="UserData.RegisterType(Type, IUserDataDescriptor)"/> Only a static form of registration is available.
        /// </summary>
        /// <param name="t"></param>
        public static void RegisterUserDataType(Type t, IUserDataDescriptor descriptor)
        {
            UserData.RegisterType(t, descriptor);
        }

        /// <summary>
        /// Internal method for registering functions on a type
        /// </summary>
        /// <param name="type"></param>
        private static void RegisterTypeFuncs(Type type, string prefix = null)
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

        //public static void HookActionProps<T0>(Type type)
        //{
        //    PropertyInfo[] props = type.GetProperties(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        //    foreach (var prop in props)
        //    {
        //        var attr = (LuaFunctionAttribute)Attribute.GetCustomAttribute(prop, typeof(LuaFunctionAttribute));
        //        if (attr != null)
        //        {
        //            var val = prop.GetValue(null);
        //            if (val.GetType().IsAssignableFrom(typeof(Action<T0>)))
        //            {
        //                var action = ((Action<T0>)val);
        //                var del = Delegate.CreateDelegate(typeof(Action<T0>), action, "Invoke");
        //                string name = attr.Name ?? prop.Name;
        //                BindingHelpers.CreateCallbackItem(callbackItems, name, del, documentation?.Data ?? "", example?.Data ?? "");
        //                //callbackItems[name] = new CallbackFunc(name, del, "", "");
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Use <see cref="BindAction(string, Action, string, string)"/>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        [Obsolete("use BindAction instead")]
        public static void AddAction(string name, Action action, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, name, action, false, documentation ?? "", example ?? "");
        }
        /// <summary>
        /// Use <see cref="BindAction(Action, string, string)"/> instead
        /// </summary>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        [Obsolete("use BindAction instead")]
        public static void AddAction(Action action, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, action.Method.Name, action, false, documentation ?? "", example ?? "");
        }

        /// <summary>
        /// Add specific <see cref="Action"/>s to the bindings, using the method's Name as the name for each
        /// </summary>
        /// <param name="actions"></param>
        [Obsolete("use BindActions instead")]
        public static void AddActions(params Action[] actions)
        {
            foreach (var action in actions)
            {
                BindingHelpers.CreateBindFunction(bindItems, action.Method.Name, action, false, "", "");
            }
        }

        /// <summary>
        /// Add a specific <see cref="Action"/> to the bindings.
        /// </summary>
        /// <param name="path">The path to bind to. Can contain "."</param>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public static void BindAction(string path, Action action, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, path, action, false, documentation ?? "", example ?? "");
        }

        /// <summary>
        /// Add a specific <see cref="Action"/> to the bindings, using the method's Name as the name
        /// </summary>
        /// <param name="action"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public static void BindAction(Action action, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, action.Method.Name, action, false, documentation ?? "", example ?? "");
        }

        /// <summary>
        /// Add specific <see cref="Action"/>s to the bindings, using the method's Name as the name for each
        /// </summary>
        /// <param name="actions"></param>
        public static void BindActions(params Action[] actions)
        {
            foreach (var action in actions)
            {
                BindingHelpers.CreateBindFunction(bindItems, action.Method.Name, action, false, "", "");
            }
        }

        /// <summary>
        /// Add a specific <see cref="Delegate"/> to the bindings
        /// </summary>
        /// <param name="name"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        [Obsolete("Use BindDelegate instead")]
        public static void AddDelegate(string name, Delegate del, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, name, del, false, documentation ?? "", example ?? "");
        }

        /// <summary>
        /// Add a specific <see cref="Delegate"/> to the bindings using the method Name as the name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        [Obsolete("Use BindDelegate instead")]
        public static void AddDelegate(Delegate del, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, del.Method.Name, del, false, documentation ?? "", example ?? "");
        }

        /// <summary>
        /// Add a specific <see cref="Delegate"/>s to the bindings using the method Names as the name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        [Obsolete("Use BindDelegates instead")]
        public static void AddDelegates(params Delegate[] dels)
        {
            foreach (var del in dels)
            {
                BindingHelpers.CreateBindFunction(bindItems, del.Method.Name, del, false, "", "");
            }
        }

        /// <summary>
        /// Add a specific <see cref="Delegate"/> to the bindings
        /// </summary>
        /// <param name="name"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public static void BindDelegate(string name, Delegate del, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, name, del, false, documentation ?? "", example ?? "");
        }

        /// <summary>
        /// Add a specific <see cref="Delegate"/> to the bindings using the method Name as the name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public static void BindDelegate(Delegate del, string documentation = "", string example = "")
        {
            BindingHelpers.CreateBindFunction(bindItems, del.Method.Name, del, false, documentation ?? "", example ?? "");
        }

        /// <summary>
        /// Add a specific <see cref="Delegate"/>s to the bindings using the method Names as the name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="del"></param>
        /// <param name="documentation"></param>
        /// <param name="example"></param>
        public static void BindDelegates(params Delegate[] dels)
        {
            foreach (var del in dels)
            {
                BindingHelpers.CreateBindFunction(bindItems, del.Method.Name, del, false, "", "");
            }
        }

        private static void RegisterAssemblyTypes(Assembly assembly)
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

        private static void BakeNewables()
        {
            bakedNewableTypeString = BakeNewableTypeString(newableTypes);
        }

        private static void BakeYieldableNewables()
        {
            bakedYieldableNewableTypeString = BakeNewableTypeString(yieldableTypes);
        }

        private static string BakeNewableTypeString(Dictionary<string, Type> source)
        {
            StringBuilder s = new StringBuilder();

            foreach (var type in source)
            {
                string typeName = type.Key;
                string newFuncName = type.Key;//.Remove(0, TypePrefix.Length);
                HashSet<int> paramCounts = new HashSet<int>();
                var ctors = type.Value.GetConstructors().Where(x => x.GetCustomAttribute(typeof(MoonSharpHiddenAttribute)) == null);// x.CustomAttributes.Any(y => y.AttributeType == typeof(MoonSharp.Interpreter.MoonSharpHiddenAttribute)));
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

                        s.AppendLine($"{NewableTable}.{newFuncName} = function({parString}) return {typeName}.__new({parString}) end");
                    }
                }
            }
            return s.ToString();
        }

        /// <summary>
        /// Initializes a script with the newable types
        /// </summary>
        /// <param name="lua"></param>
        private static void InitializeNewables(Script lua)
        {
            if (lua.Globals.Get(NewableTable).Type != DataType.Table)
            {
                lua.Globals[NewableTable] = new Table(lua);
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

        /// <summary>
        /// Initializes a script with the yielder types
        /// </summary>
        /// <param name="lua"></param>
        internal static void InitializeYieldables(Script lua)
        {
            foreach (var type in yieldableTypes)
            {
                lua.Globals[type.Key] = type.Value;
            }

            if (bakedYieldableNewableTypeString != null)
            {
                lua.DoString(bakedYieldableNewableTypeString);
            }
        }


        public static List<string> GetAllRegisteredPaths()
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
                if (item.Value is BindTable bTable)
                {
                    ret.AddRange(bTable.GetAllItemPaths());
                }
                else if (item.Value is BindEnum bEnum)
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
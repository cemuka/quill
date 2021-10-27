namespace LunarBind
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using MoonSharp.Interpreter;
    internal static class BindingHelpers
    {
        //From https://stackoverflow.com/a/40579063
        //Creates a delegate from reflection info. Very helpful for binding
        public static Delegate CreateDelegate(MethodInfo methodInfo, object target = null)
        {
            Func<Type[], Type> getType;
            var isAction = methodInfo.ReturnType.Equals((typeof(void)));
            var types = methodInfo.GetParameters().Select(p => p.ParameterType);

            if (isAction)
            {
                getType = System.Linq.Expressions.Expression.GetActionType;
            }
            else
            {
                getType = System.Linq.Expressions.Expression.GetFuncType;
                types = types.Concat(new[] { methodInfo.ReturnType });
            }

            if (methodInfo.IsStatic)
            {
                return Delegate.CreateDelegate(getType(types.ToArray()), methodInfo);
            }

            return Delegate.CreateDelegate(getType(types.ToArray()), target, methodInfo.Name);
        }

        //TODO: get the attributes here
        /// <summary>
        /// Automatically create tables, etc
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="pathString"></param>
        internal static BindFunc CreateBindFunction(Dictionary<string, BindItem> dict, string pathString, Delegate callback, bool autoYield = true, string documentation = "", string example = "")
        {
            if (string.IsNullOrWhiteSpace(pathString))
            {
                throw new Exception($"Path cannot be null, empty, or whitespace for path [{pathString}] MethodInfo: ({callback.Method.Name})");
            }
            var path = pathString.Split('.');
            string root = path[0];
            BindFunc func = new BindFunc(path[path.Length-1],  callback, autoYield, documentation, example);
            if (autoYield && func.IsYieldable && GlobalScriptBindings.AutoYield) { func.GenerateYieldableString(pathString); }

            if (path.Length == 1)
            {
                //Simple global function
                if (dict.ContainsKey(root))
                {
                    throw new Exception($"Cannot add {pathString} ({callback.Method.Name}), a {GetItemTypeStr(dict[root])} with that key already exists");
                }
                dict[root] = func;
            }
            else
            {
                //Recursion time
                if(dict.TryGetValue(root, out BindItem item))
                {
                    if(item is BindTable t)
                    {
                        t.AddBindFunc(path, 1, func);
                        t.GenerateWrappedYieldString(); //Bake the yieldable string
                    }
                    else
                    {
                        throw new Exception($"Cannot add {pathString} ({callback.Method.Name}), One or more keys in the path is assigned to a function");
                    }
                }
                else
                {
                    //Create new table
                    BindTable t = new BindTable(root);
                    dict[root] = t;
                    t.AddBindFunc(path, 1, func);
                    t.GenerateWrappedYieldString(); //Bake the yieldable string
                }
            }

            return func;
        }

        /// <summary>
        /// Automatically create tables, etc
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="pathString"></param>
        internal static BindUserObject CreateBindUserObject(Dictionary<string, BindItem> dict, string pathString, object obj)
        {
            if (string.IsNullOrWhiteSpace(pathString))
            {
                throw new Exception($"Path cannot be null, empty, or whitespace for path [{pathString}]");
            }
            var path = pathString.Split('.');
            string root = path[0];
            BindUserObject bindObj = new BindUserObject(path[path.Length - 1], obj);

            var doc = obj.GetType().GetCustomAttribute<LunarBindDocumentationAttribute>()?.Data ?? "";
            var ex = obj.GetType().GetCustomAttribute<LunarBindExampleAttribute>()?.Data ?? "";
            bindObj.Documentation = doc;
            bindObj.Example = ex;


            if (path.Length == 1)
            {
                //Simple global function
                if (dict.ContainsKey(root))
                {
                    throw new Exception($"Cannot add {pathString} (global object), a {GetItemTypeStr(dict[root])} with that key already exists");
                }
                dict[root] = bindObj;
            }
            else
            {
                //Recursion time
                if (dict.TryGetValue(root, out BindItem item))
                {
                    if (item is BindTable t)
                    {
                        t.AddBindUserObject(path, 1, bindObj);
                        t.GenerateWrappedYieldString(); //Bake the yieldable string
                    }
                    else
                    {
                        throw new Exception($"Cannot add {pathString} (global object), One or more keys in the path is already assigned to an item");
                    }
                }
                else
                {
                    //Create new table
                    BindTable t = new BindTable(root);
                    dict[root] = t;
                    t.AddBindUserObject(path, 1, bindObj);
                }
            }

            return bindObj;
        }

        /// <summary>
        /// Automatically create tables, etc
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="pathString"></param>
        internal static BindUserType CreateBindType(Dictionary<string, BindItem> dict, string pathString, Type type)
        {
            if (string.IsNullOrWhiteSpace(pathString))
            {
                throw new Exception($"Path cannot be null, empty, or whitespace for path [{pathString}]");
            }
            var path = pathString.Split('.');
            string root = path[0];
            BindUserType bindObj = new BindUserType(path[path.Length - 1], type);

            var doc = type.GetCustomAttribute<LunarBindDocumentationAttribute>()?.Data ?? "";
            var ex = type.GetCustomAttribute<LunarBindExampleAttribute>()?.Data ?? "";
            bindObj.Documentation = doc;
            bindObj.Example = ex;


            if (path.Length == 1)
            {
                //Simple global function
                if (dict.ContainsKey(root))
                {
                    throw new Exception($"Cannot add {pathString} (global object), a {GetItemTypeStr(dict[root])} with that key already exists");
                }
                dict[root] = bindObj;
            }
            else
            {
                //Recursion time
                if (dict.TryGetValue(root, out BindItem item))
                {
                    if (item is BindTable t)
                    {
                        t.AddBindUserType(path, 1, bindObj);
                        t.GenerateWrappedYieldString(); //Bake the yieldable string
                    }
                    else
                    {
                        throw new Exception($"Cannot add {pathString} (global object), One or more keys in the path is already assigned to an item");
                    }
                }
                else
                {
                    //Create new table
                    BindTable t = new BindTable(root);
                    dict[root] = t;
                    t.AddBindUserType(path, 1, bindObj);
                }
            }
            return bindObj;
        }


        /// <summary>
        /// Automatically create tables, etc for bind enums
        /// </summary>
        /// <param name="dict"></param>
        /// <param name="pathString"></param>
        internal static BindEnum CreateBindEnum(Dictionary<string, BindItem> dict, string pathString, Type enumType)
        {
            if (string.IsNullOrWhiteSpace(pathString))
            {
                throw new Exception($"Path cannot be null, empty, or whitespace for path [{pathString}] Enum Type: ({enumType.Name})");
            }
            var path = pathString.Split('.');

            string root = path[0];
            BindEnum bindEnum = new BindEnum(path[path.Length - 1], enumType);


            if (path.Length == 1)
            {
                //Simple global function
                if (dict.ContainsKey(root))
                {
                    throw new Exception($"Cannot add {pathString} ({enumType.Name}), a {GetItemTypeStr(dict[root])} with that key already exists");
                }
                dict[root] = bindEnum;
            }
            else
            {
                //Recursion time
                if (dict.TryGetValue(root, out BindItem item))
                {
                    if (item is BindTable t)
                    {
                        t.AddBindEnum(path, 1, bindEnum);
                    }
                    else
                    {
                        throw new Exception($"Cannot add {pathString} ({enumType.Name}), The root key is a {GetItemTypeStr(dict[root])}");
                    }
                }
                else
                {
                    //Create new table
                    BindTable t = new BindTable(root);
                    dict[root] = t;
                    t.AddBindEnum(path, 1, bindEnum);
                }
            }

            return bindEnum;
        }


        private static string GetItemTypeStr(BindItem item)
        {
            return (item is BindFunc ? "Function" : (item is BindEnum ? "Enum" : (item is BindUserObject ? "Global Object" : (item is BindUserType ? "Global Type" : "Table"))));
        }



        ///// <summary>
        ///// Automatically create tables, etc
        ///// </summary>
        ///// <param name="dict"></param>
        ///// <param name="pathString"></param>
        //public static void CreateCallbackEnum(Dictionary<string, BindItem> dict, string pathString, List<KeyValuePair<string,int>> keyValues, string documentation, string example)
        //{
        //    if (string.IsNullOrWhiteSpace(pathString))
        //    {
        //        throw new Exception($"Path cannot be null, empty, or whitespace for path [{pathString}] MethodInfo: ({callback.Method.Name})");
        //    }
        //    var path = pathString.Split('.');
        //    string root = path[0];
        //    BindFunc func = new BindFunc(path[path.Length - 1], callback, documentation, example);
        //    if (func.IsYieldable && GlobalScriptBindings.AutoYield) { func.GenerateYieldableString(pathString); }

        //    if (path.Length == 1)
        //    {
        //        //Simple global function
        //        if (dict.ContainsKey(root))
        //        {
        //            throw new Exception($"Cannot add {pathString} ({callback.Method.Name}), a {(dict[root].GetType() == typeof(BindFunc) ? "Function" : "Table")} with that key already exists");
        //        }
        //        dict[root] = func;
        //    }
        //    else
        //    {
        //        //Recursion time
        //        if (dict.TryGetValue(root, out BindItem item))
        //        {
        //            if (item is BindTable t)
        //            {
        //                t.AddCallbackFunc(path, 1, func);
        //                t.GenerateYieldableString(); //Bake the yieldable string
        //            }
        //            else
        //            {
        //                throw new Exception($"Cannot add {pathString} ({callback.Method.Name}), One or more keys in the path is assigned to a function");
        //            }
        //        }
        //        else
        //        {
        //            //Create new
        //            BindTable t = new BindTable(root);
        //            dict[root] = t;
        //            t.AddCallbackFunc(path, 1, func);
        //            t.GenerateYieldableString(); //Bake the yieldable string
        //        }
        //    }
        //}



    }

}

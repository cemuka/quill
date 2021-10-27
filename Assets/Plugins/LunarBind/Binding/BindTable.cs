namespace LunarBind
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using MoonSharp.Interpreter;
    using System.Reflection;
    internal class BindTable : BindItem
    {
        //Todo: use more polymorphism??
        //internal string
        internal readonly Dictionary<string, BindTable> bindTables = new Dictionary<string, BindTable>();
        internal readonly Dictionary<string, BindFunc> bindFunctions = new Dictionary<string, BindFunc>();
        internal readonly Dictionary<string, BindEnum> bindEnums = new Dictionary<string, BindEnum>();
        internal readonly Dictionary<string, BindUserObject> bindObjects = new Dictionary<string, BindUserObject>();
        internal readonly Dictionary<string, BindUserType> bindTypes = new Dictionary<string, BindUserType>();
        //private readonly Dictionary<string, BindField> bindFields = new Dictionary<string, BindField>();

        public BindTable(string name)
        {
            this.Name = name;
        }
        /// <summary>
        /// For autocomplete purposes
        /// </summary>
        /// <param name="prefix"></param>
        /// <returns></returns>
        internal List<string> GetAllItemPaths(string prefix = "")
        {
            prefix += Name + ".";
            List<string> ret = new List<string>();
            foreach (var table in bindTables.Values)
            {
                ret.AddRange(table.GetAllItemPaths(prefix));
            }
            foreach (var enu in bindEnums)
            {
                ret.AddRange(enu.Value.GetAllEnumPaths(prefix));
            }
            foreach (var func in bindFunctions)
            {
                ret.Add(prefix + func.Value.Name);
            }
            foreach (var obj in bindObjects)
            {
                ret.Add(prefix + obj.Value.Name);
            }
            foreach (var type in bindTypes)
            {
                ret.Add(prefix + type.Value.Name);
            }
            return ret;
        }

        /// <summary>
        /// For documentation purposes
        /// </summary>
        /// <returns></returns>
        internal List<BindItem> GetAllItems()
        {
            List<BindItem> bindItems = new List<BindItem>();

            foreach (var item in bindFunctions.Values)
            {
                bindItems.Add(item);
            }
            foreach (var item in bindEnums.Values)
            {
                bindItems.Add(item);
            }
            foreach (var item in bindObjects.Values)
            {
                bindItems.Add(item);
            }
            foreach (var item in bindTypes.Values)
            {
                bindItems.Add(item);
            }
            foreach (var item in bindTables.Values)
            {
                bindItems.Add(item);
            }
            return bindItems;
        }

        public void GenerateWrappedYieldString()
        {
            StringBuilder sb = new StringBuilder();
            GenYieldableStringRecursive(sb);
            YieldableString = sb.ToString();
        }

        private void GenYieldableStringRecursive(StringBuilder sb)
        {
            foreach (var f in bindFunctions.Values)
            {
                if (f.IsYieldable)
                {
                    sb.Append(f.YieldableString);
                }
            }
            foreach (var t in bindTables.Values)
            {
                t.GenYieldableStringRecursive(sb);
            }
        }

        internal void AddBindFunc(string[] path, int index, BindFunc bindFunc)
        {
            if (index + 1 >= path.Length)
            {
                //At lowest level, add callback func
                if (bindTables.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".",path)} ({bindFunc.Name}), a Table with that key already exists");
                }
                else
                {
                    CheckConflictingItems(path, index, bindFunc);
                }

                bindFunctions[path[index]] = bindFunc;
            }
            else
            {
                CheckConflictingItems(path, index, bindFunc);

                BindTable nextTable;
                if (bindTables.TryGetValue(path[index], out nextTable))
                {
                    nextTable.AddBindFunc(path, index + 1, bindFunc);
                }
                else
                {
                    nextTable = new BindTable(path[index]);
                    bindTables.Add(path[index], nextTable);
                    nextTable.AddBindFunc(path, index + 1, bindFunc);
                }
            }
        }

        
        internal void AddBindEnum(string[] path, int index, BindEnum bindEnum)
        {
            if (index + 1 >= path.Length)
            {
                //At lowest level, add enum
                if (bindTables.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".", path)} ({bindEnum.Name}), a Table with that key already exists");
                }
                else
                {
                    CheckConflictingItems(path, index, bindEnum);
                }
                bindEnums[path[index]] = bindEnum;
            }
            else
            {
                CheckConflictingItems(path, index, bindEnum);

                BindTable nextTable;
                if (bindTables.TryGetValue(path[index], out nextTable))
                {
                    nextTable.AddBindEnum(path, index + 1, bindEnum);
                }
                else
                {
                    //Create new table
                    nextTable = new BindTable(path[index]);
                    bindTables.Add(path[index], nextTable);
                    nextTable.AddBindEnum(path, index + 1, bindEnum);
                }
            }
        }

        internal void AddBindUserObject(string[] path, int index, BindUserObject bindObj)
        {
            if (index + 1 >= path.Length)
            {
                //At lowest level, add enum
                if (bindTables.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".", path)} ({bindObj.Name}), a Table with that key already exists");
                }
                else
                {
                    CheckConflictingItems(path, index, bindObj);
                }
                bindObjects[path[index]] = bindObj;
            }
            else
            {
                CheckConflictingItems(path, index, bindObj);

                BindTable nextTable;
                if (bindTables.TryGetValue(path[index], out nextTable))
                {
                    nextTable.AddBindUserObject(path, index + 1, bindObj);
                }
                else
                {
                    //Create new table
                    nextTable = new BindTable(path[index]);
                    bindTables.Add(path[index], nextTable);
                    nextTable.AddBindUserObject(path, index + 1, bindObj);
                }
            }
        }

        internal void AddBindUserType(string[] path, int index, BindUserType bindType)
        {
            if (index + 1 >= path.Length)
            {
                //At lowest level, add enum
                if (bindTables.ContainsKey(path[index]))
                {
                    throw new Exception($"Cannot add {string.Join(".", path)} ({bindType.Name}), a Table with that key already exists");
                }
                else
                {
                    CheckConflictingItems(path, index, bindType);
                }

                bindTypes[path[index]] = bindType;
            }
            else
            {
                CheckConflictingItems(path, index, bindType);

                BindTable nextTable;
                if (bindTables.TryGetValue(path[index], out nextTable))
                {
                    nextTable.AddBindUserType(path, index + 1, bindType);
                }
                else
                {
                    //Create new table
                    nextTable = new BindTable(path[index]);
                    bindTables.Add(path[index], nextTable);
                    nextTable.AddBindUserType(path, index + 1, bindType);
                }
            }
        }

        private void CheckConflictingItems(string[] path, int index, BindItem bindType)
        {
            string GetConflictingPath()
            {
                List<string> retL = new List<string>();
                for (int i = 0; i <= index; i++)
                {
                    retL.Add(path[index]);
                }
                return string.Join(".", retL);
            }

            if (bindFunctions.ContainsKey(path[index]))
            {
                throw new Exception($"Cannot add {string.Join(".", path)} ({bindType.Name}), a Function  with the key ({GetConflictingPath()}) exists in the path");
            }
            else if (bindEnums.ContainsKey(path[index]))
            {
                throw new Exception($"Cannot add {string.Join(".", path)} ({bindType.Name}), an Enum  with the key ({GetConflictingPath()}) exists in the path");
            }
            else if (bindObjects.ContainsKey(path[index]))
            {
                throw new Exception($"Cannot add {string.Join(".", path)} ({bindType.Name}), a Global Object with the key ({GetConflictingPath()}) exists in the path");
            }
            else if (bindTypes.ContainsKey(path[index]))
            {
                throw new Exception($"Cannot add {string.Join(".", path)} ({bindType.Name}), a Global Type with the key ({GetConflictingPath()}) exists in the path");
            }
        }

        private Table GenerateTable(Script script)
        {
            Table table = new Table(script);

            //Tables
            foreach (var t in bindTables.Values)
            {
                table[t.Name] = t.GenerateTable(script);
            }
            //Functions
            foreach (var f in bindFunctions.Values)
            {
                table[f.Name] = DynValue.FromObject(script, f.Callback);
            }
            //Enums
            foreach (var e in bindEnums.Values)
            {
                table[e.Name] = e.CreateEnumTable(script);
            }

            //Global Objects
            foreach (var o in bindObjects.Values)
            {
                table[o.Name] = o.UserObject;
            }

            //Global Types
            foreach (var o in bindTypes.Values)
            {
                table[o.Name] = o.UserType;
            }

            return table;
        }

        internal override void AddToScript(Script script)
        {
            script.Globals[Name] = GenerateTable(script);
            if (!string.IsNullOrWhiteSpace(YieldableString)) { script.DoString(YieldableString); }
        }
    }
}

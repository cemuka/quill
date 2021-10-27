namespace LunarBind
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Linq;
    using MoonSharp.Interpreter;
    using System.Reflection;

    internal class BindEnum : BindItem
    {
        internal Type EnumType { get; private set; }
        internal List<KeyValuePair<string, int>> EnumVals = new List<KeyValuePair<string, int>>();
        internal List<string> FieldDocumentations { get; private set; } = new List<string>();
        internal List<string> FieldExamples { get; private set; } = new List<string>();

        public BindEnum(string name, Type e)
        {
            EnumType = e;
            Name = name;

            Documentation = e.GetCustomAttribute<LunarBindDocumentationAttribute>()?.Data ?? "";
            Example = e.GetCustomAttribute<LunarBindExampleAttribute>()?.Data ?? "";

            var fields = e.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            foreach (var field in fields)
            {
                var hidden = (LunarBindHideAttribute)Attribute.GetCustomAttribute(field, typeof(LunarBindHideAttribute)) != null ||
                    (MoonSharpHiddenAttribute)Attribute.GetCustomAttribute(field, typeof(MoonSharpHiddenAttribute)) != null;
                
                if(!hidden)
                {
                    EnumVals.Add(new KeyValuePair<string, int>(field.Name, (int)field.GetValue(null)));
                    //Add any documentation attributes
                    var doc = field.GetCustomAttribute<LunarBindDocumentationAttribute>()?.Data ?? "";
                    var ex = field.GetCustomAttribute<LunarBindExampleAttribute>()?.Data ?? "";
                    FieldDocumentations.Add(doc);
                    FieldExamples.Add(ex);
                }
            }
        }

        public string[] GetAllEnumPaths(string prefix = "")
        {
            string[] ret = new string[EnumVals.Count];
            prefix += Name + ".";
            for (int i = 0; i < EnumVals.Count; i++)
            {
                ret[i] = prefix + EnumVals[i].Key;
            }
            return ret;
        }

        internal Table CreateEnumTable(Script script)
        {
            Table t = new Table(script);
            foreach (var item in EnumVals)
            {
                t[item.Key] = item.Value;
            }
            return t;
        }

        internal override void AddToScript(Script script)
        {
            Table t = new Table(script);

            foreach (var item in EnumVals)
            {
                t[item.Key] = item.Value;
            }

            script.Globals[Name] = t;
        }
    }

}

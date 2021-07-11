using QuillLib;
using UnityEngine;
using TMPro;
using MoonSharp.Interpreter;
using System;

namespace QuillLib.Lua
{
    public class QuillLua
    {
        private static QuillLuaData _data;
        private static Script _script;

        public static void Run()
        {
            UserData.RegisterAssembly();
            var path = System.IO.Path.Combine(Application.streamingAssetsPath, "LUA/main.lua");
            var code = System.IO.File.ReadAllText(path);

            _data   = new QuillLuaData();
            _script = new Script(); 

            _script.Globals["quill"] = _data;
            _script.DoString(code);

            _script.Call(_script.Globals["Init"]);
        }


        public static void Update()
        {
            _script.Call(_script.Globals["OnUpdate"], Time.deltaTime);
        }
        
        [MoonSharpUserData]
        private class QuillLuaData
        {
            public static QuillElementProxy empty()
            {
                var target = Quill.CreateEmpty();
                Quill.mainRoot.Add(target);

                return new QuillElementProxy(target);
            }

            public static QuillLabelProxy label(string text)
            {
                var target = Quill.CreateLabel(text);
                Quill.mainRoot.Add(target);

                return new QuillLabelProxy(target);
            }

            public static QuillBoxProxy box()
            {
                var target = Quill.CreateBox(Color.clear);
                Quill.mainRoot.Add(target);

                return new QuillBoxProxy(target);
            }

            public static void log(string log)
            {
                Debug.Log(log);
            }
        }
    }
}

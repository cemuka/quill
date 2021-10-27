using QuillLib;
using UnityEngine;
using MoonSharp.Interpreter;
using System;
using System.IO;
using MoonSharp.Interpreter.Loaders;
using System.Collections.Generic;
using System.Collections;

namespace QuillLib.Lua
{
    public class QuillLua
    {
        public static string    IMG_FOLDER_PATH = Application.streamingAssetsPath + "/IMAGE/";
        public static Dictionary<string, Font> loadedFonts = new Dictionary<string, Font>();

        private static Script _script;
        private static DynValue _updateResult;
        
        public static Script MainScript()
        {
             return _script; 
        }

        public static void Run()
        {
            Run(CoreModules.Preset_Default);
        }

        public static void Run(CoreModules modules)
        {
            loadedFonts.Add(Quill.defaultFont.name, Quill.defaultFont);


            var path = System.IO.Path.Combine(Application.streamingAssetsPath, "LUA/");
            _script = new Script(modules);
            _script.Options.ScriptLoader = new FileSystemScriptLoader()
            {
                ModulePaths = new string[] { path+"?", path + "?.lua" }
            };

            UserData.RegisterAssembly();
            _script.Globals["quill"]    = new QuillLuaData();
            _script.Globals["wait"]     = (Func<float, DynValue>)Wait;
            
            var filePaths = Directory.GetFiles(path, "*.lua");

            var total = string.Empty;
            foreach (var p in filePaths)
            {
                total += System.IO.File.ReadAllText(p);
            }
            _script.DoString(total);
            
            _script.Call(_script.Globals.Get("OnInit"));
            _updateResult = _script.Globals.Get("OnUpdate");

        }

        public static void Update()
        {
            if (_updateResult.Function != null)
            {
                _script.Call(_updateResult, Time.deltaTime);
            }
        }
 
        public static void MessagePost(MessageData data)
        {
            var dataProxy = new MessageDataProxy(data);
            
            _script.Call(_script.Globals["OnMessage"], dataProxy);
        }

        public static void Exit()
        {
            var result =  _script.Globals.Get("OnExit");
            if (result.Function != null)
            {
                _script.Call(result);
            }
        }

        public static DynValue Wait(float seconds)
        {
            Debug.Log("called: " + seconds);
            return DynValue.NewYieldReq(new[] 
            {
                DynValue.NewNumber(seconds) 
            });
        }

        private static IEnumerator WaitCO(float seconds)
        {
            Debug.Log("co started.");
            yield return new WaitForSeconds(seconds);
            Debug.Log("co ended.");
        }

        [MoonSharpUserData]
        private class QuillLuaData
        {
            public static int screenHeight          => Screen.height;
            public static int screenWidth           => Screen.width;

            public static QuillElementProxy mainRoot()
            {
                return new QuillElementProxy(Quill.mainCanvasElement);
            }

            public static QuillElementProxy empty()
            {
                var target = Quill.CreateEmpty();
                return new QuillElementProxy(target);
            }

            public static QuillLabelProxy label(string text)
            {
                var target = Quill.CreateLabel(text);
                return new QuillLabelProxy(target);
            }

            public static QuillBoxProxy box()
            {
                var target = Quill.CreateBox(Color.clear);
                return new QuillBoxProxy(target);
            }

            public static QuillButtonProxy button(string label)
            {
                var target = Quill.CreateButton(label);
                return new QuillButtonProxy(target);
            }

            public static void loadFont(string name, int size)
            {
                var font = Font.CreateDynamicFontFromOSFont(name, size);
                QuillLua.loadedFonts.Add(name, font);
            }

            public static void setDefaultFont(string name)
            {
                var font = QuillLua.loadedFonts[name];
                Quill.defaultFont = font;
            }

            public static void log(string log)
            {
                Debug.Log(log);
            }

            public static Table mousePosition()
            {
                var position = new Table(QuillLua.MainScript());
                var pos = Quill.MousePosition();
                position["x"] = pos.x;
                position["y"] = pos.y;

                return position;
            }

            public static DynValue waitForSeconds(float seconds)
            {
                Debug.Log($"LUA: Waiting for {seconds} seconds!");
                return DynValue.NewYieldReq(new[] 
                { 
                    DynValue.NewNumber(seconds)
                });
            }
        }
    }
}

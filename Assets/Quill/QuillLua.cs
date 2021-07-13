using QuillLib;
using UnityEngine;
using MoonSharp.Interpreter;
using System;
using System.IO;
using MoonSharp.Interpreter.Loaders;

namespace QuillLib.Lua
{
    public class QuillLua
    {
        private static Script _script;
        private static DynValue _updateResult;

        public static void Run()
        {
            var path = System.IO.Path.Combine(Application.streamingAssetsPath, "LUA/");
            _script = new Script();
            _script.Options.ScriptLoader = new FileSystemScriptLoader()
            {
                ModulePaths = new string[] { path+"?", path + "?.lua" }
            };

            UserData.RegisterAssembly();
            _script.Globals["quill"] = new QuillLuaData();
            
            var filePaths = Directory.GetFiles(path, "*.lua");

            var total = string.Empty;
            foreach (var p in filePaths)
            {
                total += System.IO.File.ReadAllText(p);
            }
            _script.DoString(total);
            
            _script.Call(_script.Globals["OnInit"]);
        }

        public static void Update()
        {
            _updateResult = _script.Globals.Get("OnUpdate");
            if (_updateResult.Function != null)
            {
                _script.Call(_updateResult, Time.deltaTime);
            }
        }
 
        public static void MessagePost(MessageData data)
        {
            var dataProxy = new MessageDataProxy()
            {
                id = data.id,
                data = data.data
            };
            
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

        [MoonSharpUserData]
        private class QuillLuaData
        {
            private static string _imageFolderPath = Application.streamingAssetsPath + "/IMAGE/";

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

            public static QuillButtonProxy button()
            {
                var target = Quill.CreateButton("");
                return new QuillButtonProxy(target);
            }

            public static void log(string log)
            {
                Debug.Log(log);
            }

            public static void setSprite(QuillBoxProxy box, string path)
            {
                var finalPath = _imageFolderPath + path;
                if (System.IO.File.Exists(finalPath))
                {
                    byte[] bytes = System.IO.File.ReadAllBytes(finalPath);
                    Texture2D tex = new Texture2D(2, 2);
                    tex.LoadImage(bytes);
 
                    var sprite = Sprite.Create(tex,
                                               new Rect(0.0f, 0.0f, tex.width, tex.height),
                                               new Vector2(0.5f, 0.5f),
                                               100.0f);

                    if (Quill.elements.ContainsKey(box.getId()))
                    {
                        var target = Quill.elements[box.getId()];
                        target.GetComponent<QuillBox>().sprite = sprite;
                    }
                }
            }
        }
    }
}

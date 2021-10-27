using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LunarBind
{

    //TODO: add better functionality
    public static class ScriptLibrary
    {
        private static readonly Dictionary<string, string> scripts = new Dictionary<string, string>();
        public static string GetScript(string name)
        {
            if(scripts.TryGetValue(name, out string value))
            {
                return value;
            }
            else
            {
                return null;
            }
        }

        public static void LoadScriptsFromDir(string dir, string searchPattern = "*.txt")
        {
            if (Directory.Exists(dir))
            {
                var files = Directory.GetFiles(dir, searchPattern);
                foreach (var fileName in files)
                {
                    var text = File.ReadAllText(fileName);
                    scripts.Add(Path.GetFileNameWithoutExtension(fileName), text);
                }
            }
        }
        public static void InitializeForUnity(Dictionary<string,string> codeToMap)
        { 
           Script.DefaultOptions.ScriptLoader = new MoonSharp.Interpreter.Loaders.UnityAssetsScriptLoader(codeToMap);
        }
    }
}

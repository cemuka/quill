using QuillLib;
using UnityEngine;
using QuillLib.Lua;

public class AppStartup : MonoBehaviour
{
    private void Start()
    {
        Application.targetFrameRate = 30;

        Quill.Init();
        QuillLua.Run();


        // var label = Quill.CreateLabel("hello world");
        // label.SetSize(100, 30);
        // label.SetPosition(100, 100);
        // Quill.mainRoot.Add(label);
    }

    private void Update()
    {
        QuillLua.Update();
    }
}
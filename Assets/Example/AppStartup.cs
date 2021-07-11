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
    }

    private void Update()
    {
        QuillLua.Update();
    }
}
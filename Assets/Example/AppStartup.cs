using QuillLib;
using UnityEngine;
using QuillLib.Lua;
using System;

public class AppStartup : MonoBehaviour
{
    private void Start()
    {
        Quill.Init();
        QuillLua.Run();

        Quill.message.Register(PressSpace);
    }

    private void PressSpace(MessageData data)
    {
        Debug.Log(data.id);
    }

    private void Update()
    {
        // QuillLua.Update();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var data = new MessageData()
            {
                id = "space"
            };
            Quill.message.Post(data);
            QuillLua.MessagePost(data);
        }
    }

    private void OnDestroy()
    {
        // QuillLua.Exit();
    }
}
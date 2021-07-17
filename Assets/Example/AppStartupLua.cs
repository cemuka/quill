using QuillLib;
using UnityEngine;
using QuillLib.Lua;
using System;
using System.Collections.Generic;

public class AppStartupLua : MonoBehaviour
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
        QuillLua.Update();
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var data = new MessageData("space");
            data.container.Add("score", 3);
            data.container.Add("player", "cemuka");
            Quill.message.Post(data);
            QuillLua.MessagePost(data);
        }
    }

    private void OnDestroy()
    {
        // QuillLua.Exit();
    }
}
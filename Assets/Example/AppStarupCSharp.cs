
using QuillLib;
using UnityEngine;
using QuillLib.Lua;
using System;
using System.Collections.Generic;

public class AppStarupCSharp : MonoBehaviour
{
    private void Start()
    {
        Quill.Init();

        var button = Quill.CreateButton("my awesome button");
        button.box.color = Color.blue;
        button.label.color = Color.yellow;

        button.element.SetAnchoredPosition(200, -100);
    }
}
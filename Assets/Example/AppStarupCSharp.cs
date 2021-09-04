
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

        var button = Quill.CreateButton("hello button!");
        button.interactable = false;
    }
}

using QuillLib;
using UnityEngine;
using QuillLib.Lua;
using System;

public class AppStarupCSharp : MonoBehaviour
{
    private void Start()
    {
        Quill.Init();

        Quill.mainRoot.Add(Quill.CreateEmpty());
        Quill.mainRoot.Add(Quill.CreateBox(Color.red));
        Quill.mainRoot.Add(Quill.CreateButton("noice button"));
        Quill.mainRoot.Add(Quill.CreateLabel("label"));
    }
}
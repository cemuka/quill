
using QuillLib;
using UnityEngine;
using QuillLib.Lua;
using System;

public class AppStarupCSharp : MonoBehaviour
{
    private void Start()
    {
        Quill.Init();

        var label = Quill.CreateLabel("label");

        Quill.mainRoot.Add(Quill.CreateEmpty());
        Quill.mainRoot.Add(Quill.CreateBox(Color.red));
        Quill.mainRoot.Add(Quill.CreateButton("noice button"));
        Quill.mainRoot.Add(label);

        var fonts = Font.GetOSInstalledFontNames();
        foreach (var item in fonts)
        {
            Debug.Log(item);
        }

        var firaCode = Font.CreateDynamicFontFromOSFont("Fira Code", 24);
        label.font = firaCode;
    }
}
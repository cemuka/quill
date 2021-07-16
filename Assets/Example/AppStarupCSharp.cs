
using QuillLib;
using UnityEngine;
using QuillLib.Lua;
using System;

public class AppStarupCSharp : MonoBehaviour
{
    private void Start()
    {
        Quill.Init();

        var box     = Quill.CreateBox(Color.red);
        box.SetSize(300, 300);

        var label   = Quill.CreateLabel("hello world");
        box.root.Add(label);


        label.SetSize(200, 200);
        label.SetPivot(0.5f, 0.5f);
        label.SetPosition(150, -150);
        label.alignment = TextAnchor.MiddleCenter;


        box.SetPosition(300, 300);

        Quill.mainRoot.Add(box);
    }
}

using QuillLib;
using UnityEngine;
using QuillLib.Lua;
using System;

public class AppStarupCSharp : MonoBehaviour
{
    QuillButton button;

    private void Start()
    {
        Quill.Init();

        var box     = Quill.CreateBox(Color.red);
        box.SetSize(300, 300);
        Quill.mainRoot.Add(box);

        button   = Quill.CreateButton("hello button");
        
        button.box.color = Color.blue;
        button.box.SetSize(200, 40);
        button.box.SetPosition(50, -50);
        button.onClick.AddListener(OnButtonClick);

        box.root.Add(button);
        box.SetPosition(300, -300);
    }
    
    private void OnButtonClick()
    {
        button.label.text = "this button clicked";
    }
}
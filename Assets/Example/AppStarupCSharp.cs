
using QuillLib;
using UnityEngine;
using QuillLib.Lua;
using System;
using System.Collections.Generic;

public class AppStarupCSharp : MonoBehaviour
{
    int score;
    private void Update()
    {
        score += 5;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var data = new MessageData("score");
            data.container.Add("playerName", "John");
            data.container.Add("playerScore", score);
            Quill.message.Post(data);
        }
    }

    QuillLabel scoreLabel;
    int currentScore;

    private void Start()
    {
        scoreLabel = Quill.CreateLabel("score: " + currentScore);
        Quill.message.Register(MessageListener);
    }

    private void MessageListener(MessageData data)
    {
        if (data.id == "score")
        {
            //  handle event
        }
    }
}
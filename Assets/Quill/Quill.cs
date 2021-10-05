using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QuillLib
{
    public class Quill
    {
        public static Canvas        mainCanvas          { get; private set; }
        public static QuillElement  mainCanvasElement   { get; private set; }
        public static Message       message             { get; private set; }
        public static Font          defaultFont;


        public static void Init()
        {
            message = new Message();

            var canvasGO = new GameObject("QuillCanvas");
            mainCanvas = canvasGO.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();

            if (GameObject.FindObjectOfType<EventSystem>() == null)
            {    
                var eventSystemGO = new GameObject("EventSystem");
                eventSystemGO.AddComponent<EventSystem>();
                eventSystemGO.AddComponent<StandaloneInputModule>();
            }

            mainCanvasElement = mainCanvas.gameObject.AddComponent<QuillElement>();
            mainCanvasElement.root = new ElementRoot();
            mainCanvasElement.root.rectTransform = (RectTransform)mainCanvas.transform;

            defaultFont =  Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        }
     
        public static QuillElement  CreateEmpty()
        {
            var elementGO       = new GameObject("Empty");
            var element         = elementGO.GetComponent<QuillElement>();
            
            if (element == null)
            {
                element         = elementGO.AddComponent<QuillElement>();
            }

            element.root = new ElementRoot();
            element.root.rectTransform = elementGO.AddComponent<RectTransform>();
            
            element.root.rectTransform.sizeDelta = new Vector2(100, 30);
            mainCanvasElement.root.AddChild(element);
            element.ResetTransform();
            return element;
        }

        public static QuillLabel    CreateLabel(string text)
        {
            var element = CreateEmpty();
            element.name = "Label";
            var label = element.gameObject.AddComponent<QuillLabel>();
            label.element = element;
            label.text = text;
            label.font = defaultFont;

            return label;
        }

        public static QuillBox      CreateBox(Color color)
        {
            var element         = CreateEmpty();
            element.name        = "Box";
            
            var box             = element.gameObject.AddComponent<QuillBox>();
            box.element         = element;
            box.color           = color;

            return box;
        }

        public static QuillButton   CreateButton(string text)
        {
            var element         = CreateEmpty();
            element.name        = "Button";

            var button          = element.gameObject.AddComponent<QuillButton>();
            button.box          = element.gameObject.AddComponent<QuillBox>();
            button.element      = element;
            button.box.element  = element;
            button.targetGraphic= button.box;

            button.label        = CreateLabel(text);
            button.root.AddChild(button.label);
            button.label.alignment  = TextAnchor.MiddleCenter;
            button.label.StretchToParentContainer();


            return button;
        }


        public static Vector2       MousePosition()
        {
            Vector2 pos = Vector2.zero;
            
            RectTransformUtility.ScreenPointToLocalPointInRectangle(mainCanvasElement.root.rectTransform,
                                                                    Input.mousePosition,
                                                                    null,
                                                                    out pos);

            //  valid when canvas is overlay mode
            pos.x = pos.x + Screen.width/2;
            pos.y = pos.y - Screen.height/2;

            return pos;
        }
    }
}
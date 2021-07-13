using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QuillLib
{
    public class Quill
    {
        public static Canvas        mainCanvas;
        public static ElementRoot   mainRoot => mainCanvasElement.root;
        public static QuillElement  mainCanvasElement;
        public static Message       message;
        public static Dictionary<int, QuillElement> elements;
        
        
        private static Font _defaultFont;
        private static int _maxId;
        private static int SetId()
        {
            return _maxId++;
        }


        public static void Init()
        {
            elements = new Dictionary<int, QuillElement>();
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
            mainCanvasElement.id = -1;
            mainCanvasElement.root = new ElementRoot();
            mainCanvasElement.root.rectTransform = (RectTransform)mainCanvas.transform;

            _defaultFont =  Resources.GetBuiltinResource(typeof(Font), "Arial.ttf") as Font;
        }
     
        public static QuillElement  CreateEmpty()
        {
            var elementGO       = new GameObject("Empty");
            var element         = elementGO.GetComponent<QuillElement>();
            
            if (element == null)
            {
                element         = elementGO.AddComponent<QuillElement>();
            }

            element.id = SetId();
            elements.Add(element.id, element);

            element.root = new ElementRoot();
            element.root.rectTransform = elementGO.AddComponent<RectTransform>();
            
            element.SetDefaultTransformValues();
            element.root.rectTransform.sizeDelta = new Vector2(100, 30);
            element.root.rectTransform.anchoredPosition = Vector2.zero;
            return element;
        }

        public static QuillLabel    CreateLabel(string text)
        {
            var element = CreateEmpty();
            element.name = "Label";
            var label = element.gameObject.AddComponent<QuillLabel>();
            label.element = element;
            label.text = text;
            label.font = _defaultFont;

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
            button.label        = CreateLabel(text);
            button.label.transform.SetParent(button.transform);
            button.targetGraphic= button.box;


            return button;
        }

    }
}
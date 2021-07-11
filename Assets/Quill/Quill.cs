using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace QuillLib
{
    public class Quill
    {
        public static Canvas mainCanvas;
        public static ElementRoot mainRoot;

        public static Dictionary<int, QuillElement> elements;
        private static int _maxId;

        public static void Init()
        {
            elements = new Dictionary<int, QuillElement>();

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

            mainRoot = new ElementRoot();
            mainRoot.rectTransform = (RectTransform)mainCanvas.transform;
        }
     
        private static int SetId()
        {
            return _maxId++;
        }

        private static QuillElement MakeElement(GameObject elementGO)
        {
            var element = elementGO.GetComponent<QuillElement>();
            if (element == null)
            {
                element = elementGO.AddComponent<QuillElement>();
            }

            element.id = SetId();
            elements.Add(element.id, element);

            element.root = new ElementRoot();
            element.root.rectTransform = elementGO.GetComponent<RectTransform>();
            if (element.root.rectTransform == null)
            {
                element.root.rectTransform = elementGO.AddComponent<RectTransform>();
            }
            
            element.SetDefaultTransformValues();
            element.root.rectTransform.sizeDelta = new Vector2(100, 30);
            element.root.rectTransform.anchoredPosition = Vector2.zero;
            return element;
        }



        public static QuillElement  CreateEmpty()
        {
            var elementGO           = new GameObject("Empty");
            //  elementGO.transform.SetParent(mainRoot.rectTransform);
            QuillElement element    = MakeElement(elementGO);

            return element;
        }

        public static QuillLabel    CreateLabel(string text)
        {
            var elementGO           = TMP_DefaultControls.CreateText(new TMP_DefaultControls.Resources());
            var label               = elementGO.AddComponent<QuillLabel>();
            
            var element             = MakeElement(elementGO);
            element.name            = "Label";
            
            label.Text              = elementGO.GetComponent<TMP_Text>();
            label.Text.text         = text;

            return label;
        }

        public static QuillBox      CreateBox(Color color)
        {
            var elementGO           = new GameObject("Box");
            var box                 = elementGO.AddComponent<QuillBox>();
            
            var element             = MakeElement(elementGO);
            
            box.boxImage            = elementGO.AddComponent<Image>();
            box.SetColor(color);

            return box;
        }
    
    }
}
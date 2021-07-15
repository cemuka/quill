using System;
using System.Collections.Generic;
using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

namespace QuillLib.Lua
{
    [MoonSharpUserData]
    public class MessageDataProxy
    {
        public string id;
        public Table container;
        public MessageDataProxy(MessageData data)
        {
            id = data.id;
            container = new Table(QuillLua.MainScript());
            foreach (var item in data.container)
            {
                container[item.Key] = item.Value;
            }
        }
    }
    [MoonSharpUserData]
    public class QuillElementProxy
    {
        private IQuillElement _target;

        public QuillElementProxy(IQuillElement element)
        {
            _target = element;
        }

        public int getId()
        {
            return _target.id;
        }

        public void destroy()
        {
            if (Quill.elements.ContainsKey(_target.id))
            {
                Quill.elements.Remove(_target.id);
                MonoBehaviour.Destroy(_target.root.rectTransform.gameObject);
            }
        }

        public void addChild(QuillElementProxy target)
        {
            if (Quill.elements.ContainsKey(target.getId()))
            {
                var element = Quill.elements[target.getId()];
                _target.root.Add(element);
            }
        }

        public void setPivot(float x, float y)
        {
            _target.root.rectTransform.pivot = new Vector2(x, y);
        }

        public void setSize(float x, float y)
        {
            _target.root.rectTransform.sizeDelta = new Vector2(x, y);
        }

        public void setPosition(float x, float y)
        {
            _target.root.rectTransform.anchoredPosition = new Vector2(x, y);
        }

        public void setAnchorsMin(float x, float y){ _target.root.rectTransform.anchorMin = new Vector2(x, y); }
        public void setAnchorsMax(float x, float y){ _target.root.rectTransform.anchorMax = new Vector2(x, y); }

        public void setDefaultTransformValues()
        {
            setPivot(0, 1);
            setAnchorsMin(0,1);
            setAnchorsMax(0,1);
        }
    }

    [MoonSharpUserData]
    public class QuillLabelProxy : QuillElementProxy
    {
        private QuillLabel _target;

        [MoonSharpHidden]
        public QuillLabelProxy(QuillLabel label): base(label)
        {
            _target = label;
        }

        public string getText()
        {
            return _target.text;
        }

        public void setText(string text)
        {
            _target.text = text;
        }

        public void setColor(float r, float g, float b) 
        {
            _target.color = new Color(r,g,b);
        }

        public void setColor(float r, float g, float b, float a) 
        {
            _target.color = new Color(r,g,b,a);
        }

        public void setColor(Table color)
        {
            var r = (float)color.Get("r").Number;
            var g = (float)color.Get("g").Number;
            var b = (float)color.Get("b").Number;
             
            var alpha = color.Get("a");
            float a = 1;

            if (alpha.IsNotNil())
            {
                a = (float)alpha.Number;
            }

            setColor(r,g,b,a);
        }

    }

    [MoonSharpUserData]
    public class QuillBoxProxy : QuillElementProxy
    {
        private QuillBox _target;

        [MoonSharpHidden]
        public QuillBoxProxy(QuillBox box) : base(box)
        {
            _target = box;
        }

        public void setColor(float r, float g, float b) 
        {
            _target.color = new Color(r,g,b);
        }

        public void setColor(float r, float g, float b, float a) 
        {
            _target.color = new Color(r,g,b,a);
        }

        public void setColor(Table color)
        {
            var r = (float)color.Get("r").Number;
            var g = (float)color.Get("g").Number;
            var b = (float)color.Get("b").Number;
             
            var alpha = color.Get("a");
            float a = 1;

            if (alpha.IsNotNil())
            {
                a = (float)alpha.Number;
            }

            setColor(r,g,b,a);
        }

        public void sprite(string path, Table options)
        {
            var finalPath = QuillLua.IMG_FOLDER_PATH + path;
            if (System.IO.File.Exists(finalPath))
            {
                if (Quill.elements.ContainsKey(_target.id))
                {
                    // Vector2 pivot
                    // float pixelsPerUnit
                    // uint extrude
                    // SpriteMeshType meshType
                    // Vector4 border
                    var filterMode       = (FilterMode)Enum.Parse(typeof(FilterMode), options.Get("filterMode").String, true);
                    var pivot           = new Vector2(  (float)options.Get("pivotX").Number, 
                                                        (float)options.Get("pivotY").Number);
                    var extrude         =               (uint)options.Get("extrude").Number;
                    var pixelsPerUnit   =               (float)options.Get("pixelsPerUnit").Number;
                    var border          = new Vector4(  (float)options.Get("borderX").Number,
                                                        (float)options.Get("borderY").Number,
                                                        (float)options.Get("borderZ").Number,
                                                        (float)options.Get("borderW").Number);


                    CreateSprite(finalPath, filterMode, pivot, pixelsPerUnit, extrude, border);
                }
            }
        }

        public void sprite(string path)
        {
            sprite(path, DefaultSpriteOptions());
        }

        public void setImageType(string imageType)
        {
            var type = (Image.Type)Enum.Parse(typeof(Image.Type), imageType, true);
            _target.type = type;
        }

        private static Table DefaultSpriteOptions()
        {
            var options = new Table(QuillLua.MainScript());

            options["filterMode"]        = "Bilinear";
            options["pivotX"]           = 0.5f;
            options["pivotY"]           = 0.5f;
            options["extrude"]          = 0f;
            options["pixelsPerUnit"]    = 100f;
            options["borderX"]          = 0f;
            options["borderY"]          = 0f;
            options["borderZ"]          = 0f;
            options["borderW"]          = 0f;

            return options;
        }
    
        private void CreateSprite(string finalPath, FilterMode filterMode, Vector2 pivot, float pixelsPerUnit, uint extrude, Vector4 border)
        {
            byte[] bytes = System.IO.File.ReadAllBytes(finalPath);
            var tex = new Texture2D(2,2);
            tex.LoadImage(bytes);
            tex.filterMode = filterMode;
            var rect = new Rect(0, 0, tex.width, tex.height);

            var sprite = Sprite.Create(tex, rect, pivot, pixelsPerUnit, extrude, SpriteMeshType.FullRect, border);

            var target = Quill.elements[_target.id];
            target.GetComponent<QuillBox>().sprite = sprite;
        }
    }

    [MoonSharpUserData]
    public class QuillButtonProxy : QuillElementProxy
    {
        public QuillBoxProxy box;
        public QuillLabelProxy label;
        public event Action onClick;

        private QuillButton _target;

        [MoonSharpHidden]
        public QuillButtonProxy(QuillButton button) : base(button)
        {
            _target     = button;
            label       = new QuillLabelProxy(_target.label);
            box         = new QuillBoxProxy(_target.box);
            
            _target.onClick.AddListener(
                () => onClick?.Invoke()
            );
        }
    }

    
}

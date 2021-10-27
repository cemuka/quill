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

        [MoonSharpHidden]
        public QuillElementProxy(IQuillElement element)
        {
            _target = element;
        }
        
        [MoonSharpHidden]
        public IQuillElement Element => _target;

        public void show(){_target.RectTransform.gameObject.SetActive(true);}
        public void hide(){_target.RectTransform.gameObject.SetActive(false);}

        public void destroy()
        {
            MonoBehaviour.Destroy(_target.RectTransform.gameObject);
        }

        public void addChild(QuillElementProxy childElement)
        {
            _target.AddChild(childElement.Element);
        }

        public void setPivot(float x, float y)
        {
            _target.RectTransform.pivot = new Vector2(x, y);
        }

        public void setSize(float x, float y)
        {
            _target.RectTransform.sizeDelta = new Vector2(x, y);
        }

        public void setAnchoredPosition(float x, float y)
        {
            _target.RectTransform.anchoredPosition = new Vector2(x, y);
        }

        public void setAnchorsMin(float x, float y){ _target.RectTransform.anchorMin = new Vector2(x, y); }
        public void setAnchorsMax(float x, float y){ _target.RectTransform.anchorMax = new Vector2(x, y); }

        public void resetTransform()
        {
            _target.ResetTransform();
        }

        public void stretchToParent()
        {
            _target.StretchToParentContainer();
        }
    }

    [MoonSharpUserData]
    public class QuillLabelProxy : QuillElementProxy
    {
        private QuillLabel _target;

        [MoonSharpHidden]
        public QuillLabelProxy(QuillLabel label): base(label.element)
        {
            _target = label;
            _target.font = Quill.defaultFont;
        }

        public string getText()
        {
            return _target.text;
        }

        public void setText(string text)
        {
            _target.text = text;
        }

        public string getFont()
        {
            return _target.font.name;
        }

        public void setFont(string fontName)
        {
            _target.font = QuillLua.loadedFonts[fontName];
        }

        public Table getColor()
        {
            var color = new Table(QuillLua.MainScript());
            color["r"] = _target.color.r;
            color["g"] = _target.color.g;
            color["b"] = _target.color.b;
            color["a"] = _target.color.a;

            return color;
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

        public void setTextAnchor(string textAnchor)
        {
            _target.alignment   = (TextAnchor)Enum.Parse(typeof(TextAnchor), textAnchor, true);
        }

    }

    [MoonSharpUserData]
    public class QuillBoxProxy : QuillElementProxy
    {
        private const string FILTER_MODE         = "filterMode";
        private const string PIVOT_X             = "pivotX";
        private const string PIVOT_Y             = "pivotY";
        private const string EXTRUDE             = "extrude";
        private const string PIXELS_PER_UNIT     = "pixelsPerUnit";
        private const string BORDER_X            = "borderX";
        private const string BORDER_Y            = "borderY";
        private const string BORDER_Z            = "borderZ";
        private const string BORDER_W            = "borderW";

        private QuillBox _target;

        [MoonSharpHidden]
        public QuillBoxProxy(QuillBox box) : base(box.element)
        {
            _target = box;
        }

        public Table getColor()
        {
            var color = new Table(QuillLua.MainScript());
            color["r"] = _target.color.r;
            color["g"] = _target.color.g;
            color["b"] = _target.color.b;
            color["a"] = _target.color.a;

            return color;
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
                var defaults        = DefaultSpriteOptions();

                // Vector2 pivot
                // float pixelsPerUnit
                // uint extrude
                // SpriteMeshType meshType
                // Vector4 border

                var pX              = options.Get(PIVOT_X);
                var pivotX          = pX.IsNil() ? (float)defaults.Get(PIVOT_X).Number : (float)pX.Number;

                var pY              = options.Get(PIVOT_Y);
                var pivotY          = pY.IsNil() ? (float)defaults.Get(PIVOT_X).Number : (float)pY.Number;
                var pivot           = new Vector2(pivotX, pivotY);

                var  fm             = options.Get(FILTER_MODE);
                string fmParsed     = fm.IsNil() ? (string)defaults.Get(FILTER_MODE).String : (string)fm.String;
                var filterMode       = (FilterMode)Enum.Parse(typeof(FilterMode), fmParsed, true);

                var ext             = options.Get(EXTRUDE);
                var extrude         = ext.IsNil() ? (uint)defaults.Get(EXTRUDE).Number : (uint)ext.Number;
                
                var ppu             = options.Get(PIXELS_PER_UNIT);
                var pixelsPerUnit   = ppu.IsNil() ? (float)defaults.Get(PIXELS_PER_UNIT).Number : (float)ppu.Number;

                var bX              = options.Get(BORDER_X);
                var borderX         = bX.IsNil() ? (float)defaults.Get(BORDER_X).Number : (float)bX.Number;
                
                var bY              = options.Get(BORDER_Y);
                var borderY         = bY.IsNil() ? (float)defaults.Get(BORDER_Y).Number : (float)bY.Number;
                
                var bZ              = options.Get(BORDER_Z);
                var borderZ         = bZ.IsNil() ? (float)defaults.Get(BORDER_Z).Number : (float)bZ.Number;
                
                var bW              = options.Get(BORDER_W);
                var borderW         = bW.IsNil() ? (float)defaults.Get(BORDER_W).Number : (float)bW.Number;
                var border          = new Vector4( borderX, borderY, borderZ, borderW );

                CreateSprite(finalPath, filterMode, pivot, pixelsPerUnit, extrude, border);
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

            // Vector2 pivot
            // float pixelsPerUnit
            // uint extrude
            // SpriteMeshType meshType
            // Vector4 border

            options[FILTER_MODE]        = "Bilinear";
            options[PIVOT_X]            = 0.5f;
            options[PIVOT_Y]            = 0.5f;
            options[EXTRUDE]            = 0f;
            options[PIXELS_PER_UNIT]    = 100f;
            options[BORDER_X]           = 0f;
            options[BORDER_Y]           = 0f;
            options[BORDER_Z]           = 0f;
            options[BORDER_W]           = 0f;

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

            _target.GetComponent<QuillBox>().sprite = sprite;
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
        public QuillButtonProxy(QuillButton button) : base(button.element)
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

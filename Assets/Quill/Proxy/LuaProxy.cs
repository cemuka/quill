using System;
using MoonSharp.Interpreter;
using UnityEngine;

namespace QuillLib.Lua
{
    [MoonSharpUserData]
    public class MessageDataProxy
    {
        public string id;
        public object data;
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
            MonoBehaviour.Destroy(_target.root.rectTransform.gameObject);
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

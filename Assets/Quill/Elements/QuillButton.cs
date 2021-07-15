using UnityEngine;
using UnityEngine.UI;
namespace QuillLib
{
    public class QuillButton : Button, IQuillElement
    {
        public QuillElement element;
        public QuillLabel label;
        public QuillBox box;        

        //  IQuillElement implementation
        public int id => element.id;
        public ElementRoot root => element.root; 
        
        public void SetPivot(float x, float y)
        {
            root.rectTransform.pivot = new Vector2(x, y);
        }

        public void SetSize(float x, float y)
        {
            root.rectTransform.sizeDelta = new Vector2(x, y);
        }

        public void SetPosition(float x, float y)
        {
            root.rectTransform.anchoredPosition = new Vector2(x, y);
        }

        public void SetAnchorsMin(float x, float y){ root.rectTransform.anchorMin = new Vector2(x, y); }
        public void SetAnchorsMax(float x, float y){ root.rectTransform.anchorMax = new Vector2(x, y); }

        public void SetDefaultTransformValues()
        {
            SetPivot(0, 1);
            SetAnchorsMin(0,1);
            SetAnchorsMax(0,1);
        }
    }
}
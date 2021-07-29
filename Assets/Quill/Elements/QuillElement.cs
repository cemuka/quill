using UnityEngine;

namespace QuillLib
{
    public interface IQuillElement
    {
        int id { get; }
        ElementRoot root { get; }
        void SetPivot(float x, float y);
        void SetSize(float x, float y);
        void SetPosition(float x, float y);
        void SetAnchorsMin(float x, float y);
        void SetAnchorsMax(float x, float y);
        void ResetTransform();
        void StretchToParentContainer();
    }

    public class QuillElement : MonoBehaviour, IQuillElement
    {
        public int id { get; set; }
        public ElementRoot root { get; set; }

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

        public void ResetTransform()
        {
            SetPivot(0, 1);
            SetAnchorsMin(0,1);
            SetAnchorsMax(0,1);
            SetPosition(0,0);
        }

        public void StretchToParentContainer()
        {
            SetAnchorsMin(0, 0);
            SetAnchorsMax(1, 1);
            SetPivot(0.5f, 0.5f);
            root.rectTransform.Left(0);
            root.rectTransform.Right(0);
            root.rectTransform.Top(0);
            root.rectTransform.Bottom(0);
        }
    }
    
    public class ElementRoot
    {
        public RectTransform rectTransform;

        public void Add(IQuillElement element)
        {
            element.root.rectTransform.SetParent(rectTransform);
            element.root.rectTransform.anchoredPosition = Vector2.zero;
        }
    }
}
using UnityEngine;

namespace QuillLib
{
    public interface IQuillElement
    {
        RectTransform RectTransform { get; }
        void AddChild(IQuillElement element);
        void SetPivot(float x, float y);
        void SetSize(float x, float y);
        void SetAnchoredPosition(float x, float y);
        void SetAnchorsMin(float x, float y);
        void SetAnchorsMax(float x, float y);
        void ResetTransform();
        void StretchToParentContainer();
    }

    public class QuillElement : MonoBehaviour, IQuillElement
    {
        public RectTransform RectTransform { get; set; }
        
        public void AddChild(IQuillElement element)
        {
            element.RectTransform.SetParent(RectTransform);
            element.RectTransform.anchoredPosition = Vector2.zero;
        }

        public void SetPivot(float x, float y)
        {
            RectTransform.pivot = new Vector2(x, y);
        }

        public void SetSize(float x, float y)
        {
            RectTransform.sizeDelta = new Vector2(x, y);
        }

        public void SetAnchoredPosition(float x, float y)
        {
            RectTransform.anchoredPosition = new Vector2(x, y);
        }

        public void SetAnchorsMin(float x, float y){ RectTransform.anchorMin = new Vector2(x, y); }
        public void SetAnchorsMax(float x, float y){ RectTransform.anchorMax = new Vector2(x, y); }

        public void ResetTransform()
        {
            SetPivot(0, 1);
            SetAnchorsMin(0,1);
            SetAnchorsMax(0,1);
            SetAnchoredPosition(0,0);
        }

        public void StretchToParentContainer()
        {
            SetAnchorsMin(0, 0);
            SetAnchorsMax(1, 1);
            SetPivot(0.5f, 0.5f);
            RectTransform.Left(0);
            RectTransform.Right(0);
            RectTransform.Top(0);
            RectTransform.Bottom(0);
        }
    }
}
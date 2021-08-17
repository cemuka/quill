using UnityEngine;
using UnityEngine.UI;
namespace QuillLib
{
    public class QuillBox : Image, IQuillElement
    {
        public QuillElement element;

        public ElementRoot root => element.root; 
        
        public void SetPivot(float x, float y)
        {
            element.SetPivot(x, y);
        }

        public void SetSize(float x, float y)
        {
            element.SetSize(x, y);
        }

        public void SetPosition(float x, float y)
        {
            element.SetPosition(x, y);
        }

        public void SetAnchorsMin(float x, float y){ element.SetAnchorsMin(x, y); }
        public void SetAnchorsMax(float x, float y){ element.SetAnchorsMin(x, y); }

        public void ResetTransform()
        {
            SetPivot(0, 1);
            SetAnchorsMin(0,1);
            SetAnchorsMax(0,1);
        }

        public void StretchToParentContainer()
        {
            element.StretchToParentContainer();
        }
    }
}
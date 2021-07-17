using MoonSharp.Interpreter;
using UnityEngine;
using UnityEngine.UI;

namespace QuillLib
{
    public class QuillLabel : Text, IQuillElement
    {
        public QuillElement element;



        //  IQuillElement implementation
        public int id => element.id;
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

        public void SetDefaultTransformValues()
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
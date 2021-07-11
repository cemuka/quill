using UnityEngine;
using UnityEngine.UI;
namespace QuillLib
{
    public class QuillBox : QuillElement
    {
        public Image boxImage;

        public void SetColor(Color color) 
        {
            boxImage.color = color;
        }
    }
}
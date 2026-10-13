using UnityEngine;
using UnityEngine.UI;

namespace FrecuenciaCero
{
    [RequireComponent(typeof(RawImage))]
    public sealed class MenuTitleInterference : MonoBehaviour
    {
        private RectTransform rect;
        private RawImage title;
        private Vector2 origin;
        private void Awake() { rect=GetComponent<RectTransform>();title=GetComponent<RawImage>();origin=rect.anchoredPosition; }
        private void Update()
        {
            float cycle=Time.unscaledTime%7.3f;
            bool interference=cycle>7.16f;
            rect.anchoredPosition=origin+(interference?new Vector2(Mathf.Sin(Time.unscaledTime*110f),0):Vector2.zero);
            title.color=new Color(1,1,1,interference?.94f:1);
        }
        private void OnDisable()
        {if(rect!=null)rect.anchoredPosition=origin;if(title!=null)title.color=Color.white;}
    }
}

using UnityEngine;

namespace FrecuenciaCero
{
    [ExecuteAlways]
    public sealed class MenuDesignScale : MonoBehaviour
    {
        private void LateUpdate()
        {
            var parent=transform.parent as RectTransform;
            if(parent!=null) transform.localScale=Vector3.one*Mathf.Min(parent.rect.width/1920f,parent.rect.height/1080f);
        }
    }
}

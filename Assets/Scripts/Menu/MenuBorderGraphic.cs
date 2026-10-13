using UnityEngine;
using UnityEngine.UI;

namespace FrecuenciaCero
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MenuBorderGraphic : MaskableGraphic
    {
        public Image edge;
        private void Update() { if(edge!=null)color=edge.color; }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();var rect=rectTransform.rect;
            Add(mesh,rect.xMin,rect.yMin,rect.width,1);Add(mesh,rect.xMin,rect.yMax-1,rect.width,1);
            Add(mesh,rect.xMin,rect.yMin,1,rect.height);Add(mesh,rect.xMax-1,rect.yMin,1,rect.height);
        }
        private void Add(VertexHelper mesh,float x,float y,float w,float h)
        {
            int n=mesh.currentVertCount;
            mesh.AddVert(new Vector3(x,y),color,Vector2.zero);mesh.AddVert(new Vector3(x,y+h),color,Vector2.zero);
            mesh.AddVert(new Vector3(x+w,y+h),color,Vector2.zero);mesh.AddVert(new Vector3(x+w,y),color,Vector2.zero);
            mesh.AddTriangle(n,n+1,n+2);mesh.AddTriangle(n,n+2,n+3);
        }
    }
}

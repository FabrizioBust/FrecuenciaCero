using UnityEngine;
using UnityEngine.UI;

namespace FrecuenciaCero
{
    public sealed class ChapterVignetteGraphic : MaskableGraphic
    {
        protected override void Awake(){base.Awake();raycastTarget=false;}
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();Rect area=rectTransform.rect;
            for(int ring=0;ring<12;ring++)
            {
                float outer=ring/12f,inner=(ring+1)/12f;
                Rect a=Inset(area,outer),b=Inset(area,inner);
                Color ca=new Color(0,0,0,Mathf.Pow(1-outer,2)*.82f),cb=new Color(0,0,0,Mathf.Pow(1-inner,2)*.82f);
                Quad(mesh,new Vector2(a.xMin,a.yMin),new Vector2(a.xMax,a.yMin),new Vector2(b.xMax,b.yMin),new Vector2(b.xMin,b.yMin),ca,cb);
                Quad(mesh,new Vector2(a.xMax,a.yMin),new Vector2(a.xMax,a.yMax),new Vector2(b.xMax,b.yMax),new Vector2(b.xMax,b.yMin),ca,cb);
                Quad(mesh,new Vector2(a.xMax,a.yMax),new Vector2(a.xMin,a.yMax),new Vector2(b.xMin,b.yMax),new Vector2(b.xMax,b.yMax),ca,cb);
                Quad(mesh,new Vector2(a.xMin,a.yMax),new Vector2(a.xMin,a.yMin),new Vector2(b.xMin,b.yMin),new Vector2(b.xMin,b.yMax),ca,cb);
            }
        }
        private static Rect Inset(Rect r,float t)=>new Rect(r.x+r.width*.28f*t,r.y+r.height*.28f*t,r.width*(1-.56f*t),r.height*(1-.56f*t));
        private static void Quad(VertexHelper mesh,Vector2 a,Vector2 b,Vector2 c,Vector2 d,Color edge,Color middle)
        {
            int start=mesh.currentVertCount;mesh.AddVert(a,edge,Vector2.zero);mesh.AddVert(b,edge,Vector2.zero);mesh.AddVert(c,middle,Vector2.zero);mesh.AddVert(d,middle,Vector2.zero);
            mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
        }
    }
}

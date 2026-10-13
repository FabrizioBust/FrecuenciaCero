using UnityEngine;
using UnityEngine.UI;

namespace FrecuenciaCero
{
    // Small UI mesh overlays: no camera motion, textures or gameplay dependencies.
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class MenuAtmosphereGraphic : MaskableGraphic
    {
        public bool titleOnly;
        protected override void Awake() { base.Awake(); raycastTarget = false; }
        private void Update() { SetVerticesDirty(); }
        protected override void OnPopulateMesh(VertexHelper mesh)
        {
            mesh.Clear();
            float time = Application.isPlaying ? Time.unscaledTime : 0;
            if (titleOnly)
            {
                // Sparse, fixed dark cuts produce a worn title; a few move briefly like radio interference.
                for (int i = 0; i < 46; i++)
                {
                    float x = 168 + (i * 137 % 550);
                    float y = i % 2 == 0 ? 905 + (i * 23 % 68) : 752 + (i * 31 % 104);
                    Quad(mesh, x, y, 3 + i * 11 % 40, i % 4 == 0 ? 3 : 1, new Color(.01f,.015f,.02f,.7f));
                }
                bool glitch = time % 7.3f > 7.16f;
                Quad(mesh, 185, 877, glitch ? 540 : 95, 1, new Color(.9f,.13f,.15f,glitch ? .5f : .25f));
                // Thin radio pulse decoration beside CERO, within the title composition only.
                for (int i = 0; i < 115; i++)
                {
                    float x = 165 + i * 5;
                    if (x > 275 && x < 600) continue;
                    float pulse = Mathf.Exp(-Mathf.Pow((i % 40 - 20) / 4f, 2)) * Mathf.Sin(i * 1.8f) * 20;
                    Quad(mesh, x, 796 + pulse, 6, 1, new Color(.85f,.1f,.12f,.65f));
                }
                return;
            }
            // Long translucent banks drifting across the distant forest, keeping buttons clear.
            for (int bank = 0; bank < 3; bank++)
            for (int i = 0; i < 40; i++)
            {
                float x = 825 + i * 28;
                float y = 315 + bank * 100 + Mathf.Sin(i * .14f + time * .035f + bank) * 28;
                float nextY = 315 + bank * 100 + Mathf.Sin((i + 1) * .14f + time * .035f + bank) * 28;
                float alpha = Mathf.Sin(i / 40f * Mathf.PI) * .018f;
                float nextAlpha = Mathf.Sin((i + 1) / 40f * Mathf.PI) * .018f;
                GradientQuad(mesh, x, y, nextY, 28, 75, new Color(.42f,.54f,.62f,alpha),
                    new Color(.42f,.54f,.62f,nextAlpha));
            }
            float light = .35f + .65f * Mathf.Pow(Mathf.Max(0, Mathf.Sin(time * 1.65f)), 4);
            // Position corresponds to the beacon in Menu_FondoLimpio.png.
            for (int ring = 7; ring >= 1; ring--)
                Disc(mesh, new Vector2(1495,905), ring * 3, new Color(1,.025f,.04f,light * .022f));
            Disc(mesh, new Vector2(1495,905), 4, new Color(1,.14f,.16f,light));
        }
        private static void Quad(VertexHelper mesh, float x, float y, float w, float h, Color tint)
        {
            int start = mesh.currentVertCount;
            mesh.AddVert(new Vector3(x,y),tint,Vector2.zero);
            mesh.AddVert(new Vector3(x,y+h),tint,Vector2.zero);
            mesh.AddVert(new Vector3(x+w,y+h),tint,Vector2.zero);
            mesh.AddVert(new Vector3(x+w,y),tint,Vector2.zero);
            mesh.AddTriangle(start,start+1,start+2); mesh.AddTriangle(start,start+2,start+3);
        }
        private static void GradientQuad(VertexHelper mesh, float x,float y,float rightY,float w,float h,Color tint,Color rightTint)
        {
            int start=mesh.currentVertCount;
            Color clear=new Color(tint.r,tint.g,tint.b,0);
            mesh.AddVert(new Vector3(x,y),clear,Vector2.zero);
            mesh.AddVert(new Vector3(x,y+h*.5f),tint,Vector2.zero);
            mesh.AddVert(new Vector3(x+w,rightY+h*.5f),rightTint,Vector2.zero);
            mesh.AddVert(new Vector3(x+w,rightY),clear,Vector2.zero);
            mesh.AddVert(new Vector3(x,y+h),clear,Vector2.zero);
            mesh.AddVert(new Vector3(x+w,rightY+h),clear,Vector2.zero);
            mesh.AddTriangle(start,start+1,start+2);mesh.AddTriangle(start,start+2,start+3);
            mesh.AddTriangle(start+1,start+4,start+5);mesh.AddTriangle(start+1,start+5,start+2);
        }
        private static void Disc(VertexHelper mesh,Vector2 center,float radius,Color tint)
        {
            int start=mesh.currentVertCount;
            mesh.AddVert(center,tint,Vector2.zero);
            for(int i=0;i<16;i++)
            {
                float angle=i*Mathf.PI/8;
                mesh.AddVert(center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius,tint,Vector2.zero);
            }
            for(int i=0;i<16;i++)mesh.AddTriangle(start,start+1+i,start+1+(i+1)%16);
        }
    }
}

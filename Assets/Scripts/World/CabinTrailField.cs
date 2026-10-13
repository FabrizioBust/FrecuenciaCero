using UnityEngine;

namespace FrecuenciaCero
{
    public static class CabinTrailField
    {
        public static readonly Vector2[] Trail={new Vector2(-12,-14),new Vector2(-11,-9),new Vector2(-7,-6),new Vector2(-8,-2),
            new Vector2(-3,1),new Vector2(0,-.5f),new Vector2(4,2),new Vector2(5,6),new Vector2(6.1f,8.1f)};
        public static float PathDistance(Vector2 point)
        {
            float best=1000;
            for(int i=1;i<Trail.Length;i++){Vector2 a=Trail[i-1],d=Trail[i]-a;best=Mathf.Min(best,Vector2.Distance(point,a+d*Mathf.Clamp01(Vector2.Dot(point-a,d)/d.sqrMagnitude)));}
            return best;
        }
        public static float FogDensity(Vector2 point)
        {
            float path=Mathf.SmoothStep(0,1,(PathDistance(point)-.6f)/2);
            float cabin=Mathf.Exp(-(point-new Vector2(7,8.5f)).sqrMagnitude/12);
            return Mathf.Lerp(.32f,1,path)*(1-.4f*cabin);
        }
    }
}

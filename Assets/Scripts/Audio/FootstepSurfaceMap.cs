using UnityEngine;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent]
    public sealed class FootstepSurfaceMap : MonoBehaviour
    {
        public enum Terrain { ExteriorReten, AbandonedForest, Generator, CabinTrail, RetenInterior, CabinInterior }
        [SerializeField] private Terrain terrain;
        [SerializeField] private Vector2[] serviceTrail;
        public FootstepSurface At(Vector2 feet)
        {
            if(terrain==Terrain.RetenInterior)return FootstepSurface.RetenWood;
            if(terrain==Terrain.CabinInterior)return FootstepSurface.CabinWood;
            float distance;
            switch(terrain)
            {
                case Terrain.AbandonedForest:
                    distance=AbandonedForestField.PathDistance(feet);
                    if(Vector2.Distance(feet,new Vector2(2,-4))<3.4f||feet.x>-11.8f&&feet.x<-4.5f&&feet.y>5.3f&&feet.y<10.2f)return FootstepSurface.Dirt;
                    break;
                case Terrain.Generator:distance=(float)GeneratorTerrainField.PathDistance(feet.x,feet.y);break;
                case Terrain.CabinTrail:distance=CabinTrailField.PathDistance(feet);break;
                default:distance=Mathf.Min((float)OrganicTerrainField.PathDistance(feet.x,feet.y),RouteDistance(feet,serviceTrail));break;
            }
            return distance<.94f?FootstepSurface.Dirt:FootstepSurface.Grass;
        }
        private static float RouteDistance(Vector2 p,Vector2[] route)
        {
            float best=float.MaxValue;if(route==null)return best;
            for(int i=1;i<route.Length;i++){var a=route[i-1];var d=route[i]-a;best=Mathf.Min(best,Vector2.Distance(p,a+d*Mathf.Clamp01(Vector2.Dot(p-a,d)/Mathf.Max(.0001f,d.sqrMagnitude))));}return best;
        }
    }
}

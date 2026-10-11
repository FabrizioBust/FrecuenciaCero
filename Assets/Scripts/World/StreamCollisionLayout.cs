using UnityEngine;

namespace FrecuenciaCero
{
    [ExecuteAlways, DisallowMultipleComponent]
    public sealed class StreamCollisionLayout : MonoBehaviour
    {
        [SerializeField] private PolygonCollider2D north = null;
        [SerializeField] private PolygonCollider2D south = null;
        private void OnEnable() => Rebuild();
        public void Rebuild()
        {
            Apply(north, -2.25f, 14f);
            Apply(south, -14f, -3.75f);
        }
        private static void Apply(PolygonCollider2D collider, float min, float max)
        {
            if (collider == null) return;
            int count = Mathf.CeilToInt((max-min)/.125f)+1;
            var points = new Vector2[count*2];
            for(int i=0;i<count;i++)
            {
                float y=Mathf.Lerp(min,max,i/(float)(count-1));
                float center=(float)OrganicTerrainField.RiverCenter(y), width=(float)OrganicTerrainField.CollisionHalfWidth(y);
                points[i]=collider.transform.InverseTransformPoint(new Vector3(center-width,y,0));
                points[count*2-1-i]=collider.transform.InverseTransformPoint(new Vector3(center+width,y,0));
            }
            collider.isTrigger=false;
            bool same=collider.pathCount==1;
            Vector2[] previous=same?collider.GetPath(0):null;
            same= same && previous.Length==points.Length;
            for(int i=0;same && i<points.Length;i++) same=previous[i]==points[i];
            if(same)return;
            collider.pathCount=1;
            collider.SetPath(0,points);
        }
    }
}

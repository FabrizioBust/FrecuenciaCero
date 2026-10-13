using System.Collections.Generic;
using UnityEngine;

namespace FrecuenciaCero
{
    // A small bounded grid over real solid colliders. Rebuilt only when hearing/paths change.
    public static class ForestNavigation
    {
        private static readonly Vector2Int[] Neighbours = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        public static bool Clear(Vector2 a, Vector2 b, Collider2D owner)
        {
            foreach (var hit in Physics2D.CircleCastAll(a, .30f, (b-a).normalized, Vector2.Distance(a,b)))
                if (Solid(hit.collider, owner)) return false;
            return true;
        }
        private static bool Solid(Collider2D collider, Collider2D owner) => collider != null && !collider.isTrigger && collider != owner &&
            collider.GetComponentInParent<LeoMovement>() == null && collider.GetComponentInParent<Escarbador>() == null;
        public static List<Vector2> Find(Vector2 from, Vector2 goal, Rect bounds, Collider2D owner)
        {
            if (Clear(from,goal,owner)) return new List<Vector2>{goal};
            const float step = .5f;
            int width = Mathf.CeilToInt(bounds.width/step), height = Mathf.CeilToInt(bounds.height/step);
            int Index(Vector2 p) => Mathf.Clamp(Mathf.RoundToInt((p.y-bounds.yMin)/step),0,height-1)*width+
                Mathf.Clamp(Mathf.RoundToInt((p.x-bounds.xMin)/step),0,width-1);
            Vector2 Point(int i) => new Vector2(bounds.xMin+(i%width)*step,bounds.yMin+(i/width)*step);
            int start = Index(from), end = Index(goal);
            var queue = new Queue<int>(); var parent = new Dictionary<int,int>();
            queue.Enqueue(start); parent[start] = start;
            while(queue.Count > 0)
            {
                int current = queue.Dequeue(); if(current == end) break;
                int x=current%width,y=current/width;
                foreach(var d in Neighbours)
                {
                    int nx=x+d.x,ny=y+d.y;if(nx<0||ny<0||nx>=width||ny>=height)continue;
                    int next=ny*width+nx;if(parent.ContainsKey(next))continue;
                    Vector2 p=Point(next);bool blocked=false;
                    foreach(var c in Physics2D.OverlapCircleAll(p,.30f))if(Solid(c,owner)){blocked=true;break;}
                    if(blocked||!Clear(Point(current),p,owner))continue;
                    parent[next]=current;queue.Enqueue(next);
                }
            }
            if(!parent.ContainsKey(end))return new List<Vector2>();
            var route=new List<Vector2>();int node=end;
            while(node!=start){route.Add(Point(node));node=parent[node];}
            route.Reverse();
            if(Clear(Point(end),goal,owner))route.Add(goal);
            return route;
        }
    }
}

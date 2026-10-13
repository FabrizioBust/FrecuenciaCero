using UnityEngine;
using UnityEngine.Rendering;

namespace FrecuenciaCero
{
    // Visual only: uses the same reach/availability rules as the existing interaction controller.
    [DisallowMultipleComponent]
    public sealed class InteractionOutline : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer artwork;
        [SerializeField] private MeshRenderer meshArtwork;
        [SerializeField] private WorldInteraction target;
        [SerializeField] private Transform leo;
        [SerializeField] private NarrativeDirector director;
        [SerializeField] private Material outlineMaterial;
        [SerializeField] private float widthWorld = .025f;
        private InteractionProbe probe;
        private MeshRenderer outline;
        private Mesh mesh;
        private Sprite lastSprite;
        private MaterialPropertyBlock properties;
        private PlayerInteraction controller;
        public bool IsHighlighted => outline != null && outline.enabled;
        private void Start()
        {
            if (leo == null || outlineMaterial == null || (artwork == null && meshArtwork == null)) { enabled = false; return; }
            probe = new InteractionProbe(leo);
            controller = director != null ? director.GetComponent<PlayerInteraction>() : null;
            var obj = new GameObject("Contorno de silueta",typeof(MeshFilter),typeof(MeshRenderer));
            obj.transform.SetParent(artwork != null ? artwork.transform : meshArtwork.transform,false);
            outline = obj.GetComponent<MeshRenderer>();outline.sharedMaterial = outlineMaterial;
            outline.shadowCastingMode = ShadowCastingMode.Off;outline.receiveShadows = false;
            mesh = new Mesh { name = "Contorno temporal" };obj.GetComponent<MeshFilter>().sharedMesh = mesh;
            properties = new MaterialPropertyBlock();Rebuild();outline.enabled = false;
        }
        private void LateUpdate()
        {
            if(outline == null)return;
            if(artwork != null && artwork.sprite != lastSprite)Rebuild();
            Renderer source = artwork != null ? (Renderer)artwork : meshArtwork;
            outline.sortingLayerID = source.sortingLayerID;outline.sortingOrder = source.sortingOrder+1;
            var movement = leo.GetComponent<LeoMovement>();
            outline.enabled = source.enabled && source.gameObject.activeInHierarchy && target != null && target.isActiveAndEnabled &&
                !RetenTravel.IsLoading && director != null && !director.IsOpen && movement != null && movement.enabled && !movement.IsBlocked &&
                (controller == null || controller.FocusTarget == target) && target.IsAvailable(probe);
        }
        private void Rebuild()
        {
            if(artwork == null) { RebuildMesh(); return; }
            lastSprite = artwork.sprite;if(lastSprite == null)return;
            float ppu=lastSprite.pixelsPerUnit;
            float pad=widthWorld/Mathf.Max(.001f,Mathf.Abs(artwork.transform.lossyScale.x));
            Vector2 min=-lastSprite.pivot/ppu-Vector2.one*pad,max=(lastSprite.rect.size-lastSprite.pivot)/ppu+Vector2.one*pad;
            Rect rect=lastSprite.textureRect;var tex=lastSprite.texture;
            Vector2 uvMin=new Vector2((rect.x-pad*ppu)/tex.width,(rect.y-pad*ppu)/tex.height);
            Vector2 uvMax=new Vector2((rect.xMax+pad*ppu)/tex.width,(rect.yMax+pad*ppu)/tex.height);
            mesh.Clear();mesh.vertices=new[]{new Vector3(min.x,min.y),new Vector3(min.x,max.y),new Vector3(max.x,max.y),new Vector3(max.x,min.y)};
            mesh.uv=new[]{uvMin,new Vector2(uvMin.x,uvMax.y),uvMax,new Vector2(uvMax.x,uvMin.y)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.RecalculateBounds();
            properties.SetTexture("_MainTex",tex);
            properties.SetVector("_UVRect",new Vector4(rect.x/tex.width,rect.y/tex.height,rect.xMax/tex.width,rect.yMax/tex.height));
            properties.SetVector("_OutlineStep",new Vector4(pad*ppu/tex.width,pad*ppu/tex.height,0,0));
            outline.SetPropertyBlock(properties);
        }
        private void RebuildMesh()
        {
            var original = meshArtwork.GetComponent<MeshFilter>().sharedMesh;
            mesh.Clear();mesh.vertices = original.vertices;mesh.triangles = original.triangles;mesh.uv = original.uv;
            var tex = meshArtwork.sharedMaterial.mainTexture;
            var uv = mesh.uv;Vector2 min=Vector2.one*float.PositiveInfinity,max=Vector2.one*float.NegativeInfinity;
            foreach(var p in uv){min=Vector2.Min(min,p);max=Vector2.Max(max,p);}
            var vertices=mesh.vertices;var bounds=original.bounds;
            float pad=widthWorld/Mathf.Max(.001f,Mathf.Abs(meshArtwork.transform.lossyScale.x));
            for(int i=0;i<vertices.Length;i++)
            {
                Vector3 sign=vertices[i]-bounds.center;
                vertices[i]+=new Vector3(Mathf.Sign(sign.x)*pad,Mathf.Sign(sign.y)*pad,0);
                uv[i]+=new Vector2(Mathf.Sign(sign.x)*pad/Mathf.Max(.01f,bounds.size.x)*(max.x-min.x),Mathf.Sign(sign.y)*pad/Mathf.Max(.01f,bounds.size.y)*(max.y-min.y));
            }
            mesh.vertices=vertices;mesh.uv=uv;mesh.RecalculateBounds();
            properties.SetTexture("_MainTex",tex);properties.SetVector("_UVRect",new Vector4(min.x,min.y,max.x,max.y));
            properties.SetVector("_OutlineStep",new Vector4(pad/bounds.size.x*(max.x-min.x),pad/bounds.size.y*(max.y-min.y),0,0));
            outline.SetPropertyBlock(properties);
        }
        private void OnDisable(){if(outline != null)outline.enabled=false;}
        private void OnDestroy(){if(mesh != null)Destroy(mesh);}
    }
}

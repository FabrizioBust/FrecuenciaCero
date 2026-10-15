using UnityEngine;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent,RequireComponent(typeof(AudioSource))]
    public sealed class LocalizedRiverAudio : MonoBehaviour
    {
        [SerializeField] private Transform leo;
        [SerializeField] private Collider2D[] representedWater;
        [SerializeField] private AudioClip water;
        [SerializeField] private float audibleDistance=6;
        private AudioSource source;
        private float proximity;
        public float DistanceToWater {get;private set;}
        private void Awake(){source=GetComponent<AudioSource>();source.playOnAwake=false;source.loop=true;source.spatialBlend=0;source.clip=water;source.volume=0;}
        private void Update()
        {
            if(leo==null||water==null)return;Vector2 p=leo.position,nearest=p;float distance=float.MaxValue;
            foreach(var area in representedWater){if(area==null||!area.enabled)continue;var q=area.ClosestPoint(p);float d=Vector2.Distance(p,q);if(d<distance){distance=d;nearest=q;}}
            DistanceToWater=distance;float target=distance<audibleDistance?Mathf.Pow(1-distance/audibleDistance,2):0;
            if(!GameplayPause.IsPaused)proximity=Mathf.MoveTowards(proximity,target,Time.unscaledDeltaTime*.85f);source.volume=MenuPreferences.EffectsVolume*.38f*proximity;source.panStereo=Mathf.Clamp((nearest.x-p.x)/audibleDistance,-1,1)*.4f;
            if(GameplayPause.IsPaused)return;
            if(proximity>.001f&&!source.isPlaying)source.Play();else if(proximity<=.001f&&source.isPlaying)source.Stop();
        }
        private void OnDisable(){source?.Stop();}
    }
}

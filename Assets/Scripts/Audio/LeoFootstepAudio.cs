using UnityEngine;

namespace FrecuenciaCero
{
    // Listens to existing footstep noise; never emits noise or changes movement/AI.
    [DefaultExecutionOrder(30),DisallowMultipleComponent,RequireComponent(typeof(AudioSource),typeof(LeoMovement))]
    public sealed class LeoFootstepAudio : MonoBehaviour
    {
        [SerializeField] private GameSoundPalette sounds;
        [SerializeField] private FootstepSurfaceMap surfaceMap;
        [SerializeField,Range(0,1)] private float gain=.72f;
        private AudioSource source;
        private LeoMovement movement;
        private Rigidbody2D body;
        private Collider2D feet;
        private CinematicWalk walker;
        private Vector2 previous;
        private float activeGain,nextCinematic;
        private readonly int[] last={-1,-1,-1,-1};
        private System.Random random;
        public int PlayedCount {get;private set;}
        public FootstepSurface LastSurface {get;private set;}
        public AudioClip LastClip {get;private set;}
        private void Awake()
        {
            source=GetComponent<AudioSource>();movement=GetComponent<LeoMovement>();body=GetComponent<Rigidbody2D>();feet=GetComponent<Collider2D>();walker=GetComponent<CinematicWalk>();
            source.playOnAwake=false;source.loop=false;source.spatialBlend=0;previous=body.position;random=new System.Random(GetEntityId().GetHashCode());
        }
        private void OnEnable(){NoiseEvents.Emitted+=OnNoise;}
        private void OnDisable(){NoiseEvents.Emitted-=OnNoise;source?.Stop();}
        private void OnNoise(NoiseEvent noise)
        {
            if((noise.Kind!=NoiseKind.Walk&&noise.Kind!=NoiseKind.Run)||!movement.enabled||movement.IsBlocked||Vector2.Distance(noise.Position,feet.bounds.center)>.3f)return;
            Play(noise.Kind==NoiseKind.Run);
        }
        private void Play(bool running)
        {
            if(sounds==null||surfaceMap==null)return;LastSurface=surfaceMap.At(feet.bounds.center);var clips=sounds.Steps(LastSurface);
            if(clips==null||clips.Length==0)return;
            int bank=(int)LastSurface,index=random.Next(clips.Length);if(clips.Length>1&&index==last[bank])index=(index+1+random.Next(clips.Length-1))%clips.Length;last[bank]=index;
            if(clips[index]==null)return;source.Stop();source.clip=LastClip=clips[index];source.pitch=(float)(.985+random.NextDouble()*.03)*(running?1.025f:1);
            activeGain=gain*(running?1.06f:1);source.volume=activeGain*MenuPreferences.EffectsVolume;source.Play();PlayedCount++;
        }
        private void FixedUpdate()
        {
            float moved=Vector2.Distance(body.position,previous);previous=body.position;
            // CinematicWalk moves the body with LeoMovement disabled; sound remains separate from AI noise.
            if(!movement.enabled&&walker!=null&&walker.IsWalking&&moved>.001f&&moved<.5f&&Time.time>=nextCinematic){Play(false);nextCinematic=Time.time+.48f;}
        }
        private void Update()
        {
            bool walking=(movement.enabled&&!movement.IsBlocked||walker!=null&&walker.IsWalking)&&body.linearVelocity.sqrMagnitude>.04f;
            if(!GameplayPause.IsPaused&&!walking)activeGain=Mathf.MoveTowards(activeGain,0,Time.unscaledDeltaTime*22);
            source.volume=activeGain*MenuPreferences.EffectsVolume;if(activeGain<=0&&source.isPlaying)source.Stop();
        }
    }
}

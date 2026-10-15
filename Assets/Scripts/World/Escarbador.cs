using System.Collections.Generic;
using UnityEngine;

namespace FrecuenciaCero
{
    public enum EscarbadorState { Hidden, Alert, Investigate, Chase, Search, Withdraw }
    [RequireComponent(typeof(Rigidbody2D),typeof(Collider2D),typeof(SpriteRenderer))]
    public sealed class Escarbador : MonoBehaviour
    {
        [SerializeField] private Vector2 hideout = new Vector2(3,-3);
        [SerializeField] private Rect navigationBounds = new Rect(-16,-16,32,32);
        [SerializeField, Range(0,1)] private float sensitivity = .18f;
        [SerializeField] private float investigateSpeed = 2.1f, chaseSpeed = 3.4f, withdrawalSpeed = 1.6f;
        [SerializeField] private float alertSeconds = .7f, searchSeconds = 5f, continuousNoiseWindow = 1.1f;
        [SerializeField] private float footstepStoppingDistance = 1.05f, searchRadius = 1.25f;
        [SerializeField] private int contactDamage = 1;
        [SerializeField] private Sprite[] crawlFrames;
        private Rigidbody2D body; private Collider2D solid; private SpriteRenderer artwork;
        private List<Vector2> path = new List<Vector2>();
        private int waypoint, continuousSteps;
        private float stateSince, lastStep = -100, nextPath, nextSearch, animation;
        private float appearance;
        private float searchAngle;
        private Color originalColor;
        private bool activated, frozen;
        private Vector2 destination;
        public EscarbadorState State { get; private set; } = EscarbadorState.Hidden;
        public Vector2 LastHeard { get; private set; }
        public NoiseKind LastKind { get; private set; }
        public bool Frozen { get => frozen; set { frozen = value; if(value && body != null)body.linearVelocity=Vector2.zero; } }
        private void Awake(){body=GetComponent<Rigidbody2D>();solid=GetComponent<Collider2D>();artwork=GetComponent<SpriteRenderer>();originalColor=artwork.color;}
        private void Start()
        {
            activated=RetenMission.Session.EncounterCompleted;
            artwork.enabled=false;solid.enabled=false;
            if(activated && !RetenMission.Session.EscapedEscarbador) RetenMission.Session.CompleteEvasion();
        }
        private void OnEnable()=>NoiseEvents.Emitted+=Hear;
        private void OnDisable(){NoiseEvents.Emitted-=Hear;if(body!=null)body.linearVelocity=Vector2.zero;}
        public void Hear(NoiseEvent noise)
        {
            if(GameplayPause.IsPaused||!activated||frozen||noise.Intensity<sensitivity||noise.Kind==NoiseKind.Soil||
                Vector2.Distance(transform.position,noise.Position)>noise.Radius)return;
            LastHeard=noise.Position;LastKind=noise.Kind;
            bool step=noise.Kind==NoiseKind.Walk||noise.Kind==NoiseKind.Run;
            continuousSteps=step?(Time.time-lastStep<continuousNoiseWindow?continuousSteps+1:1):0;
            if(step)lastStep=Time.time;
            destination=LastHeard;nextPath=0;
            SetState(State==EscarbadorState.Hidden?EscarbadorState.Alert:
                continuousSteps>=2?EscarbadorState.Chase:EscarbadorState.Investigate);
        }
        public void RevealListening(Vector2 sound)
        {
            activated=true;artwork.enabled=true;LastHeard=destination=sound;LastKind=NoiseKind.Walk;
            appearance=0;
            artwork.flipX=sound.x<transform.position.x;nextPath=0;SetState(EscarbadorState.Alert);
        }
        public void ResetAtHideout()
        {
            if(body==null)return;
            body.position=hideout;transform.position=hideout;body.linearVelocity=Vector2.zero;
            frozen=false;activated=RetenMission.Session.EncounterCompleted;
            path.Clear();SetState(EscarbadorState.Hidden);
            if(activated)RetenMission.Session.CompleteEvasion();
        }
        private void SetState(EscarbadorState state)
        {
            State=state;stateSince=Time.time;
            artwork.enabled=state!=EscarbadorState.Hidden;solid.enabled=state!=EscarbadorState.Hidden;
            if(state==EscarbadorState.Search)
            {
                nextSearch=nextPath=0;
                Vector2 offset=body.position-LastHeard;searchAngle=Mathf.Atan2(offset.y,offset.x);
            }
        }
        private void FixedUpdate()
        {
            if(!activated||frozen){body.linearVelocity=Vector2.zero;return;}
            if(State==EscarbadorState.Hidden){body.linearVelocity=Vector2.zero;return;}
            if(State==EscarbadorState.Alert)
            {
                body.linearVelocity=Vector2.zero;
                if(Time.time-stateSince>=alertSeconds)SetState(EscarbadorState.Investigate);
                return;
            }
            if(State==EscarbadorState.Chase&&Time.time-lastStep>continuousNoiseWindow)SetState(EscarbadorState.Investigate);
            if(State==EscarbadorState.Search)
            {
                if(Time.time-stateSince>searchSeconds){destination=hideout;nextPath=0;SetState(EscarbadorState.Withdraw);}
                else if(Time.time>nextSearch)
                {
                    searchAngle+=.65f;
                    destination=LastHeard+new Vector2(Mathf.Cos(searchAngle),Mathf.Sin(searchAngle))*searchRadius;
                    nextSearch=Time.time+1.1f;nextPath=0;
                }
            }
            // Footsteps are an approximate auditory location. Search around them once quiet;
            // continuous movement can still be chased into physical contact. Bottles use their exact impact.
            float stoppingDistance=State==EscarbadorState.Investigate && (LastKind==NoiseKind.Walk||LastKind==NoiseKind.Run)
                ? footstepStoppingDistance : .26f;
            if(Vector2.Distance(body.position,destination)<stoppingDistance)
            {
                body.linearVelocity=Vector2.zero;
                if(State==EscarbadorState.Withdraw){SetState(EscarbadorState.Hidden);RetenMission.Session.CompleteEvasion();}
                else if(State==EscarbadorState.Investigate||State==EscarbadorState.Chase)SetState(EscarbadorState.Search);
                return;
            }
            if(Time.time>=nextPath)
            {path=ForestNavigation.Find(body.position,destination,navigationBounds,solid);waypoint=0;nextPath=Time.time+1.1f;}
            while(waypoint<path.Count&&Vector2.Distance(body.position,path[waypoint])<.09f)waypoint++;
            if(waypoint>=path.Count)
            {
                body.linearVelocity=Vector2.zero;
                if(Time.time-stateSince>searchSeconds && State!=EscarbadorState.Withdraw)SetState(EscarbadorState.Search);
                return;
            }
            Vector2 direction=(path[waypoint]-body.position).normalized;
            body.linearVelocity=direction*(State==EscarbadorState.Chase?chaseSpeed:State==EscarbadorState.Withdraw?withdrawalSpeed:investigateSpeed);
            if(Mathf.Abs(direction.x)>.05f)artwork.flipX=direction.x<0;
        }
        private void LateUpdate()
        {
            if(artwork.enabled)
            {
                appearance=Mathf.MoveTowards(appearance,1,Time.deltaTime);
                artwork.color=new Color(originalColor.r,originalColor.g,originalColor.b,originalColor.a*appearance);
            }
            else appearance=0;
            artwork.sortingOrder=Mathf.RoundToInt(-transform.position.y*32);
            if(crawlFrames!=null&&crawlFrames.Length>0)
            {animation+=Time.deltaTime*(body.linearVelocity.sqrMagnitude>.05f?7:2);artwork.sprite=crawlFrames[(int)animation%crawlFrames.Length];}
        }
        private void OnCollisionStay2D(Collision2D collision)
        {
            if(!frozen&&activated&&State!=EscarbadorState.Hidden)
                collision.gameObject.GetComponent<SurvivalPlayer>()?.TakeDamage(contactDamage);
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace FrecuenciaCero
{
    public sealed class RootsEncounter : MonoBehaviour
    {
        [SerializeField] private SurvivalPlayer leo;
        [SerializeField] private Escarbador creature;
        [SerializeField] private Transform movingSoil;
        [SerializeField] private Vector2 encounterPoint = new Vector2(-2,-1);
        [SerializeField] private float triggerRadius = 1.6f;
        [SerializeField] private Vector2 safeExit = new Vector2(11,-10);
        [SerializeField] private float exitRadius = 1.3f;
        [SerializeField] private UnityEvent levelTwoReady = new UnityEvent();
        [SerializeField] private ChapterOneEnding chapterEnding;
        private bool playing, ending;
        private string title, text;
        private int page;
        private float panelOpacity;
        private string shownTitle, shownText;
        private bool shownTutorial;
        private AudioSource soilAudio;
        private AudioClip rustle;
        private SmoothCameraFollow encounterFollow;
        private bool encounterFollowWasEnabled;
        private Vector3 soilRestScale;
        public bool IsPlaying => playing;
        public bool IsEnding => ending;
        public int TutorialPage => page;
        private void Start()
        {
            if(RetenMission.Session.EncounterStarted&&!RetenMission.Session.EncounterCompleted)RetenMission.Session.FinishEncounter();
            soilAudio=gameObject.AddComponent<AudioSource>();soilAudio.playOnAwake=false;
            rustle=AudioClip.Create("Tierra y hojas — síntesis local",22050,1,44100,false);
            var samples=new float[22050];var random=new System.Random(17);float low=0;
            for(int i=0;i<samples.Length;i++){low=low*.75f+((float)random.NextDouble()*2-1)*.25f;samples[i]=low*Mathf.Sin(Mathf.PI*i/samples.Length)*.3f;}
            rustle.SetData(samples,0);
            if(RetenMission.Session.LevelOneCompleted||RetenMission.Session.Chapters.ClosingStarted)
            {ending=true;chapterEnding.Begin();}
        }
        private void Update()
        {
            if(GameplayPause.IsPaused)return;
            panelOpacity=Mathf.MoveTowards(panelOpacity,string.IsNullOrEmpty(title)?0:1,Time.unscaledDeltaTime/.18f);
            if(!string.IsNullOrEmpty(title)){shownTitle=title;shownText=text;shownTutorial=page>0;}
            if(leo.Dead||RetenTravel.IsLoading)return;
            var session=RetenMission.Session;
            if(!playing&&!ending&&!session.EncounterStarted && leo.CanAct && !leo.Aiming &&
                Vector2.Distance(leo.transform.position,encounterPoint)<triggerRadius)
            {session.BeginEncounter();StartCoroutine(FirstEncounter());}
            if(!playing&&!ending&&session.EncounterCompleted&&session.EscapedEscarbador&&creature.State==EscarbadorState.Hidden&&leo.CanAct&&!leo.Aiming&&
                Vector2.Distance(leo.transform.position,safeExit)<exitRadius)
            {ending=true;chapterEnding.Begin();}
        }
        public void PlaySoilAt(Vector2 point)=>Rustle(point);
        public void ChapterCardReady()=>levelTwoReady.Invoke();
        private void Rustle(Vector2 point)
        {
            soilAudio.volume=MenuPreferences.EffectsVolume*.55f;soilAudio.PlayOneShot(rustle);
            NoiseEvents.Emit(new NoiseEvent(point,.7f,8,NoiseKind.Soil));
        }
        private IEnumerator FirstEncounter()
        {
            playing=true;leo.Cinematic=true;leo.GetComponent<LeoMovement>().IsBlocked=true;
            leo.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;creature.Frozen=true;
            Camera camera=Camera.main;var follow=camera.GetComponent<SmoothCameraFollow>();bool oldFollow=follow.enabled;follow.enabled=false;
            encounterFollow=follow;encounterFollowWasEnabled=oldFollow;
            Vector3 origin=camera.transform.position;
            Vector2 heard=NoiseEvents.LastFootstep.Radius>0?NoiseEvents.LastFootstep.Position:(Vector2)leo.transform.position+Vector2.up;
            // The interrupted steps stay behind Leo; the enemy never reads his transform for detection.
            if(Vector2.Distance(heard,leo.transform.position)<1.2f)heard+=Vector2.up*2f;
            Rustle(movingSoil.position);
            Vector3 scale=movingSoil.localScale;
            soilRestScale=scale;
            for(float t=0;t<1;t+=Time.deltaTime){movingSoil.localScale=scale*(1+Mathf.Sin(t*19)*.035f);yield return null;}
            movingSoil.localScale=scale;
            yield return Pan(camera,new Vector3(creature.transform.position.x,creature.transform.position.y,-10),1.2f);
            creature.RevealListening(heard);
            yield return new WaitForSeconds(1.8f);
            title="LEO";text="¿Qué... fue eso?";yield return new WaitForSeconds(2.2f);title=text=null;
            yield return Pan(camera,new Vector3(leo.transform.position.x,leo.transform.position.y,-10),1.2f);
            follow.enabled=oldFollow;
            RetenMission.Session.UnlockNoiseHud();
            page=1;title="NO HAGAS RUIDO";text="El Escarbador detecta los sonidos de tus movimientos. Suelta WASD para permanecer quieto. Caminar genera ruido y correr genera todavía más.";
            yield return ContinuePage();
            page=2;title="ESPERA A QUE SE ALEJE";text="Cuando deje de buscarte, continúa con cuidado.";
            yield return ContinuePage();
            page=0;title=text=null;RetenMission.Session.FinishEncounter();creature.Frozen=false;
            leo.Cinematic=false;leo.GetComponent<LeoMovement>().IsBlocked=false;playing=false;encounterFollow=null;
        }
        private IEnumerator ContinuePage()
        {
            // Require release and a new press, never consume the same E used to enter a scene.
            yield return new WaitForSeconds(.35f);
            while(Keyboard.current!=null&&(Keyboard.current.eKey.isPressed||Keyboard.current.enterKey.isPressed))yield return null;
            while(Keyboard.current==null||!(Keyboard.current.eKey.wasPressedThisFrame||Keyboard.current.enterKey.wasPressedThisFrame))yield return null;
            yield return null;
        }
        private static IEnumerator Pan(Camera camera,Vector3 target,float seconds)
        {
            Vector3 origin=camera.transform.position;
            for(float t=0;t<seconds;t+=Time.deltaTime){camera.transform.position=Vector3.Lerp(origin,target,Mathf.SmoothStep(0,1,t/seconds));yield return null;}
            camera.transform.position=target;
        }
        private void OnGUI()
        {
            if(GameplayPause.IsPaused)return;
            var matrix=GUI.matrix;var color=GUI.color;int oldDepth=GUI.depth;float scale=NarrativePanels.Scale;
            GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);GUI.depth=-400;float width=Screen.width/scale,height=Screen.height/scale;
            if(panelOpacity>0 && !ending)
                NarrativePanels.Message(width,height,shownTitle,shownText,shownText,shownTutorial?"E / ENTER — continuar":null,shownTutorial,panelOpacity);
            GUI.matrix=matrix;GUI.color=color;GUI.depth=oldDepth;
        }
        private void OnDisable()
        {
            if(!playing)return;
            StopAllCoroutines();
            if(encounterFollow!=null)encounterFollow.enabled=encounterFollowWasEnabled;
            encounterFollow=null;
            if(movingSoil!=null)movingSoil.localScale=soilRestScale;
            if(leo!=null)
            {
                leo.Cinematic=false;
                leo.GetComponent<LeoMovement>().IsBlocked=false;
                leo.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;
            }
            // Match the existing scene-reload recovery: an interrupted encounter resumes
            // from its completed checkpoint rather than keeping a stopped coroutine alive.
            var session=RetenMission.Session;
            if(session.EncounterStarted&&!session.EncounterCompleted)session.FinishEncounter();
            if(creature!=null)creature.ResetAtHideout();
            playing=false;page=0;title=text=shownTitle=shownText=null;panelOpacity=0;
        }
        private void OnDestroy(){if(rustle!=null)Destroy(rustle);}
    }
}

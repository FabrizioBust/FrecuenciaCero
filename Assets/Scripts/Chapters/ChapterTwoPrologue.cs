using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FrecuenciaCero
{
    [DefaultExecutionOrder(350)]
    public sealed class ChapterTwoPrologue : MonoBehaviour
    {
        [SerializeField] private SurvivalPlayer leo;
        [SerializeField] private NarrativeDirector director;
        [SerializeField] private ChapterAudio audioCues;
        [SerializeField] private DialogueSequence[] radioLines;
        [SerializeField] private Vector2 radioPoint = new Vector2(-8,-6);
        [SerializeField] private Vector2 cabinPoint = new Vector2(7,8.5f);
        [SerializeField] private Light2D windowLight;
        [SerializeField] private SpriteRenderer windowGlow;
        private bool busy, previousMovement;
        private System.IDisposable hiddenHud;
        public bool IsBusy => busy;
        public bool WindowIsFading { get; private set; }
        private void Awake()
        {
            if(!RetenMission.Session.Chapters.ArrivalFinished)Lock();
        }
        private IEnumerator Start()
        {
            RetenMission.Session.StartChapterTwo();
            if(RetenMission.Session.Chapters.WindowExtinguished){windowLight.intensity=0;windowGlow.color=Color.clear;}
            if(RetenMission.Session.Chapters.ArrivalFinished)yield break;
            while(RetenTravel.IsLoading)yield return null;yield return null;
            GeneratorGateFade localFade=null;
            if(FindAnyObjectByType<ChapterTransitionOverlay>()==null){localFade=GeneratorGateFade.Create();StartCoroutine(localFade.Blend(1,0,1));}
            yield return leo.GetComponent<CinematicWalk>().Walk(Vector2.up,2.6f,.7f);
            RetenMission.Session.Chapters.FinishArrival();Unlock();
            if(localFade!=null)Destroy(localFade.gameObject);
        }
        private void Lock()
        {
            busy=true;previousMovement=leo.GetComponent<LeoMovement>().enabled;
            leo.Cinematic=true;leo.GetComponent<LeoMovement>().enabled=false;leo.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;
            hiddenHud=HudVisibility.HideForCinematic();
        }
        private void Unlock()
        {
            leo.GetComponent<CinematicWalk>().Stop();leo.Cinematic=false;leo.GetComponent<LeoMovement>().enabled=previousMovement;
            leo.GetComponent<LeoMovement>().IsBlocked=false;hiddenHud?.Dispose();hiddenHud=null;busy=false;
        }
        private void Update()
        {
            if(busy||RetenTravel.IsLoading||!leo.CanAct||leo.Aiming)return;
            var progress=RetenMission.Session.Chapters;
            if(!progress.RadioCompleted&&(Vector2.Distance(leo.transform.position,radioPoint)<2.0f||Vector2.Distance(leo.transform.position,cabinPoint)<5.5f))
                StartCoroutine(RadioConversation());
            else if(progress.RadioCompleted&&!progress.WindowExtinguished&&Vector2.Distance(leo.transform.position,cabinPoint)<4.5f)
                StartCoroutine(ExtinguishWindow());
        }
        private IEnumerator RadioConversation()
        {
            Lock();
            for(int i=0;i<radioLines.Length;i++)
            {
                if(i==1){audioCues.PlayCue("radio_static");yield return new WaitForSeconds(1.1f);}
                if(i==2){yield return new WaitForSeconds(1.2f);audioCues.PlayCue("radio_warning");}
                bool closed=false;director.OpenDialogue(radioLines[i],()=>closed=true);
                while(!closed)yield return null;
                yield return new WaitForSeconds(.25f);
            }
            audioCues.PlayCue("radio_cut");RetenMission.Session.Chapters.FinishRadio();Unlock();
        }
        private IEnumerator ExtinguishWindow()
        {
            WindowIsFading=true;RetenMission.Session.DiscoverCabin();
            float intensity=windowLight.intensity;Color tint=windowGlow.color;
            for(float t=0;t<.65f;t+=Time.deltaTime)
            {
                float amount=1-Mathf.SmoothStep(0,1,t/.65f);windowLight.intensity=intensity*amount;
                windowGlow.color=new Color(tint.r,tint.g,tint.b,tint.a*amount);yield return null;
            }
            windowLight.intensity=0;windowGlow.color=Color.clear;WindowIsFading=false;
        }
        private void OnDisable()
        {
            if(busy&&leo!=null)Unlock();else{hiddenHud?.Dispose();hiddenHud=null;}
        }
    }
}

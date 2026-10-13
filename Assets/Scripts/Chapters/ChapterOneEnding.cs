using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FrecuenciaCero
{
    public sealed class ChapterOneEnding : MonoBehaviour
    {
        public const string NextScene = "Assets/Scenes/Nivel02/N2_SenderoCabana.unity";
        [SerializeField] private SurvivalPlayer leo;
        [SerializeField] private Escarbador creature;
        [SerializeField] private RootsEncounter roots;
        [SerializeField] private ChapterAudio audioCues;
        [SerializeField] private Transform subtleBranches;
        [SerializeField] private Canvas view;
        [SerializeField] private CanvasGroup blackout, closingCard;
        [SerializeField] private Button continueButton;
        private bool begun, transitioning;
        private float startedAt, captionOpacity;
        private bool whisperVisible;
        private SmoothCameraFollow follow;
        private bool previousFollow, previousMovement;
        private System.IDisposable hiddenHud;
        public bool IsPlaying => begun && !CardReady;
        public bool CardReady { get; private set; }
        public bool IsTransitioning => transitioning;
        public float Elapsed => begun ? Time.time - startedAt : 0;
        public Button ContinueButton => continueButton;
        public string Phase { get; private set; }
        private void Awake()
        {
            continueButton.onClick.AddListener(Continue);
            continueButton.interactable = false;
            blackout.alpha = closingCard.alpha = 0;
            view.gameObject.SetActive(false);
        }
        public void Begin()
        {
            if (begun || leo.Dead || RetenTravel.IsLoading) return;
            begun = true; startedAt = Time.time;
            previousMovement = leo.GetComponent<LeoMovement>().enabled;
            leo.Cinematic = true; leo.Aiming = false; leo.GetComponent<LeoMovement>().enabled = false;
            leo.GetComponent<Rigidbody2D>().linearVelocity = Vector2.zero; creature.Frozen = true;
            hiddenHud = HudVisibility.HideForCinematic();
            view.gameObject.SetActive(true);
            follow = Camera.main.GetComponent<SmoothCameraFollow>(); previousFollow = follow.enabled; follow.enabled = false;
            bool fresh = RetenMission.Session.Chapters.BeginClosing();
            StartCoroutine(fresh && !RetenMission.Session.LevelOneCompleted ? Cinematic() : ResumeCard());
        }
        private IEnumerator Cinematic()
        {
            Phase = "walk";
            var walker = leo.GetComponent<CinematicWalk>();
            StartCoroutine(walker.Walk(new Vector2(1,-.55f), 3f, .7f));
            Vector3 origin = Camera.main.transform.position;
            Vector3 restingCamera = origin + new Vector3(.55f,-.2f,0);
            for (float t = 0; t < 3; t += Time.deltaTime)
            { Camera.main.transform.position = Vector3.Lerp(origin, restingCamera, Mathf.SmoothStep(0,1,Mathf.Min(1,t/1.4f))); yield return null; }
            walker.Stop(); Phase = "soil";
            roots.PlaySoilAt((Vector2)leo.transform.position + new Vector2(-2,1.3f));
            leo.GetComponent<LeoSpriteAnimator>().Face(new Vector2(-1,.6f));
            yield return new WaitForSeconds(1.4f);
            Phase = "branches";
            Vector3 branchOrigin = subtleBranches.localPosition;
            for (float t = 0; t < 2.3f; t += Time.deltaTime)
            { subtleBranches.localPosition = branchOrigin + Vector3.right * (Mathf.Sin(t/2.3f*Mathf.PI)*.12f); yield return null; }
            subtleBranches.localPosition = branchOrigin;
            yield return new WaitForSeconds(.8f);
            Phase = "radio"; audioCues.PlayCue("radio_static"); yield return new WaitForSeconds(1.2f);
            Phase = "whisper"; audioCues.PlayCue("whisper_leo"); whisperVisible = true;
            yield return new WaitForSeconds(2.4f); whisperVisible = false;
            Phase = "silence"; yield return new WaitForSeconds(2.7f);
            Phase = "fade"; yield return Blend(blackout, 1, 3.6f);
            RetenMission.Session.CompleteLevelOne(); roots.ChapterCardReady();
            yield return ShowCard();
        }
        private IEnumerator ResumeCard()
        {
            // Interrupted closing is recovered once, keeping inventory and avoiding another 18-second sequence.
            RetenMission.Session.CompleteLevelOne(); blackout.alpha = 1;
            yield return ShowCard();
        }
        private IEnumerator ShowCard()
        {
            Phase = "card";
            yield return Blend(closingCard, 1, .8f);
            CardReady = RetenMission.Session.LevelOneCompleted;
            continueButton.interactable = CardReady;
            if (CardReady && EventSystem.current != null) continueButton.Select();
        }
        private void Update() => captionOpacity = Mathf.MoveTowards(captionOpacity, whisperVisible ? 1 : 0, Time.unscaledDeltaTime/.18f);
        private void OnGUI()
        {
            if (captionOpacity <= 0) return;
            var matrix=GUI.matrix;var color=GUI.color;int depth=GUI.depth;
            float scale=NarrativePanels.Scale;GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);GUI.depth=-400;
            NarrativePanels.Message(Screen.width/scale,Screen.height/scale,"RADIO","«Leo...»","«Leo...»",null,false,captionOpacity);
            GUI.matrix=matrix;GUI.color=color;GUI.depth=depth;
        }
        private static IEnumerator Blend(CanvasGroup group, float target, float duration)
        {
            float origin = group.alpha;
            for(float t=0;t<duration;t+=Time.unscaledDeltaTime){group.alpha=Mathf.Lerp(origin,target,Mathf.SmoothStep(0,1,t/duration));yield return null;}
            group.alpha=target;
        }
        public void Continue()
        {
            if (!CardReady || transitioning || RetenTravel.IsLoading) return;
            transitioning=true;continueButton.interactable=false;StartCoroutine(ContinueRoutine());
        }
        private IEnumerator ContinueRoutine()
        {
            var overlay=ChapterTransitionOverlay.Create("CAPÍTULO II","LA CABAÑA DEL GUARDABOSQUES");
            yield return overlay.Cover();yield return overlay.ShowTitle();
            if(RetenTravel.Begin(NextScene,"capitulo2_inicio",leo.GetComponentInChildren<FlashlightBattery>(),leo.GetComponentInChildren<FlashlightController>()))
            {overlay.StartCoroutine(RevealAfterArrival(overlay));yield break;}
            yield return overlay.Reveal();transitioning=false;continueButton.interactable=true;
        }
        private static IEnumerator RevealAfterArrival(ChapterTransitionOverlay overlay)
        {
            while(RetenTravel.IsLoading)yield return null;yield return null;yield return overlay.Reveal();
        }
        private void OnDisable()
        {
            hiddenHud?.Dispose();hiddenHud=null;
            if(begun&&leo!=null){leo.GetComponent<CinematicWalk>()?.Stop();leo.Cinematic=false;leo.GetComponent<LeoMovement>().enabled=previousMovement;}
            if(follow!=null)follow.enabled=previousFollow;
        }
    }
}

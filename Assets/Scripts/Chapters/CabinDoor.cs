using System.Collections;
using UnityEngine;

namespace FrecuenciaCero
{
    public sealed class CabinDoor : WorldInteraction
    {
        public const string InteriorScene="Assets/Scenes/Nivel02/N2_CabanaGuardabosques.unity";
        [SerializeField] private SurvivalPlayer leo;
        [SerializeField] private ChapterTwoPrologue story;
        [SerializeField] private ChapterAudio audioCues;
        [SerializeField] private DialogueSequence conversation;
        [SerializeField] private Transform doorLeaf;
        private bool busy, previousMovement;
        private System.IDisposable hiddenHud;
        public bool IsOpening { get; private set; }
        public float OpenAmount { get; private set; }
        public override string Prompt=>RetenMission.Session.Chapters.DoorConversationCompleted?"[E] Entrar en la cabaña":"[E] Llamar a la puerta";
        private void Start(){if(RetenMission.Session.Chapters.DoorConversationCompleted)SetOpen(1);}
        public override bool IsAvailable(InteractionProbe player)=>!busy&&!story.IsBusy&&!story.WindowIsFading&&RetenMission.Session.Chapters.WindowExtinguished&&
            !RetenTravel.IsLoading&&player.CanReach(InteractionPoint,1.35f,Vector2.down,1.2f);
        public override void Interact(NarrativeDirector director)
        {
            if(busy||director==null||director.IsOpen||leo.Dead||!leo.CanAct||leo.Aiming)return;
            if(!RetenMission.Session.Chapters.WindowExtinguished||story.IsBusy||story.WindowIsFading)return;
            busy=true;previousMovement=leo.GetComponent<LeoMovement>().enabled;
            leo.Cinematic=true;leo.GetComponent<LeoMovement>().enabled=false;leo.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;
            hiddenHud=HudVisibility.HideForCinematic();StartCoroutine(Enter(director));
        }
        private void SetOpen(float amount){OpenAmount=amount;doorLeaf.localScale=new Vector3(Mathf.Lerp(1,.12f,amount),1,1);}
        private IEnumerator Enter(NarrativeDirector director)
        {
            if(!RetenMission.Session.Chapters.DoorConversationCompleted)
            {
                audioCues.PlayCue("knock");yield return new WaitForSeconds(2.1f);
                bool closed=false;director.OpenDialogue(conversation,()=>closed=true);while(!closed)yield return null;
                RetenMission.Session.Chapters.FinishDoorConversation();yield return new WaitForSeconds(.7f);
                IsOpening=true;
                for(float t=0;t<1.5f;t+=Time.deltaTime){SetOpen(Mathf.SmoothStep(0,1,t/1.5f));yield return null;}
                SetOpen(1);IsOpening=false;yield return new WaitForSeconds(.4f);
            }
            var fade=GeneratorGateFade.Create();yield return fade.Blend(0,1,.9f);
            if(RetenTravel.Begin(InteriorScene,"cabana_umbral",leo.GetComponentInChildren<FlashlightBattery>(),leo.GetComponentInChildren<FlashlightController>()))
            {fade.FinishAfterTravel();yield break;}
            yield return fade.Blend(1,0,.5f);Destroy(fade.gameObject);Release();
        }
        private void Release()
        {
            hiddenHud?.Dispose();hiddenHud=null;
            if(leo!=null){leo.Cinematic=false;leo.GetComponent<LeoMovement>().enabled=previousMovement;leo.GetComponent<LeoMovement>().IsBlocked=false;}
            busy=false;
        }
        private void OnDisable(){if(busy)Release();}
    }
}

using System.Collections;
using UnityEngine;

namespace FrecuenciaCero
{
    [DefaultExecutionOrder(350)]
    public sealed class CabinVestibule : MonoBehaviour
    {
        [SerializeField] private SurvivalPlayer leo;
        private System.IDisposable hiddenHud;
        private bool locked, previousMovement;
        private void Awake()
        {
            if(RetenMission.Session.Chapters.CabinEntered)return;
            locked=true;previousMovement=leo.GetComponent<LeoMovement>().enabled;
            leo.Cinematic=true;leo.GetComponent<LeoMovement>().enabled=false;hiddenHud=HudVisibility.HideForCinematic();
        }
        private IEnumerator Start()
        {
            if(!locked)yield break;
            while(RetenTravel.IsLoading)yield return null;yield return null;
            yield return leo.GetComponent<CinematicWalk>().Walk(Vector2.up,1.2f,.6f);
            RetenMission.Session.EnterCabin();Release();
        }
        private void Release()
        {
            hiddenHud?.Dispose();hiddenHud=null;locked=false;
            if(leo!=null){leo.Cinematic=false;leo.GetComponent<LeoMovement>().enabled=previousMovement;leo.GetComponent<LeoMovement>().IsBlocked=false;}
        }
        private void OnDisable(){if(locked)Release();}
    }
}

using UnityEngine;

namespace FrecuenciaCero
{
    [DefaultExecutionOrder(10),DisallowMultipleComponent]
    public sealed class IntroVehicleSoundMarker : MonoBehaviour
    {
        private IntroHistoriaController intro;
        private void Awake()=>intro=GetComponent<IntroHistoriaController>();
        private void Update(){if(intro.Elapsed>=36&&intro.engineStop!=null)VehicleSoundContinuity.MarkStopped();}
    }
}

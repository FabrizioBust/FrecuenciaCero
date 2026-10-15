using System.Collections;
using UnityEngine;

namespace FrecuenciaCero
{
    // Session reference prevents replay on room/scene returns, without touching mission mechanics.
    public static class VehicleSoundContinuity
    {
        private static MissionProgress stopped,arrived;
        public static bool EngineStopped=>ReferenceEquals(stopped,RetenMission.Session);
        public static void MarkStopped()=>stopped=RetenMission.Session;
        public static bool TryArrival(){if(ReferenceEquals(arrived,RetenMission.Session))return false;arrived=RetenMission.Session;return true;}
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset(){stopped=arrived=null;}
    }
    [DisallowMultipleComponent,RequireComponent(typeof(AudioSource))]
    public sealed class VehicleArrivalSound : MonoBehaviour
    {
        [SerializeField] private GameSoundPalette sounds;
        [SerializeField] private Transform leo,truck;
        private AudioSource source;
        public int PlayedCues {get;private set;}
        private void Awake(){source=GetComponent<AudioSource>();source.playOnAwake=false;source.loop=false;source.spatialBlend=0;}
        private IEnumerator Start()
        {
            if(sounds==null||RetenMission.Session.TreeFallStarted||RetenMission.Session.TreeFallen||!VehicleSoundContinuity.TryArrival())yield break;
            // Normal intro already plays shutdown at its actual stopping event. Skips/direct editor entry finish it here.
            if(!VehicleSoundContinuity.EngineStopped&&sounds.motorStopping!=null){Play(sounds.motorStopping);VehicleSoundContinuity.MarkStopped();yield return new WaitForSeconds(sounds.motorStopping.length+.15f);}
            else yield return new WaitForSeconds(.15f);
            if(sounds.truckDoor!=null)Play(sounds.truckDoor);
        }
        private void Play(AudioClip clip){source.clip=clip;source.Play();PlayedCues++;}
        private void Update(){float distance=leo!=null&&truck!=null?Vector2.Distance(leo.position,truck.position):0;source.volume=MenuPreferences.EffectsVolume*.8f*Mathf.Clamp01(1-distance/9);}
        private void OnDisable(){source?.Stop();}
    }
}

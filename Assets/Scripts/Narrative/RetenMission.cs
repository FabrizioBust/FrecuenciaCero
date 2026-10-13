using UnityEngine;

namespace FrecuenciaCero
{
    // No persistent GameObjects. Scene changes retain flags; starting Play resets them.
    public static class RetenMission
    {
        public static MissionProgress Session { get; private set; } = new MissionProgress();
        public static float GeneratorStartedAt { get; private set; } = -1f;
        public static float FuseNoticeUntil { get; private set; }
        public static void RepairGenerator()
        {
            if (Session.RepairGenerator()) GeneratorStartedAt = Time.time;
        }
        public static void CollectFuse()
        {
            if (Session.CollectFuse()) FuseNoticeUntil = Time.unscaledTime + 3f;
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void ResetSession()
        {
            Session = new MissionProgress();
            GeneratorStartedAt = -1f;
            FuseNoticeUntil = 0f;
            NoiseEvents.ResetFootsteps();
        }
    }
}

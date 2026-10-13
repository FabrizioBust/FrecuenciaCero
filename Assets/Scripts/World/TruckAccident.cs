using UnityEngine;

namespace FrecuenciaCero
{
    [DisallowMultipleComponent]
    public sealed class TruckAccident : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer intactTruck;
        [SerializeField] private GameObject wreck;
        [SerializeField] private Transform discoveryPoint;
        [SerializeField] private Transform leo;
        [SerializeField] private NarrativeDirector director;
        [SerializeField] private DialogueSequence discovery;
        [SerializeField, Min(.1f)] private float discoveryRange = 1.8f;
        private InteractionProbe probe;
        private bool visible;

        private void Awake() { if (leo != null) probe = new InteractionProbe(leo); }
        private void OnEnable() { Refresh(); }
        private void Refresh()
        {
            visible = RetenMission.Session.TreeFallen;
            if (intactTruck != null) intactTruck.enabled = !visible;
            if (wreck != null) wreck.SetActive(visible);
        }
        private void Update()
        {
            MissionProgress state = RetenMission.Session;
            if (visible != state.TreeFallen) Refresh();
            if (!visible || state.WreckDiscovered || director == null || director.IsOpen ||
                RetenTravel.IsLoading || discoveryPoint == null || probe == null) return;
            Vector2 point = discoveryPoint.position;
            Vector2 approach = probe.Position - point;
            // Proximity plus line of sight; cannot discover through the trunk or other obstacles.
            if (!probe.CanReach(point, discoveryRange, approach, discoveryRange)) return;
            director.OpenDialogue(discovery, () => state.CompleteWreckDiscovery());
        }
    }
}

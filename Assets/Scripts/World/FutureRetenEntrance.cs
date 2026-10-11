using UnityEngine;

namespace FrecuenciaCero
{
    // Authoring marker only: no input, interaction, scene loading or puzzle behavior yet.
    [DisallowMultipleComponent]
    public sealed class FutureRetenEntrance : MonoBehaviour
    {
        [SerializeField] private string plannedScene = "N1_InteriorReten";
        [SerializeField] private string plannedKey = "E";
        public string PlannedScene => plannedScene;
        public string PlannedKey => plannedKey;
    }
}

using UnityEngine;

namespace FrecuenciaCero
{
    // Handles direct editor entry. Normal room travel keeps the current chapter objective.
    public sealed class CabinRoomContext : MonoBehaviour
    {
        private void Start()
        {
            if (!RetenMission.Session.Chapters.CabinEntered) RetenMission.Session.EnterCabin();
        }
    }
}

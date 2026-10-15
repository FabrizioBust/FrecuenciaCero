using UnityEngine;

namespace FrecuenciaCero
{
    public enum FootstepSurface { Dirt, Grass, RetenWood, CabinWood }
    [CreateAssetMenu(menuName="Frecuencia Cero/Audio/Paleta de sonidos")]
    public sealed class GameSoundPalette : ScriptableObject
    {
        public AudioClip[] dirt, grass, retenWood, cabinWood;
        public AudioClip motorRunning, motorStopping, truckDoor, river;
        public AudioClip[] radioStatic;
        public AudioClip radioCut;
        public AudioClip[] Steps(FootstepSurface surface)
        {
            switch(surface){case FootstepSurface.Grass:return grass;case FootstepSurface.RetenWood:return retenWood;case FootstepSurface.CabinWood:return cabinWood;default:return dirt;}
        }
    }
}

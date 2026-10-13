using System;
using UnityEngine;

namespace FrecuenciaCero
{
    public enum NoiseKind { Walk, Run, Bottle, Soil, Environment }
    public readonly struct NoiseEvent
    {
        public readonly Vector2 Position;
        public readonly float Intensity, Radius;
        public readonly NoiseKind Kind;
        public NoiseEvent(Vector2 position, float intensity, float radius, NoiseKind kind)
        { Position = position; Intensity = intensity; Radius = radius; Kind = kind; }
    }
    public static class NoiseEvents
    {
        public static event Action<NoiseEvent> Emitted;
        public static NoiseEvent LastFootstep { get; private set; }
        public static void Emit(NoiseEvent noise)
        {
            if (noise.Intensity <= 0 || noise.Radius <= 0) return;
            if (noise.Kind == NoiseKind.Walk || noise.Kind == NoiseKind.Run) LastFootstep = noise;
            Emitted?.Invoke(noise);
        }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset() { Emitted = null; LastFootstep = default; }
        public static void ResetFootsteps() => LastFootstep = default;
    }
}

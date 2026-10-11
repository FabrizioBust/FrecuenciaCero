using System;
using UnityEngine;

namespace FrecuenciaCero
{
    [Serializable]
    public sealed class DialogueLine
    {
        public string speaker;
        [TextArea(2, 5)] public string text;
        [Tooltip("Identificador opcional para futuros sonidos; no reproduce audio por sí mismo.")]
        public string soundCue;
    }

    [CreateAssetMenu(menuName = "Frecuencia Cero/Narrativa/Diálogo")]
    public sealed class DialogueSequence : ScriptableObject
    {
        public DialogueLine[] lines = new DialogueLine[0];
    }
}

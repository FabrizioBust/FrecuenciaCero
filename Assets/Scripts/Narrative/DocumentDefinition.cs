using UnityEngine;

namespace FrecuenciaCero
{
    [CreateAssetMenu(menuName = "Frecuencia Cero/Narrativa/Documento")]
    public sealed class DocumentDefinition : ScriptableObject
    {
        public string title;
        [TextArea(8, 24)] public string body;
        public Sprite illustration;
    }
}

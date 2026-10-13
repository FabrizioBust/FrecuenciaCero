using UnityEngine;

namespace FrecuenciaCero
{
    [CreateAssetMenu(menuName = "Frecuencia Cero/Inventario de supervivencia")]
    public sealed class SurvivalInventorySettings : ScriptableObject
    {
        [SerializeField, Min(1), Tooltip("Capacidad compartida por todas las escenas; no altera la vida máxima.")]
        private int maximumMedkits = 3;
        public int MaximumMedkits => Mathf.Max(1, maximumMedkits);
        public static int MedkitCapacity
        {
            get
            {
                var settings = Resources.Load<SurvivalInventorySettings>("FrecuenciaCero/InventorySettings");
                return settings != null ? settings.MaximumMedkits : 3;
            }
        }
    }
}

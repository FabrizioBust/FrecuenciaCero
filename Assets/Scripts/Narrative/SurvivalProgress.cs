using System.Collections.Generic;

namespace FrecuenciaCero
{
    // Owned by the existing mission session; no persistent scene objects or duplicated player.
    public sealed class SurvivalProgress
    {
        public const int MaximumHealth = 3, MaximumBatteries = 5, MaximumBottles = 10;
        public int Health { get; private set; } = MaximumHealth;
        public int Batteries { get; private set; }
        public int Bottles { get; private set; }
        public int Medkits { get; private set; }
        public int MaximumMedkits { get; private set; } = 3;
        private readonly HashSet<string> collected = new HashSet<string>();
        // A shared asset supplies the capacity; changing scenes never clears stock or collected IDs.
        public void ConfigureMedkitCapacity(int capacity) => MaximumMedkits = System.Math.Max(Medkits, System.Math.Max(1, capacity));
        public bool WasCollected(string id) => collected.Contains(id);
        public bool Collect(string id, SupplyKind kind)
        {
            if (string.IsNullOrWhiteSpace(id) || WasCollected(id)) return false;
            if (kind == SupplyKind.Battery && Batteries >= MaximumBatteries ||
                kind == SupplyKind.Bottle && Bottles >= MaximumBottles ||
                kind == SupplyKind.Medkit && Medkits >= MaximumMedkits) return false;
            collected.Add(id);
            if (kind == SupplyKind.Battery) Batteries++;
            else if (kind == SupplyKind.Bottle) Bottles++;
            else Medkits++;
            return true;
        }
        public bool SpendBottle() { if (Bottles <= 0) return false; Bottles--; return true; }
        public bool UseBattery() { if (Batteries <= 0) return false; Batteries--; return true; }
        public bool UseMedkit()
        {
            if (Health <= 0 || Health >= MaximumHealth || Medkits <= 0) return false;
            Medkits--; Health++; return true;
        }
        public bool Damage(int amount)
        {
            if (Health <= 0 || amount <= 0) return false;
            Health = System.Math.Max(0, Health - amount); return true;
        }
        public void RetryHealth() => Health = MaximumHealth;
    }
    public enum SupplyKind { Battery, Bottle, Medkit }
}

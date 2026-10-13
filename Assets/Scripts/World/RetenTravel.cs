using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrecuenciaCero
{
    // Only the arrival and flashlight snapshot survive travel; scenes own their existing systems.
    public static class RetenTravel
    {
        public static bool IsLoading { get; private set; }
        private static string pendingScene;
        private static string pendingSpawn;
        private static float charge;
        private static bool flashlightOn;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void Reset()
        {
            IsLoading = false;
            pendingScene = pendingSpawn = null;
        }

        public static bool Begin(string scenePath, string spawnId, FlashlightBattery battery, FlashlightController flashlight)
        {
            if (IsLoading) return false;
            if (!Application.CanStreamedLevelBeLoaded(scenePath))
            {
                Debug.LogError("Falta la escena en Build Profiles > Scene List: " + scenePath);
                return false;
            }
            charge = battery != null ? battery.Percent : 100f;
            flashlightOn = flashlight == null || flashlight.IsOn;
            pendingScene = System.IO.Path.GetFileNameWithoutExtension(scenePath);
            pendingSpawn = spawnId;
            IsLoading = true;
            try
            {
                AsyncOperation operation = SceneManager.LoadSceneAsync(scenePath, LoadSceneMode.Single);
                if (operation == null) throw new InvalidOperationException("Unity no inicio la carga de " + scenePath);
                operation.completed += _ => IsLoading = false;
                return true;
            }
            catch (Exception error)
            {
                IsLoading = false;
                pendingScene = pendingSpawn = null;
                Debug.LogException(error);
                return false;
            }
        }

        public static bool TryArrive(string sceneName, string spawnId, FlashlightBattery battery, FlashlightController flashlight)
        {
            if (sceneName != pendingScene || spawnId != pendingSpawn) return false;
            battery?.RestoreCharge(charge);
            flashlight?.SetOn(flashlightOn);
            pendingScene = pendingSpawn = null;
            return true;
        }
    }
}

using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace FrecuenciaCero.Editor
{
    public static class BosqueNightLightingSetup
    {
        private const string ScenePath = "Assets/Scenes/Nivel01_Bosque.unity";
        private const string MaterialPath = "Assets/Art/Placeholders/ProvisionalLit.mat";

        [MenuItem("Frecuencia Cero/Configurar iluminacion de Etapa 2")]
        public static void Configure()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Detén Play antes de configurar la iluminación.");
            Scene scene = SceneManager.GetSceneByPath(ScenePath);
            bool openedHere = !scene.IsValid() || !scene.isLoaded;
            if (openedHere) scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Additive);
            Scene previous = SceneManager.GetActiveScene();
            try
            {
                if (scene.isDirty)
                    throw new InvalidOperationException("Guarda los cambios propios de Nivel01_Bosque antes de configurar la iluminación.");
                Directory.CreateDirectory("Docs/Backups");
                File.Copy(ScenePath, "Docs/Backups/Nivel01_Bosque.pre-configuracion-" + Guid.NewGuid().ToString("N") + ".unity.txt");
                SceneManager.SetActiveScene(scene);
                Transform leo = null;
                Camera camera = null;
                Light2D ambient = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    foreach (Transform item in root.GetComponentsInChildren<Transform>(true))
                        if (item.name == "Leo") leo = item;
                    foreach (Camera item in root.GetComponentsInChildren<Camera>(true))
                        if (item.CompareTag("MainCamera")) camera = item;
                    foreach (Light2D item in root.GetComponentsInChildren<Light2D>(true))
                        if (item.lightType == Light2D.LightType.Global) ambient = item;
                }
                if (leo == null || camera == null || ambient == null)
                    throw new InvalidOperationException("La escena debe conservar Leo, Main Camera y Global Light 2D.");
                var shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Lit-Default");
                if (shader == null) throw new InvalidOperationException("Falta Sprite-Lit-Default de URP 2D.");
                var material = AssetDatabase.LoadAssetAtPath<Material>(MaterialPath);
                if (material == null)
                {
                    material = new Material(shader);
                    AssetDatabase.CreateAsset(material, MaterialPath);
                }
                foreach (GameObject root in scene.GetRootGameObjects())
                    foreach (SpriteRenderer sprite in root.GetComponentsInChildren<SpriteRenderer>(true))
                        if (sprite.sharedMaterial != null && sprite.sharedMaterial.name == "ProvisionalUnlit")
                            sprite.sharedMaterial = material;
                ambient.color = new Color(0.64f, 0.72f, 0.86f);
                ambient.intensity = 0.45f;

                Transform flashlight = leo.Find("Linterna");
                if (flashlight == null)
                {
                    flashlight = new GameObject("Linterna").transform;
                    flashlight.SetParent(leo, false);
                    flashlight.localPosition = new Vector3(0f, 0.2f, 0f);
                    var light = flashlight.gameObject.AddComponent<Light2D>();
                    light.lightType = Light2D.LightType.Point;
                    light.blendStyleIndex = 0;
                    light.pointLightInnerAngle = 34f;
                    light.pointLightOuterAngle = 64f;
                    light.pointLightInnerRadius = 1.925f;
                    light.pointLightOuterRadius = 5.5f;
                    light.intensity = 1.6f;
                    light.color = new Color(1f, 0.93f, 0.78f);
                    light.shadowsEnabled = false;
                    var lightSettings = new SerializedObject(light);
                    var layers = lightSettings.FindProperty("m_ApplyToSortingLayers");
                    layers.arraySize = 1;
                    layers.GetArrayElementAtIndex(0).intValue = 0;
                    lightSettings.ApplyModifiedPropertiesWithoutUndo();
                }
                var controller = flashlight.GetComponent<FlashlightController>();
                if (controller == null) controller = flashlight.gameObject.AddComponent<FlashlightController>();
                var controllerSettings = new SerializedObject(controller);
                controllerSettings.FindProperty("worldCamera").objectReferenceValue = camera;
                controllerSettings.ApplyModifiedPropertiesWithoutUndo();
                var battery = flashlight.GetComponent<FlashlightBattery>();
                if (battery == null) battery = flashlight.gameObject.AddComponent<FlashlightBattery>();

                BatteryUI hud = null;
                foreach (GameObject root in scene.GetRootGameObjects())
                    if (root.TryGetComponent(out BatteryUI existing)) hud = existing;
                if (hud == null) hud = new GameObject("Interfaz_Bateria").AddComponent<BatteryUI>();
                var hudSettings = new SerializedObject(hud);
                hudSettings.FindProperty("battery").objectReferenceValue = battery;
                hudSettings.FindProperty("flashlight").objectReferenceValue = controller;
                hudSettings.ApplyModifiedPropertiesWithoutUndo();
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
                AssetDatabase.SaveAssets();
                Debug.Log("Etapa 2 guardada: linterna, batería e iluminación nocturna.");
            }
            finally
            {
                if (previous.IsValid() && previous.isLoaded) SceneManager.SetActiveScene(previous);
                if (openedHere) EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}

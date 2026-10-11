using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FrecuenciaCero.Editor
{
    public static class ExteriorRetenTools
    {
        [MenuItem("Frecuencia Cero/Exterior Reten/Guardar Tilemaps preparados")]
        public static void SaveTerrain()
        {
            Save(false);
        }

        [MenuItem("Frecuencia Cero/Exterior Reten/Reconstruir terreno organico y guardar")]
        public static void RebuildTerrain()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Deten Play antes de reconstruir el terreno.");
            OrganicTerrainBaker.Bake();
            Save(true);
        }

        private static void Save(bool rebuild)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Detén Play antes de guardar los Tilemaps.");
            const string path = "Assets/Scenes/Nivel01/N1_ExteriorReten.unity";
            Scene scene = SceneManager.GetSceneByPath(path);
            bool openedHere = !scene.IsValid() || !scene.isLoaded;
            if (openedHere) scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                foreach (GameObject root in scene.GetRootGameObjects())
                    foreach (ExteriorTerrain terrain in root.GetComponentsInChildren<ExteriorTerrain>(true))
                    {
                        if (rebuild) terrain.RebuildGeneratedSurface();
                        else terrain.BuildIfNeeded();
                        if (!terrain.IsPrepared)
                            throw new InvalidOperationException("El terreno no esta preparado. Espera a que termine la importacion; revisa SueloOrganico.png y vuelve a ejecutar el menu.");
                    }
                foreach (GameObject root in scene.GetRootGameObjects())
                    foreach (StreamCollisionLayout river in root.GetComponentsInChildren<StreamCollisionLayout>(true))
                        river.Rebuild();
                EditorSceneManager.MarkSceneDirty(scene);
                if (!EditorSceneManager.SaveScene(scene))
                    throw new InvalidOperationException("Unity no pudo guardar N1_ExteriorReten.");
                Debug.Log("N1_ExteriorReten: Tilemaps guardados sin reconstruir objetos del exterior.");
            }
            finally
            {
                if (openedHere) EditorSceneManager.CloseScene(scene, true);
            }
        }
    }
}

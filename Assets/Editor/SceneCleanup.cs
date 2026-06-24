using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections.Generic;

public static class SceneCleanup
{
    [MenuItem("Tools/Cleanup Scene (remove broken refs)")]
    public static void Run()
    {
        var scene = SceneManager.GetActiveScene();
        int removed = 0;
        var toDestroy = new List<GameObject>();

        foreach (var root in scene.GetRootGameObjects())
        {
            CollectBroken(root, toDestroy);
        }

        foreach (var go in toDestroy)
        {
            if (go != null)
            {
                Object.DestroyImmediate(go);
                removed++;
            }
        }

        if (removed > 0)
        {
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log($"Cleanup: removed {removed} broken GameObject(s) from '{scene.name}'.");
        }
        else
        {
            Debug.Log("Cleanup: nothing to remove.");
        }
    }

    static void CollectBroken(GameObject go, List<GameObject> bucket)
    {
        if (go == null) return;
        if (IsBroken(go)) bucket.Add(go);
        for (int i = 0; i < go.transform.childCount; i++)
            CollectBroken(go.transform.GetChild(i).gameObject, bucket);
    }

    static bool IsBroken(GameObject go)
    {
        if (go == null) return true;
        if (go.name.Contains("(Missing)")) return true;
        if (go.name.StartsWith("EA03_") || go.name.Contains("_EA03_")) return true;
        if (go.name.EndsWith("LOD0") || go.name.EndsWith("LOD1") || go.name.EndsWith("LOD2")) return true;
        if (go.name == "GameObject" && go.GetComponent<MeshFilter>() == null) return true;

        var mf = go.GetComponent<MeshFilter>();
        if (mf != null && mf.sharedMesh == null) return true;

        var rend = go.GetComponent<Renderer>();
        if (rend != null && rend.sharedMaterials != null)
        {
            foreach (var m in rend.sharedMaterials)
                if (m == null) return true;
        }
        return false;
    }

    [MenuItem("Tools/Cleanup Library (delete Library/, Logs/, Temp/)")]
    public static void CleanLibrary()
    {
        if (!EditorUtility.DisplayDialog("Cleanup Library",
            "Delete Library/, Logs/, Temp/ and force a reimport?\n" +
            "This can take several minutes. Unity will not be usable during the reimport.",
            "Yes, clean", "Cancel")) return;

        var projectRoot = System.IO.Directory.GetParent(Application.dataPath).FullName;
        foreach (var dir in new[] { "Library", "Logs", "Temp", "obj" })
        {
            var path = System.IO.Path.Combine(projectRoot, dir);
            if (System.IO.Directory.Exists(path))
            {
                try { System.IO.Directory.Delete(path, true); Debug.Log($"Deleted {dir}/"); }
                catch (System.Exception e) { Debug.LogWarning($"Could not delete {dir}/: {e.Message}"); }
            }
        }
        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
    }
}

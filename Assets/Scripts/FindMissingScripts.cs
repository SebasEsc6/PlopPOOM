using UnityEditor;
using UnityEngine;

public class FindMissingScripts : EditorWindow
{
    [MenuItem("Tools/Find Missing Scripts in All Prefabs")]
    static void ShowWindow() => GetWindow<FindMissingScripts>();

    void OnGUI()
    {
        if (GUILayout.Button("Scan Prefabs"))
            Scan();
    }

    static void Scan()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab"))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var comps = prefab.GetComponentsInChildren<Component>(true);
            for (int i = 0; i < comps.Length; i++)
            {
                if (comps[i] == null)
                    Debug.LogError($"Missing script in Prefab: {path}, child index {i}");
            }
        }
    }
}

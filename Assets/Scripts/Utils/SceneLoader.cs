using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Static utility for global scene management using scene names.
/// </summary>
public static class SceneLoader
{
    /// <summary>
    /// Loads a scene by name.
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("SceneLoader: Scene name is null or empty.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Reloads the currently active scene.
    /// </summary>
    public static void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class _SceneController : MonoBehaviour
{
    [SerializeField] private GameObject networkManagerPrefab;
    private GameObject networkManagerInstance;

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void GoScene(string nameScene)
    {
        SceneManager.LoadScene(nameScene);
    }

    public void GoMultiplayerScene(string nameScene)
    {
        SceneManager.LoadScene(nameScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MultiplayerScene") 
        {
            if (networkManagerInstance == null)
            {
                networkManagerInstance = Instantiate(networkManagerPrefab);
            }
        }
    }
}

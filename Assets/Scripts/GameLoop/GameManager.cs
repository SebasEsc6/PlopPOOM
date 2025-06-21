using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
        GetDatabase();
    }
    public void GetDatabase()
    {
        SORegistry.RegisterAll<SO_Item>("SO/Items");
        // SORegistry.RegisterAll<SO_Weapons>("Weapons");
        SORegistry.RegisterAll<SO_PowerUps>("SO/PowerUps");
    }
}

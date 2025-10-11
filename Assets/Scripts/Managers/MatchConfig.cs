using UnityEngine;

public static class MatchConfig
{
    public static int LivesPerPlayer { get; private set; } = 1;

    public static void SetLives(int lives)
    {
        LivesPerPlayer = Mathf.Max(1, lives);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetOnRunStart()
    {
        LivesPerPlayer = 1;
    }
}

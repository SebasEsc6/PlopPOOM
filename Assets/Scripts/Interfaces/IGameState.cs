public interface IGameState
{
    /// <summary>
    /// Called when the state is entered.
    /// Use this to initialize timers, spawn logic, etc.
    /// </summary>
    /// <param name="manager">Reference to the GameManager</param>
    void EnterState(GameManager manager);

    /// <summary>
    /// Called every frame while this state is active.
    /// Should contain logic that needs to run during the state.
    /// </summary>
    /// <param name="manager">Reference to the GameManager</param>
    void UpdateState(GameManager manager);

    /// <summary>
    /// Called when exiting the current state.
    /// Use this to clean up state-specific data or stop timers.
    /// </summary>
    /// <param name="manager">Reference to the GameManager</param>
    void ExitState(GameManager manager);
}

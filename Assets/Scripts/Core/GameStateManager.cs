using System;
using UnityEngine;

public enum GameState
{
    MenuGameplay,
    Dialogue, // Talking to NPCs (Can't move)
    Paused, //Menu is open
    LevelComplete,
    GameOver,
    Ending // Conclude the game (e.g., show credits)
}

public class GameStateManager : MonoBehaviour
{
    //Singleton
    public static GameStateManager instance { get; private set; }

    public GameState curState { get; private set; }

    public event Action<GameState> onStateChange;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            
            // Initial state
            curState = GameState.MenuGameplay;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ChangeState(GameState newState)
    {
        //  Allow re-triggering Dialogue after scene reload
        //  Allow Ending → Dialogue transition for restart
        if (curState == newState && newState != GameState.Dialogue && newState != GameState.Ending)
        {
            Debug.LogWarning($"[GameStateManager] Already in state: {newState}");
            return;
        }

        GameState oldState = curState;
        curState = newState;
        
        Debug.Log($"[GameStateManager] State: {oldState} → {newState}");
        
        onStateChange?.Invoke(newState);
    }

    // Helper methods
    public bool IsInDialogue() => curState == GameState.Dialogue;
    public bool IsEnding() => curState == GameState.Ending;
    public bool IsPaused() => curState == GameState.Paused;
}

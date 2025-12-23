using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles ending screen buttons (Return to Menu / Exit Game)
/// </summary>
public class EndingButtonsHandler : MonoBehaviour
{
    [Header("Button References")]
    [SerializeField] private Button returnToMenuButton;
    [SerializeField] private Button exitGameButton;

    [Header("Settings")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private void Start()
    {
        SetupButtons();
    }

    private void SetupButtons()
    {
        // Return to Menu button
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.AddListener(OnReturnToMenu);
            Debug.Log("[EndingButtons] ✅ Return to Menu button setup complete");
        }
        else
        {
            Debug.LogWarning("[EndingButtons] ⚠️ Return to Menu button not assigned!");
        }

        // Exit Game button
        if (exitGameButton != null)
        {
            exitGameButton.onClick.AddListener(OnExitGame);
            Debug.Log("[EndingButtons] ✅ Exit Game button setup complete");
        }
        else
        {
            Debug.LogWarning("[EndingButtons] ⚠️ Exit Game button not assigned!");
        }
    }

    private void OnReturnToMenu()
    {
        Debug.Log("[EndingButtons] 🔄 Returning to Main Menu...");

        // Play click sound
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayButtonClick();
        }

        // Stop all audio
        if (SoundManager.instance != null)
        {
            SoundManager.instance.StopAll();
        }

        // Reset global state
        if (GlobalStateManager.instance != null)
        {
            GlobalStateManager.instance.ResetState();
            Debug.Log("[EndingButtons] Global state reset");
        }

        // Change game state
        if (GameStateManager.instance != null)
        {
            GameStateManager.instance.ChangeState(GameState.MenuGameplay);
            Debug.Log("[EndingButtons] Game state changed to MenuGameplay");
        }

        // Load Main Menu
        Debug.Log($"[EndingButtons] Loading scene: {mainMenuSceneName}");
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnExitGame()
    {
        Debug.Log("[EndingButtons] 👋 Exiting game...");

        // Play click sound
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayButtonClick();
        }

        // Exit application
#if UNITY_EDITOR
        Debug.Log("[EndingButtons] (Editor) Stopping play mode");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Debug.Log("[EndingButtons] Quitting application");
        Application.Quit();
#endif
    }

    private void OnDestroy()
    {
        Debug.Log("[EndingButtons] Cleaning up button listeners...");

        // Clean up listeners
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.RemoveAllListeners();
        }

        if (exitGameButton != null)
        {
            exitGameButton.onClick.RemoveAllListeners();
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Main Menu controller - Manages its own background music
/// </summary>
public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button playButton;
    [SerializeField] private Button exitButton; // ✅ NEW: Exit button

    [Header("Audio")]
    [SerializeField] private AudioSource menuAudioSource;
    [SerializeField] private AudioClip menuBGM;
    [SerializeField] private AudioClip buttonClickSound;
    [SerializeField] private AudioClip buttonHoverSound;

    [Header("Settings")]
    [SerializeField] private string gameplaySceneName = "Level1";

    private void Awake()
    {
        // Create audio source if not assigned
        if (menuAudioSource == null)
        {
            menuAudioSource = gameObject.AddComponent<AudioSource>();
            menuAudioSource.loop = true;
            menuAudioSource.playOnAwake = false;
        }
    }

    private void Start()
    {
        // ✅ Play menu BGM directly
        if (menuBGM != null && menuAudioSource != null)
        {
            menuAudioSource.clip = menuBGM;
            menuAudioSource.Play();
            Debug.Log("[MainMenu] ✅ Playing Menu BGM");
        }
        else
        {
            Debug.LogError("[MainMenu] ❌ Menu BGM or AudioSource not assigned!");
        }

        // Setup Play button
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
            Debug.Log("[MainMenu] ✅ Play button setup complete");
        }
        else
        {
            Debug.LogError("[MainMenu] ❌ Play button is not assigned!");
        }

        // ✅ Setup Exit button
        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitButtonClicked);
            Debug.Log("[MainMenu] ✅ Exit button setup complete");
        }
        else
        {
            Debug.LogWarning("[MainMenu] ⚠️ Exit button is not assigned!");
        }

        // Add button animations
        AddButtonAnimation(playButton);
        AddButtonAnimation(exitButton);
    }

    private void OnPlayButtonClicked()
    {
        Debug.Log("[MainMenu] 🎮 Play button clicked!");

        // Play button click sound
        if (buttonClickSound != null && menuAudioSource != null)
        {
            menuAudioSource.PlayOneShot(buttonClickSound);
        }

        // ✅ Load scene directly (no fade)
        Debug.Log($"[MainMenu] Loading scene: {gameplaySceneName}");
        SceneManager.LoadScene(gameplaySceneName);
    }

    // ✅ NEW: Exit button handler
    private void OnExitButtonClicked()
    {
        Debug.Log("[MainMenu] 👋 Exit button clicked!");

        // Play button click sound
        if (buttonClickSound != null && menuAudioSource != null)
        {
            menuAudioSource.PlayOneShot(buttonClickSound);
        }

        // Exit application
#if UNITY_EDITOR
        Debug.Log("[MainMenu] (Editor) Stopping play mode");
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Debug.Log("[MainMenu] Quitting application");
        Application.Quit();
#endif
    }

    private void AddButtonAnimation(Button button)
    {
        if (button != null)
        {
            // Add hover animator
            var animator = button.gameObject.GetComponent<UIButtonHoverAnimator>();
            if (animator == null)
            {
                animator = button.gameObject.AddComponent<UIButtonHoverAnimator>();
                animator.hoverScale = 1.1f;
                animator.animDuration = 0.2f;
                animator.hoverSound = buttonHoverSound;
            }
        }
    }

    private void OnDestroy()
    {
        if (playButton != null)
        {
            playButton.onClick.RemoveAllListeners();
        }

        if (exitButton != null)
        {
            exitButton.onClick.RemoveAllListeners();
        }
    }
}
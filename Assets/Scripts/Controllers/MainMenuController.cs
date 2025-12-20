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

        // Setup button listener
        if (playButton != null)
        {
            playButton.onClick.AddListener(OnPlayButtonClicked);
            Debug.Log("[MainMenu] ✅ Play button setup complete");
        }
        else
        {
            Debug.LogError("[MainMenu] ❌ Play button is not assigned!");
        }

        // Add button animation
        AddButtonAnimation();
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

    private void AddButtonAnimation()
    {
        if (playButton != null)
        {
            // Add hover animator
            var animator = playButton.gameObject.GetComponent<UIButtonHoverAnimator>();
            if (animator == null)
            {
                animator = playButton.gameObject.AddComponent<UIButtonHoverAnimator>();
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
    }
}
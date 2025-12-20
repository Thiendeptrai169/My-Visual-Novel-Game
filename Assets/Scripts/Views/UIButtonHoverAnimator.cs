using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// Simple hover animation for UI buttons - No SoundManager dependency
/// </summary>
public class UIButtonHoverAnimator : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Animation Settings")]
    public float hoverScale = 1.1f;
    public float animDuration = 0.2f;

    [Header("Audio (Optional)")]
    public AudioClip hoverSound;
    public float hoverSoundVolume = 0.6f;

    private Vector3 originalScale;
    private bool isInitialized = false;
    private AudioSource audioSource;

    private void Start()
    {
        Initialize();
    }

    private void Initialize()
    {
        if (!isInitialized)
        {
            originalScale = transform.localScale;

            // Get or create audio source if hover sound is assigned
            if (hoverSound != null)
            {
                audioSource = GetComponentInParent<AudioSource>();
                if (audioSource == null)
                {
                    // Try to find MainMenuController's audio source
                    var mainMenu = FindObjectOfType<MainMenuController>();
                    if (mainMenu != null)
                    {
                        audioSource = mainMenu.GetComponent<AudioSource>();
                    }
                }
            }

            isInitialized = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isInitialized)
            Initialize();

        // Play hover sound if available
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound, hoverSoundVolume);
        }

        // Scale up animation
        transform.DOScale(originalScale * hoverScale, animDuration)
            .SetEase(Ease.OutBack)
            .SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isInitialized)
            Initialize();

        // Scale back to original
        transform.DOScale(originalScale, animDuration)
            .SetEase(Ease.OutQuad)
            .SetUpdate(true);
    }

    private void OnDisable()
    {
        if (isInitialized)
        {
            transform.DOKill();
            transform.localScale = originalScale;
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }
}
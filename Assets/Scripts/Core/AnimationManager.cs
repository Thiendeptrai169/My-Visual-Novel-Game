using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// Manages animations throughout the game
//Don't need to be attached to the Inspector, automatically called by other scripts
public class AnimationManager : MonoBehaviour
{
    public static AnimationManager instance;

    [Header("Defalut Setting")]
    public float defaultFadeDuration = 0.3f;
    public float defaultScaleDuration = 0.5f;
    public float defaultSlideDuration = 0.4f;
    public float defaultTypingSpeed = 0.05f;

    [Header("Tension Animation Settings")]
    public float tensionPulseStrength = 0.15f;
    public float tensionPulseDuration = 0.8f;
    public Color tensionCriticalColor = new Color(0.56f, 0.27f, 0.68f); // Purple


    [Header("Text Animation Components")]
    private bool isTyping = false;
    private bool isSkipping = false;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);

            //Configure DOTween
            DOTween.Init(false, true, LogBehaviour.ErrorsOnly);
            DOTween.defaultEaseType = Ease.OutQuad;
            
        }
        else
        {
            Destroy(gameObject);
        }
    }

    #region Fade Animations

    // Fades in a CanvasGroup over a specified duration
    public void FadeIn(CanvasGroup canvasGroup, float duration = -1, System.Action onComplete = null)
    {
        if (canvasGroup == null) return;

        if (duration < 0) duration = defaultFadeDuration;

        canvasGroup.alpha = 0; //Set the transparency of the canvas group
        canvasGroup.DOFade(1, duration).OnComplete(() => onComplete?.Invoke()); //DOTween fade     
    }

    public void FadeOut(CanvasGroup canvasGroup, float duration, System.Action onComplete = null)
    {
        
        if (canvasGroup == null)
        {
            Debug.LogError("[AnimationManager] CanvasGroup is NULL!");
            onComplete?.Invoke();
            return;
        }

        canvasGroup.DOFade(0f, duration)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                //Debug.Log("[AnimationManager] Fade out complete");
                onComplete?.Invoke();
            });
    }

    // Fade with Image
    public void FadeInImage(Image image, float duration = -1, System.Action onComplete = null)
    {
        if (image == null) return;
        if (duration < 0) duration = defaultFadeDuration;

        Color color = image.color;
        color.a = 0;
        image.color = color;
        image.DOFade(1, duration).OnComplete(() => onComplete?.Invoke());
    }

    public void FadeOutImage(Image image, float duration = -1, System.Action onComplete = null)
    {
        if (image == null) return;
        if (duration < 0) duration = defaultFadeDuration;

        image.DOFade(0, duration).OnComplete(() => onComplete?.Invoke());
    }

    #endregion

    #region Scale Animations
    public void PopIn(Transform target, float duration = -1, System.Action onComplete = null)
    {
        if (target == null) return;
        if (duration < 0) duration = defaultScaleDuration;
        target.localScale = Vector3.zero; //Start from zero scale
        target.DOScale(Vector3.one, duration)
              .SetEase(Ease.OutBack) // set the easing (speed of the animation) 
              .OnComplete(() => onComplete?.Invoke());
    }

    public void PopOut(Transform target, float duration = -1, System.Action onComplete = null)
    {
        if (target == null) return;
        if (duration < 0) duration = defaultScaleDuration;
        target.DOScale(Vector3.zero, duration)
              .SetEase(Ease.InBack) // set the easing (speed of the animation) 
              .OnComplete(() => onComplete?.Invoke());
    }

    public void PunchScale(Transform target, float strength = 0.1f)
    {
        if (target == null) return;
        target.DOPunchScale(Vector3.one * strength, 0.3f, 10, 1);

    }

    //Zoom in and out quickly
    public void BounceScale(Transform target, float targetScale = 1.1f, float duration = 0.2f)
    {
        if (target == null) return;
        target.DOScale(targetScale, duration)
              .SetEase(Ease.OutBack);
    }

    //reset scale
    public void ResetScale(Transform target, float duration = 0.2f)
    {
        if (target == null) return;
        target.DOScale(1f, duration)
              .SetEase(Ease.OutQuad);
    }

    #endregion

    #region Shake Animations

    public void ShakePosition(Transform target, float strength = 10f, float duration = 0.3f) 
    {
        if(target == null) return;
        target.DOShakePosition(duration,strength,10,90,false, true);
    }

    public void ScreenShake (float strength = 0.3f, float duration = 0.5f)
    {
        if(Camera.main == null) return;
        Camera.main.DOShakePosition(duration, strength, 10, 90, false);
    }
    #endregion


    #region Slide Animations
    public void SlideInFromBottom(RectTransform target, float duration = -1, System.Action onComplete = null)
    {
        if(target == null) return;
        if(duration < 0) duration = defaultSlideDuration;

        Vector2 originalPos = target.anchoredPosition;
        Vector2 startPos = new Vector2(originalPos.x, originalPos.y - 500f);

        target.anchoredPosition = startPos; //Set start position at the bottom

        target.DOAnchorPos(originalPos, duration) //Set back to original position with animation(DOTween)
              .SetEase(Ease.OutQuad)
              .OnComplete(() => onComplete?.Invoke());
    }
    
    // Slide for choices
    public void SlideInFromRight(RectTransform target, float delay = 0f, System.Action onComplete = null)
    {
        if (target == null) return;

        Vector2 originalPos = target.anchoredPosition;
        Vector2 startPos = new Vector2(originalPos.x + 800f, originalPos.y);
        target.anchoredPosition = startPos; //Set start position at the right

        target.DOAnchorPos(originalPos, defaultSlideDuration) //Set back to original position with animation(DOTween)
              .SetDelay(delay)
              .SetEase(Ease.OutCubic)
              .OnComplete(() => onComplete?.Invoke());
    }
    
    public void SlideInFromLeft(RectTransform target, float duration = - 1, System.Action onComplete = null)
    {
        if (target == null) return;
        if (duration < 0) duration = defaultSlideDuration;
        Vector2 originalPos = target.anchoredPosition;
        Vector2 startPos = new Vector2(originalPos.x - 800f, originalPos.y);
        target.anchoredPosition = startPos; //Set start position at the left
        target.DOAnchorPos(originalPos, duration) //Set back to original position with animation(DOTween)
              .SetEase(Ease.InCubic)
              .OnComplete(() => onComplete?.Invoke());
    }

    #endregion

    #region Text Animations
    //public Tween TypeWritterEffect(TMP_Text txtComponent, string fullText, float speed = -1, System.Action onComplete = null)
    //{
    //    if (txtComponent == null) return null;
    //    if(speed < 0)  speed = defaultTypingSpeed;

    //    float duration = fullText.Length * speed;

    //    txtComponent.text = ""; //Clear existing text

    //    return txtComponent.DOText(fullText, duration)
    //                       .SetEase(Ease.Linear)
    //                       .OnComplete(() => onComplete?.Invoke());

    //}

    public void SkipTyping()
    {
        if (isTyping)
        {
            isSkipping = true;
        }
    }

    public IEnumerator TypeText(TMP_Text text, string fullText, float duration, System.Action onComplete = null)
    {
        isTyping = true;
        isSkipping = false;

        text.text = "";
        float t = 0f;
        int totalChars = fullText.Length;

        while (t < duration)
        {
            if (isSkipping) 
            {
                text.text = fullText; // Show full text immediately
                isTyping = false;
                yield break;
            }

            int visibleCount = Mathf.FloorToInt((t / duration) * totalChars); // Calculate number of characters to show
            text.text = fullText.Substring(0, visibleCount); // Update text
            t += Time.deltaTime; // Increment time
            yield return null;
        }
        text.text = fullText; // Ensure full text is displayed at the end
        isTyping = false;
        onComplete?.Invoke();
    }

    //changing the text color to target color and back repeatedly
    public void PulseTextColor(TMP_Text txtComponent, Color targetColor, float duration = 0.5f) 
    {
        if(txtComponent == null) return;

        Sequence pulseSequence = DOTween.Sequence();
        pulseSequence.Append(txtComponent.DOColor(targetColor,duration))
                     .Append(txtComponent.DOColor(Color.white,duration))
                     .SetLoops(-1,LoopType.Yoyo);
    }

    #endregion

    #region Tension-Specific Animations

    // Pulsing effect based on tension level
    public void TensionPulse(Transform target, float curTension)
    {
        if(target == null) return;

        target.DOKill(); //Stop existing tween

        if (curTension >= GlobalStateManager.SOFT_THRESHOLD)
        {
            target.DOPunchScale(Vector3.one * tensionPulseStrength, tensionPulseDuration, 5, 0.5f)
                  .SetLoops(-1, LoopType.Restart);
        }    
        else if(curTension >= GlobalStateManager.HARD_THRESHOLD)
        {
           target.DOPunchScale(Vector3.one * tensionPulseStrength * 0.5f, tensionPulseDuration * 1.5f, 3, 0.5f)
                 .SetLoops(-1, LoopType.Restart);
        }
    }

    public void StopTensionPulse(Transform target)
    {
        if(target == null) return;
        target.DOKill();
        target.localScale = Vector3.one; //Reset scale
    }

    public void TensionBlink(Image iconImage, float curTension)
    {
        if (iconImage == null) return;

        iconImage.DOKill(); //Stop existing tween

        float blinkSpeed = 0.5f; // Default blink speed

        if (curTension >= GlobalStateManager.HARD_THRESHOLD) // >= 85
        {
            // Critical: Fast blinking with red tint
            blinkSpeed = 0.2f;
            Sequence blinkSeq = DOTween.Sequence();
            blinkSeq.Append(iconImage.DOFade(0.1f, blinkSpeed))
                    .Append(iconImage.DOFade(1f, blinkSpeed))
                    .SetLoops(-1, LoopType.Restart);
        }
        else if (curTension >= GlobalStateManager.SOFT_THRESHOLD) // >= 60
        {
            // Warning: Normal blinking with orange tint
            blinkSpeed = 0.4f;
            Sequence blinkSeq = DOTween.Sequence();
            blinkSeq.Append(iconImage.DOFade(0.5f, blinkSpeed))
                    .Append(iconImage.DOFade(1f, blinkSpeed))
                    .SetLoops(-1, LoopType.Restart);
        }
    }

    public void StopTensionBlink(Image iconImage)
    {
        if (iconImage == null) return;
        iconImage.DOKill();

        // Fade back to full opacity
        iconImage.DOFade(1f, 0.2f);
    }


    // Fill the tension bar to a target amount over a duration
    public void FillTensionBar(Slider tensionSlider, float targetFillAmount, float duration = 0.5f)
    {
        if (tensionSlider == null) return;
        
        tensionSlider.DOValue(targetFillAmount, duration)
                     .SetEase(Ease.OutQuad);
    }

    // Color transition for tension
    public void TransitionTensionColor(Image image, Color targetColor, float duration = 0.3f)
    {
        if (image == null) return;

        image.DOColor(targetColor, duration)
             .SetEase(Ease.OutQuad);
    }
    #endregion

    #region Background Animations
    //Crossfade between two background images
    public void CrossFadeBackgrounds(Image curBg, Image nextBg, System.Action onComplete, float duration = 1.0f)
    {
        if(curBg == null || nextBg == null) return;
        nextBg.color = new Color(1, 1, 1, 0); //Set next background to transparent
        nextBg.gameObject.SetActive(true);

        Sequence crossFade = DOTween.Sequence();
        crossFade.Append(curBg.DOFade(0, duration)) //Fade out current background
                 .Join(nextBg.DOFade(1, duration)) //Fade in next background
                 .OnComplete(() =>
                 {
                     curBg.gameObject.SetActive(false); //Deactivate current background
                     onComplete?.Invoke();
                 });
    }
    #endregion

    #region Button Animations
    //hover
    public void OnChoiceHover(Transform button)
    {
        BounceScale(button, 1.1f, 0.2f);
        PunchScale(button, 0.1f);
    }

    //click
    public void OnChoiceClick(Transform button, System.Action onComplete = null)
    {
        Sequence clickSequence = DOTween.Sequence();
        clickSequence.Append(button.DOScale(0.9f,0.1f))
                     .Append(button.DOScale(1.0f, 0.1f))
                     .Append(button.DOScale(1.1f,0.1f))
                     .OnComplete(() => onComplete?.Invoke());
    }

    //exit
    public void OnChoiceExit(Transform button)
    {
        ResetScale(button, 0.15f);
    }
    #endregion

    #region Utility
    //Kill tweens object
    public static void KillTween(Transform target)
    {
        if (target == null) return;
        target.DOKill();
    }

    //Pause all animations
    public static void PauseAllAnimations()
    {
        DOTween.PauseAll();
    }

    //Resume all animations
    public static void ResumeAllAnimations()
    {
        DOTween.PlayAll();
    }

    #endregion

}

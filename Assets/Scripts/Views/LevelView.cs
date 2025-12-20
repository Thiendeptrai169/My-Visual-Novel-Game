using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelView : MonoBehaviour
{
    [Header("UI Components")]
    public Slider tensionSlider;
    public Image tensionFillImage;
    public Transform tensionIcon;
    public Image tensionIconImage;
    public CanvasGroup dialogueCanvasGroup;
    public RectTransform dialoguePanel;
    public TextMeshProUGUI dialogueText;
    public Transform choicesContainer;
    public GameObject choiceButtonPrefab;

    [Header("Character Dialogue")]
    public GameObject characterDialoguePanel;
    public CanvasGroup characterCanvasGroup;
    public CharacterDialogueView characterView;

    [Header("Result UI")]
    public GameObject resultPanel;
    public CanvasGroup resultCanvasGroup;
    public TextMeshProUGUI resultTitle;
    public TextMeshProUGUI resultMessage;

    [Header("Ending Animation")]
    public float titleFadeInDuration = 0.5f;
    public float descriptionFadeInDelay = 0.8f;
    public float descriptionFadeInDuration = 0.5f;
    public float autoRestartDelay = 5f;

    [Header("Background Images")]
    public Image curBackground;
    public Image nextBackground;

    [Header("Choice Animation")]
    public float choiceStaggerDelay = 0.1f;

    private List<GameObject> curChoiceButtons = new List<GameObject>();
    private Coroutine curTypingCoroutine;
    private Coroutine endingCoroutine;
    
    // Character dialogue state
    private CharacterDialogueSO currentCharacterDialogue;
    private int currentLineIndex = 0;

    #region Tension UI

    public void UpdateTensionUI(float tensionRatio, float tension)
    {
        AnimationManager.instance.FillTensionBar(tensionSlider, tensionRatio, 0.5f);

        Color tensionColor = GetTensionColor(tension);
        AnimationManager.instance.TransitionTensionColor(tensionFillImage, tensionColor, 0.3f);

        if (tension >= GlobalStateManager.HARD_THRESHOLD)
        {
            tensionIconImage.gameObject.SetActive(true);
            AnimationManager.instance.TensionPulse(tensionIcon, tension);
            AnimationManager.instance.TensionBlink(tensionIconImage, tension);
            AnimationManager.instance.ScreenShake(0.2f, 0.3f);
        }
        else if (tension >= GlobalStateManager.SOFT_THRESHOLD)
        {
            tensionIconImage.gameObject.SetActive(true);
            AnimationManager.instance.TensionPulse(tensionIcon, tension);
            AnimationManager.instance.TensionBlink(tensionIconImage, tension);
        }
        else
        {
            AnimationManager.instance.StopTensionPulse(tensionIcon);
            AnimationManager.instance.StopTensionBlink(tensionIconImage);
            tensionIconImage.gameObject.SetActive(false);
        }
    }

    private Color GetTensionColor(float tension)
    {
        if (tension < 30f)
            return new Color(0.15f, 0.68f, 0.38f); // Green
        else if (tension < 60f)
            return new Color(0.95f, 0.61f, 0.07f); // Orange
        else if (tension < 85f)
            return new Color(0.91f, 0.30f, 0.24f); // Red
        else
            return new Color(0.56f, 0.27f, 0.68f); // Purple
    }

    #endregion

    #region Dialogue Display

    public void ShowDialouge(string text, List<DialougeChoice> choices, System.Action onTypingComplete = null)
    {
        if (curTypingCoroutine != null)
        {
            StopCoroutine(curTypingCoroutine);
        }

        dialoguePanel.gameObject.SetActive(true);

        AnimationManager.instance.FadeIn(dialogueCanvasGroup, 0.3f);
        AnimationManager.instance.SlideInFromBottom(dialoguePanel, 0.4f);

        curTypingCoroutine = StartCoroutine(
            AnimationManager.instance.TypeText(dialogueText, text, 1f, () =>
            {
                onTypingComplete?.Invoke();
                ShowChoices();
            })
        );
    }

    private void ShowChoices()
    {
        for (int i = 0; i < curChoiceButtons.Count; i++)
        {
            RectTransform choiceRect = curChoiceButtons[i].GetComponent<RectTransform>();
            float delay = i * choiceStaggerDelay;

            AnimationManager.instance.SlideInFromRight(choiceRect, delay);
        }
    }

    public void OnOffTheDialogue(bool isActive)
    {
        if (isActive)
        {
            dialoguePanel.gameObject.SetActive(true);
            dialogueText.gameObject.SetActive(true);
            choicesContainer.gameObject.SetActive(true);
            tensionSlider.gameObject.SetActive(true);

            // Fade in
            AnimationManager.instance.FadeIn(dialogueCanvasGroup, 0.3f);
        }
        else
        {
            // Fade out then hide
            AnimationManager.instance.FadeOut(dialogueCanvasGroup, 0.3f, () =>
            {
                dialoguePanel.gameObject.SetActive(false);
            });
        }
    }

    public void SetDialougeText(string text)
    {
        dialogueText.text = text;
    }

    #endregion

    #region Character Dialogue

    public void ShowCharacterDialogue(CharacterDialogueSO dialogueData, System.Action onComplete)
    {
        // Hide main dialogue UI
        OnOffTheDialogue(false);

        // Show character dialogue panel
        characterDialoguePanel.SetActive(true);
        AnimationManager.instance.FadeIn(characterCanvasGroup, 0.3f);

        // Start playing dialogue lines
        currentCharacterDialogue = dialogueData;
        currentLineIndex = 0;

        PlayNextCharacterLine(onComplete);
    }

    private void PlayNextCharacterLine(System.Action onComplete)
    {
        if (currentLineIndex >= currentCharacterDialogue.dialogueLines.Count)
        {
            // All lines played, hide character dialogue
            HideCharacterDialogue(() =>
            {
                onComplete?.Invoke();
            });
            return;
        }

        DialogueLine line = currentCharacterDialogue.dialogueLines[currentLineIndex];
        currentLineIndex++;

        // Show this line using helper view
        characterView.ShowLine(line, () =>
        {
            // Line complete, play next
            PlayNextCharacterLine(onComplete);
        });
    }

    private void HideCharacterDialogue(System.Action onComplete)
    {
        AnimationManager.instance.FadeOut(characterCanvasGroup, 0.3f, () =>
        {
            characterDialoguePanel.SetActive(false);
            characterView.ClearCharacters();

            // Return to main dialogue UI
            OnOffTheDialogue(true);
            onComplete?.Invoke();
        });
    }

    // Handle input for character dialogue
    public void OnCharacterDialogueContinue()
    {
        characterView.OnContinueClicked();
    }

    #endregion

    #region Choice Buttons

    public void ClearChoices()
    {
        foreach (Transform child in choicesContainer)
        {
            RectTransform rect = child.GetComponent<RectTransform>();
            AnimationManager.instance.SlideInFromLeft(rect, 0.2f, () =>
            {
                Destroy(child.gameObject);
            });
        }
        curChoiceButtons.Clear();
        // Force rebuild layout after clearing
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(choicesContainer as RectTransform);

    }
   
    public void CreateChoiceButton(DialougeChoice choice, System.Action<DialougeChoice> onClickCallback)
    {
        GameObject btnObj = Instantiate(choiceButtonPrefab, choicesContainer);
        curChoiceButtons.Add(btnObj);

        btnObj.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceText;
        btnObj.GetComponentInChildren<TextMeshProUGUI>().color = new Color32(255, 255, 255, 255);

        Button button = btnObj.GetComponent<Button>();
        if (button == null)
            button = btnObj.AddComponent<Button>();

        ChoiceButtonAnimator buttonAnim = btnObj.GetComponent<ChoiceButtonAnimator>();
        if (buttonAnim == null)
            buttonAnim = btnObj.AddComponent<ChoiceButtonAnimator>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            AnimationManager.instance.OnChoiceClick(btnObj.transform, () => onClickCallback?.Invoke(choice));
        });
    }

    public void PrepareChoiceButtonsForAnimation()
    {
        StartCoroutine(HideButtonsAfterLayout());
    }

    private System.Collections.IEnumerator HideButtonsAfterLayout()
    {
        // Force layout update
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(choicesContainer as RectTransform);

        // Wait 1 frame for layout to fully apply
        yield return null;

        // Now hide all buttons for animation
        foreach (GameObject btnObj in curChoiceButtons)
        {
            RectTransform rect = btnObj.GetComponent<RectTransform>();
            Vector2 hiddenPos = rect.anchoredPosition;
            hiddenPos.x += 800f;
            rect.anchoredPosition = hiddenPos;
        }
    }

    #endregion

    #region Result Screen - Ending Display

    /// <summary>
    /// Show ending with animation (title → description → auto restart)
    /// </summary>
    public void ShowEnding(DialougeSO endingNode, System.Action onRestartCallback)
    {
        if (endingNode == null || !endingNode.isEndingNode)
        {
            Debug.LogError("[LevelView] Invalid ending node!");
            return;
        }

        if (endingCoroutine != null)
            StopCoroutine(endingCoroutine);

        endingCoroutine = StartCoroutine(ShowEndingCoroutine(endingNode, onRestartCallback));
    }

    private System.Collections.IEnumerator ShowEndingCoroutine(DialougeSO endingNode, System.Action onRestartCallback)
    {
        Debug.Log("[LevelView] Starting ending coroutine...");
        
        // Setup panel
        resultPanel.SetActive(true);
        resultCanvasGroup.alpha = 0;

        // Format title & description
        string endingType = endingNode.isGoodEnding ? "GOOD ENDING" : "BAD ENDING";
        resultTitle.text = $"<b>{endingType}: {endingNode.endingTitle}</b>";
        resultTitle.fontSize = 48;
        resultTitle.color = endingNode.isGoodEnding 
            ? new Color(0.2f, 0.8f, 0.3f)  
            : new Color(0.9f, 0.3f, 0.2f);  

        resultMessage.text = endingNode.npcText;
        resultMessage.fontSize = 28;
        resultMessage.fontStyle = FontStyles.Normal;

        // Hide text initially
        resultTitle.canvasRenderer.SetAlpha(0f);
        resultMessage.canvasRenderer.SetAlpha(0f);

        // STEP 1: Fade in panel
        Debug.Log("[LevelView] Step 1: Fading in panel...");
        float timer = 0f;
        while (timer < titleFadeInDuration)
        {
            resultCanvasGroup.alpha = Mathf.Lerp(0, 1, timer / titleFadeInDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        resultCanvasGroup.alpha = 1f;

        // STEP 2: Fade in title
        Debug.Log("[LevelView] Step 2: Fading in title...");
        resultTitle.CrossFadeAlpha(1f, titleFadeInDuration, false);
        yield return new WaitForSeconds(titleFadeInDuration);

        // STEP 3: Wait, then fade in description
        Debug.Log("[LevelView] Step 3: Waiting before description...");
        yield return new WaitForSeconds(descriptionFadeInDelay);
        
        Debug.Log("[LevelView] Step 4: Fading in description...");
        resultMessage.CrossFadeAlpha(1f, descriptionFadeInDuration, false);
        yield return new WaitForSeconds(descriptionFadeInDuration);

        // STEP 4: Wait for auto restart
        Debug.Log($"[LevelView] Step 5: Waiting {autoRestartDelay}s before restart...");
        yield return new WaitForSeconds(autoRestartDelay);

        //STEP 5: Call restart callback
        Debug.Log("[LevelView] ✅ Calling restart callback...");
        
        if (onRestartCallback != null)
        {
            onRestartCallback.Invoke();
        }
        else
        {
            Debug.LogError("[LevelView] Restart callback is NULL!");
        }
    }

    public void HideResult()
    {
        resultPanel.SetActive(false);
        if (endingCoroutine != null)
        {
            StopCoroutine(endingCoroutine);
            endingCoroutine = null;
        }
    }

    #endregion

    #region Background Management

    public void ChangeBackground(Sprite newBackground, float duration = 1f)
    {
        if (curBackground == null || nextBackground == null) return;

        nextBackground.sprite = newBackground;
        AnimationManager.instance.CrossFadeBackgrounds(curBackground, nextBackground, () =>
        {
            Image temp = curBackground;
            curBackground = nextBackground;
            nextBackground = temp;
        }, duration);
    }

    #endregion

    #region Animation Feedback

    public void PlayChoiceSelectionFeedback()
    {
        // Punch scale dialogue panel
        AnimationManager.instance.PunchScale(dialoguePanel.transform, 0.05f);
    }

    public void DelayedAction(float delay, System.Action action)
    {
        StartCoroutine(DelayedActionCoroutine(delay, action));
    }

    private System.Collections.IEnumerator DelayedActionCoroutine(float delay, System.Action action)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    public void FadeOutAndReload(System.Action onComplete)
    {
        Debug.Log("[LevelView] Starting fade out for reload...");
        
        //  Ensure callback is always called
        if (AnimationManager.instance != null)
        {
            AnimationManager.instance.FadeOut(dialogueCanvasGroup, 0.5f, () =>
            {
                Debug.Log("[LevelView] Fade out complete, calling callback...");
                onComplete?.Invoke();
            });
        }
        else
        {
            Debug.LogError("[LevelView] AnimationManager is NULL!");
            // Fallback: call immediately
            onComplete?.Invoke();
        }
    }

    public void FadeOutAndLoadNext(System.Action onComplete)
    {
        AnimationManager.instance.FadeOut(dialogueCanvasGroup, 0.5f, () =>
        {
            onComplete?.Invoke();
        });
    }

    #endregion

    #region Utility

    public void SkipTyping()
    {
        // Stop current typing coroutine
        if (curTypingCoroutine != null)
        {
            StopCoroutine(curTypingCoroutine);
            curTypingCoroutine = null;
        }

        // Call AnimationManager's skip function
        AnimationManager.instance.SkipTyping();
    }

    private void OnDestroy()
    {
        if (curTypingCoroutine != null)
            StopCoroutine(curTypingCoroutine);

        if (endingCoroutine != null)
            StopCoroutine(endingCoroutine);

        AnimationManager.KillTween(transform);
    }

    #endregion
}

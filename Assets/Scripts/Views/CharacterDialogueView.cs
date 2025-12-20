using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// Helper class for character dialogue display
/// Controlled by LevelView
/// </summary>
public class CharacterDialogueView : MonoBehaviour
{
    [Header("Character Sprites")]
    [SerializeField] private Image leftCharacter;
    [SerializeField] private Image centerCharacter;
    [SerializeField] private Image rightCharacter;
    
    [Header("Dialogue Box")]
    [SerializeField] private TextMeshProUGUI speakerNameText;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject continueIndicator;

    private Coroutine currentLineCoroutine;
    private bool waitingForInput = false;


    /// Show a dialogue line
    public void ShowLine(DialogueLine line, System.Action onLineComplete)
    {
        if (currentLineCoroutine != null)
            StopCoroutine(currentLineCoroutine);
            
        currentLineCoroutine = StartCoroutine(DisplayLineCoroutine(line, onLineComplete));
    }

    private IEnumerator DisplayLineCoroutine(DialogueLine line, System.Action onComplete)
    {
        // Update character sprite
        UpdateCharacterSprite(line);
        
        // Update speaker name
        speakerNameText.text = line.speakerName;
        
        // Use AnimationManager's TypeText
        float duration = line.dialogueText.Length * 0.03f;
        yield return StartCoroutine(
            AnimationManager.instance.TypeText(dialogueText, line.dialogueText, duration)
        );
        
        // Show continue indicator and wait for input
        continueIndicator.SetActive(true);
        AnimateContinueIndicator();
        waitingForInput = true;
        
        yield return new WaitUntil(() => !waitingForInput);
        
        continueIndicator.SetActive(false);
        onComplete?.Invoke();
    }

    private void UpdateCharacterSprite(DialogueLine line)
    {
        // Hide all first
        HideAllCharacters();
        
        // Show the active character
        if (line.position != CharacterPosition.Hidden && line.characterImage != null)
        {
            Image targetImage = GetCharacterImageByPosition(line.position);
            if (targetImage != null)
            {
                targetImage.sprite = line.characterImage;
                targetImage.gameObject.SetActive(true);
                
                // Fade in character using AnimationManager
                AnimationManager.instance.FadeInImage(targetImage, 0.2f);
            }
        }
    }

    private void AnimateContinueIndicator()
    {
        // Bounce animation using AnimationManager
        if (continueIndicator != null)
        {
            AnimationManager.instance.BounceScale(continueIndicator.transform, 1.2f, 0.5f);
        }
    }

    private Image GetCharacterImageByPosition(CharacterPosition position)
    {
        switch (position)
        {
            case CharacterPosition.Left: return leftCharacter;
            case CharacterPosition.Center: return centerCharacter;
            case CharacterPosition.Right: return rightCharacter;
            default: return null;
        }
    }

    private void HideAllCharacters()
    {
        if (leftCharacter != null) leftCharacter.gameObject.SetActive(false);
        if (centerCharacter != null) centerCharacter.gameObject.SetActive(false);
        if (rightCharacter != null) rightCharacter.gameObject.SetActive(false);
    }

    /// <summary>
    /// Handle continue input - uses AnimationManager's skip system
    /// </summary>
    public void OnContinueClicked()
    {
        // If typing, skip it (AnimationManager handles this)
        AnimationManager.instance.SkipTyping();
        
        // If waiting for input, proceed to next line
        if (waitingForInput)
        {
            waitingForInput = false;
        }
    }

    public void ClearCharacters()
    {
        HideAllCharacters();
        speakerNameText.text = "";
        dialogueText.text = "";
        
        if (continueIndicator != null)
            continueIndicator.SetActive(false);
    }

    private void OnDestroy()
    {
        // Clean up coroutine
        if (currentLineCoroutine != null)
            StopCoroutine(currentLineCoroutine);
    }
}

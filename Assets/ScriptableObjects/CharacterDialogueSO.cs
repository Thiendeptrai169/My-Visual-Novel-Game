using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ScriptableObject for character dialogue scenes
/// Each dialogue line has speaker, text, and character sprite
/// </summary>
[CreateAssetMenu(fileName = "CharacterDialogue", menuName = "SpeakUp!/Character Dialogue", order = 2)]
public class CharacterDialogueSO : ScriptableObject
{
    [Header("Dialogue Info")]
    public string sceneId; // e.g., "S0_Intro", "S2_Confrontation"

    [Header("Dialogue Lines")]
    public List<DialogueLine> dialogueLines = new List<DialogueLine>();

    [Header("After Dialogue")]
    public DialougeSO nextChoiceNode; // Return to choice system after this dialogue
    public bool autoAdvance = true; // Auto advance or wait for click
}

[System.Serializable]
public class DialogueLine
{
    [Header("Speaker")]
    public string speakerName; // "Hùng", "Nam", etc.
    public CharacterSprite characterSprite; // Which character image to show
    public CharacterPosition position; // Left, Center, Right

    [Header("Dialogue")]
    [TextArea(2, 4)]
    public string dialogueText;

    [Header("Expression")]
    public Sprite characterImage; // Character sprite with expression
    public CharacterExpression expression; // Happy, Angry, Sad, Neutral, etc.

    
}

public enum CharacterSprite
{
    None,
    Player,
    Hung,
    Nam,
    Linh,
    Teacher
}

public enum CharacterPosition
{
    Left,
    Center,
    Right,
    Hidden
}

public enum CharacterExpression
{
    Neutral,
    Happy,
    Sad,
    Angry,
    Worried
}
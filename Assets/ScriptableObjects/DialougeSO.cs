using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Dialouge", menuName = "SpeakUp/Dialouge Data")]
public class DialougeSO : ScriptableObject
{
    public string nodeId = "S0";

    [TextArea(3, 10)]
    public string npcText;

    public List<DialougeChoice> choices;

    public CharacterDialogueSO characterDialogue;

    // Ending metadata
    [Header("Ending Info (Only for E0-E8 nodes)")]
    public bool isEndingNode = false;
    public string endingTitle = ""; // e.g., "Bỏ đi", "Im lặng làm nhân chứng"
    public bool isGoodEnding = false;
}

[System.Serializable]
public class DialougeChoice
{
    [TextArea(1, 3)]
    public string choiceText;
    public float tensionImpact;
    public List<StateEffect> stateEffects; //effects to apply when this choice is selected
    public DialougeSO nextNode;
    public bool useConditionalBranching = false; 
    public List<ConditionalBranch> conditionalBranches;
    [Header("Visual")]
    public string animationTrigger;
    
}

[System.Serializable]
public class StateEffect
{
    public enum EffectType
    { 
        SetBool,
        ModifyInt,
        ModifyFloat
    }

    public EffectType effectType; 

    public string variableName; //must match the variable name in GlobalStateManager
    public bool boolValue;
    public int intValue;
    public float floatValue;
}

[System.Serializable]
public class ConditionalBranch
{
    [Header("Condition (left side)")]
    public string conditionVariable; // example "tension"

    public enum ComparisonType
    {
        GreaterThan,
        LessThan,
        EqualTo
    }
    public ComparisonType comparison;

    [Header("Right side")]
    public bool compareWithVariable = false; // if true, use different variable; if false, use comparisonValue
    public string rightHandVariable;        
    public float comparisonValue;            // fallback if compareWithVariable = false

    [Header("If True, Go To")]
    public DialougeSO targetNode;
}
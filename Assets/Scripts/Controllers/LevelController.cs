using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public class LevelController : MonoBehaviour
{
    public LevelModel model;
    public LevelView view;
    [SerializeField]
    public DialougeSO startingDialouge;

    private DialougeSO curDialouge;
    private bool isProcessingChoice = false;

    private void Awake()
    {
        
        if (GameStateManager.instance == null)
        {
            Debug.LogError("[LevelController] GameStateManager instance not found!");
            return;
        }

        //  Always unsubscribe first to prevent duplicates
        GameStateManager.instance.onStateChange -= HandleStateChange;
        
        // Then subscribe
        GameStateManager.instance.onStateChange += HandleStateChange;
        
        Debug.Log("[LevelController] ✅ Subscribed to GameStateManager events");
        Debug.Log($"[LevelController] Current GameState: {GameStateManager.instance.curState}");
    }
    
    private void Start()
    {
        
        model.SyncWithGlobalState();
        
        Debug.Log($"[LevelController] 🎮 Game starting. State: {GameStateManager.instance.curState}");
        
        // Force state to Dialogue
        if (GameStateManager.instance.curState != GameState.Dialogue)
        {
            Debug.Log("[LevelController] ⚠️ Forcing state to Dialogue...");
            GameStateManager.instance.ChangeState(GameState.Dialogue);
        }
        
        // ✅ Start tension ambient system (Level1 has its own SoundManager)
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayGameplayBGM(); // Only starts ambient
            SoundManager.instance.UpdateTensionAmbient(model._curTension, model._maxTension);
        }
        else
        {
            Debug.LogError("[LevelController] ❌ SoundManager is NULL!");
        }

        // Set initial background
        if (BackgroundManager.instance != null && startingDialouge != null)
        {
            Sprite initialBg = BackgroundManager.instance.GetBackgroundForNode(startingDialouge.nodeId);
            Debug.Log($"[LevelController] Setting initial background for node: {startingDialouge.nodeId}");
            view.ChangeBackground(initialBg, 0f);
        }

        // Init view
        Debug.Log($"[LevelController]Initial Tension: {model._curTension}/{model._maxTension}");
        view.UpdateTensionUI(model.tensionRatio, model._curTension);

        if (startingDialouge != null)
        {
            Debug.Log($"[LevelController] Starting dialogue: {startingDialouge.nodeId}");
            StartDialouge(startingDialouge);
        }
        else
        {
            Debug.LogError("[LevelController] Starting dialogue not set!");
        }
        
    }
    
    private void OnDestroy()
    {
        Debug.Log("[LevelController] OnDestroy called - cleaning up...");
        
        if (GameStateManager.instance != null)
        {
            GameStateManager.instance.onStateChange -= HandleStateChange;
            Debug.Log("[LevelController] Unsubscribed from GameStateManager");
        }
        
        StopAllCoroutines();
    }

    private void HandleStateChange(GameState newState)
    {   
        Debug.Log($"[LevelController] HandleStateChange: {newState}");
        
        switch (newState)
        {
            case GameState.Dialogue:
                view.OnOffTheDialogue(true);
                DisplayCurrentDialogue();
                break;
                
            case GameState.Ending:
                //  Hide dialogue UI when entering ending
                view.OnOffTheDialogue(false);
                break;
                
            case GameState.Paused:
                // TODO: Show pause menu
                Debug.Log("[LevelController] Game paused");
                break;
        }
    }

    public void StartDialouge(DialougeSO dialougeData)
    {
        curDialouge = dialougeData;

        if (GlobalStateManager.instance != null)
        {
            GlobalStateManager.instance.currentNodeId = dialougeData.nodeId;
        }

        GameStateManager.instance.ChangeState(GameState.Dialogue);
    }

    private void DisplayCurrentDialogue()
    {
        if (curDialouge == null)
        {
            EndDialouge();
            return;
        }

        view.ClearChoices();

        view.ShowDialouge(curDialouge.npcText, curDialouge.choices, () =>
        {
            CreateChoiceButtons();
        });
    }

    private void CreateChoiceButtons()
    {
        if(curDialouge == null || curDialouge.choices == null) return;
        
        foreach(var choice in curDialouge.choices)
        {
            view.CreateChoiceButton(choice, OnChoiceSelected);
        }
        
        view.PrepareChoiceButtonsForAnimation();
    }

    private void OnChoiceSelected(DialougeChoice choice)
    {
        if (isProcessingChoice) return;
        isProcessingChoice = true;

        // Play choice selection sound
        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayChoiceSelect();
        }

        // Play feedback animation/sound
        view.PlayChoiceSelectionFeedback();

        // 1. Apply state effects
        if (choice.stateEffects != null && choice.stateEffects.Count > 0)
        {
            foreach (var effect in choice.stateEffects)
            {
                ApplyStateEffect(effect);
            }
        }

        // 2. Calculate and apply tension
        float oldTension = model._curTension;
        float finalTensionImpact = GetTensionImpact(curDialouge.nodeId, choice.choiceText, choice.tensionImpact);
        
        GlobalStateManager.instance.ModifyTension(finalTensionImpact);
        model.SyncWithGlobalState();
        
        view.UpdateTensionUI(model.tensionRatio, model._curTension);

        // Update ambient
        if (SoundManager.instance != null)
        {
            Debug.Log($"[LevelController] Calling UpdateTensionAmbient({model._curTension:F1}, {model._maxTension})");
            SoundManager.instance.UpdateTensionAmbient(model._curTension, model._maxTension);
        }

        // 3. Check for ending conditions
        if (GlobalStateManager.instance != null && GlobalStateManager.instance.ShouldTriggerEnding(out string endingId))
        {
            GlobalStateManager.instance.currentEndingId = endingId;
            HandleEnding(endingId);
            isProcessingChoice = false;
            return;
        }

        // 4. Determine next dialogue
        DialougeSO nextDialouge = null;

        if (choice.useConditionalBranching && choice.conditionalBranches != null && choice.conditionalBranches.Count > 0)
        {
            nextDialouge = EvaluationConditionalBranches(choice.conditionalBranches);
        }

        if (nextDialouge == null)
            nextDialouge = choice.nextNode;

        // 5. Handle next dialogue
        if (nextDialouge != null)
        {
            // Check if ending node
            if (nextDialouge.choices == null || nextDialouge.choices.Count == 0)
            {
                HandleEnding(nextDialouge.nodeId);
                isProcessingChoice = false;
                return;
            }

            // Check background change
            if (BackgroundManager.instance != null && 
                BackgroundManager.instance.ShouldChangeBackground(curDialouge.nodeId, nextDialouge.nodeId))
            {
                Sprite newBg = BackgroundManager.instance.GetBackgroundForNode(nextDialouge.nodeId);
                float duration = BackgroundManager.instance.GetTransitionDuration();
                view.ChangeBackground(newBg, duration);
            }

            // Check character dialogue
            if (nextDialouge.characterDialogue != null)
            {
                view.DelayedAction(0.3f, () =>
                {
                    view.ShowCharacterDialogue(nextDialouge.characterDialogue, () =>
                    {
                        curDialouge = nextDialouge;
                        DisplayCurrentDialogue();
                        isProcessingChoice = false;
                    });
                });
            }
            else
            {
                curDialouge = nextDialouge;
                view.DelayedAction(0.3f, () =>
                {
                    DisplayCurrentDialogue();
                    isProcessingChoice = false;
                });
            }
        }
        else
        {
            // No next dialogue found, check default ending
            if (model.IsGameOver())
            {
                HandleEnding("E4");
            }
            else
            {
                HandleEnding("E2");
            }
            isProcessingChoice = false;
        }
    }

    private float GetTensionImpact(string nodeId, string choiceText, float defaultImpact)
    {
        // Special recording sound effects
        if (choiceText.Contains("quay") || choiceText.Contains("camera") || choiceText.Contains("điện thoại"))
        {
            if (SoundManager.instance != null)
            {
                SoundManager.instance.PlayRecordingStart();
            }
        }

        // Special case: S10
        if (nodeId == "L1_S10" && choiceText.Contains("Bênh Nam"))
        {
            if (GlobalStateManager.instance.deEscalationSkill > 2)
            {
                Debug.Log("[LevelController] High de-escalation skill: tension -5");
                return -5f;
            }
            else
            {
                Debug.Log("[LevelController] Low de-escalation skill: tension +5");
                return 5f;
            }
        }

        // Special case: S11
        if (nodeId == "L1_S11")
        {
            GlobalStateManager.instance.teacherArrived = true;
        }

        return defaultImpact;
    }

    private void ApplyStateEffect(StateEffect effect)
    {
        if (GlobalStateManager.instance == null) return;

        var field = typeof(GlobalStateManager).GetField(
            effect.variableName,
            BindingFlags.Public | BindingFlags.Instance);

        if (field == null)
        {
            Debug.LogWarning($"[LevelController] Field '{effect.variableName}' not found in GlobalStateManager!");
            return;
        }

        switch (effect.effectType)
        {
            case StateEffect.EffectType.SetBool:
                if (field.FieldType == typeof(bool))
                {
                    field.SetValue(GlobalStateManager.instance, effect.boolValue);
                }
                break;
            case StateEffect.EffectType.ModifyInt:
                if (field.FieldType == typeof(int))
                {
                    int currentValue = (int)field.GetValue(GlobalStateManager.instance);
                    int newValue = currentValue + effect.intValue;
                    field.SetValue(GlobalStateManager.instance, newValue);
                }
                break;
            case StateEffect.EffectType.ModifyFloat:
                if (field.FieldType == typeof(float))
                {
                    float currentValue = (float)field.GetValue(GlobalStateManager.instance);
                    float newValue = currentValue + effect.floatValue;
                    field.SetValue(GlobalStateManager.instance, newValue);
                }
                break;
        }
    }

    private float? GetGlobalStateValueAsFloat(string variableName)
    {
        if (string.IsNullOrEmpty(variableName))
            return null;

        var type = typeof(GlobalStateManager);
        var flags = BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.Instance | BindingFlags.Static;

        var field = type.GetField(variableName, flags);
        if (field == null)
        {
            return null;
        }

        object owner = null;
        if (!field.IsStatic)
        {
            if (GlobalStateManager.instance == null)
            {
                return null;
            }
            owner = GlobalStateManager.instance;
        }

        object raw = field.GetValue(owner);

        if (field.FieldType == typeof(int))
            return (int)raw;
        if (field.FieldType == typeof(float))
            return (float)raw;
        if (field.FieldType == typeof(bool))
            return ((bool)raw) ? 1f : 0f;

        return null;
    }

    private DialougeSO EvaluationConditionalBranches(List<ConditionalBranch> branches)
    {
        if (branches == null || branches.Count == 0)
            return null;

        if (GlobalStateManager.instance == null)
        {
            return null;
        }

        foreach (var branch in branches)
        {
            if (branch == null) continue;

            float? left = GetGlobalStateValueAsFloat(branch.conditionVariable);
            if (!left.HasValue) continue;

            float right;
            if (branch.compareWithVariable && !string.IsNullOrEmpty(branch.rightHandVariable))
            {
                float? rightValue = GetGlobalStateValueAsFloat(branch.rightHandVariable);
                if (!rightValue.HasValue) continue;
                right = rightValue.Value;
            }
            else
            {
                right = branch.comparisonValue;
            }

            bool conditionMet = false;
            switch (branch.comparison)
            {
                case ConditionalBranch.ComparisonType.GreaterThan:
                    conditionMet = left.Value > right;
                    break;
                case ConditionalBranch.ComparisonType.LessThan:
                    conditionMet = left.Value < right;
                    break;
                case ConditionalBranch.ComparisonType.EqualTo:
                    conditionMet = Mathf.Approximately(left.Value, right);
                    break;
            }

            if (conditionMet)
            {
                return branch.targetNode;
            }
        }

        return null;
    }

    /// <summary>
    /// ✅ Handle ending - Show ending screen with buttons (no auto-restart)
    /// </summary>
    private void HandleEnding(string endingId)
    {
        Debug.Log($"[LevelController] Handling ending: {endingId}");
        
        // Change to Ending state
        GameStateManager.instance.ChangeState(GameState.Ending);
        
        // Stop tension ambient
        if (SoundManager.instance != null)
        {
            SoundManager.instance.StopTensionAmbient(true);
        }

        // Load ending node
        DialougeSO endingNode = LoadEndingNode(endingId);

        if (endingNode == null)
        {
            Debug.LogError($"[LevelController] ❌ Ending node {endingId} not found! Forcing return to menu...");
            ReturnToMainMenu();
            return;
        }

        // Play appropriate music
        if (SoundManager.instance != null)
        {
            if (endingNode.isGoodEnding)
                SoundManager.instance.PlayGoodEnding();
            else
                SoundManager.instance.PlayBadEnding();
        }

        // Update global state
        if (GlobalStateManager.instance != null)
        {
            GlobalStateManager.instance.lastEndingWasGood = endingNode.isGoodEnding;
            GlobalStateManager.instance.currentEndingId = endingId;
        }

        // ✅ Show ending screen WITHOUT auto-restart callback
        // Buttons will handle return to menu via EndingButtonsHandler
        view.ShowEnding(endingNode, null);
    }

    /// <summary>
    /// ✅ Load ending node from Resources
    /// </summary>
    private DialougeSO LoadEndingNode(string endingId)
    {
        // Try direct load first (fastest)
        string directPath = $"DialogueAssets/Level1/{endingId}";
        DialougeSO directLoad = Resources.Load<DialougeSO>(directPath);
        
        if (directLoad != null)
        {
            Debug.Log($"[LevelController] Found ending node: {directLoad.name}");
            return directLoad;
        }

        // Fallback: Search all nodes in folder
        DialougeSO[] allNodes = Resources.LoadAll<DialougeSO>("DialogueAssets/Level1");
        
        foreach (var node in allNodes)
        {
            if (node.nodeId == endingId)
            {
                //Debug.Log($"[LevelController] ✅ Found ending node via search: {node.name}");
                return node;
            }
        }

        Debug.LogError($"[LevelController] ❌ Ending node {endingId} not found in Resources!");
        return null;
    }

    /// <summary>
    /// ✅ RENAMED: RestartLevel → ReturnToMainMenu (for clarity)
    /// Called by EndingButtonsHandler when user clicks "Return to Menu"
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("[LevelController] 🔄 Returning to Main Menu...");
        
        // Stop all audio in this scene
        if (SoundManager.instance != null)
        {
            SoundManager.instance.StopAll();
            Debug.Log("[LevelController] Stopped all audio");
        }

        // Reset global state
        if (GlobalStateManager.instance != null)
        {
            GlobalStateManager.instance.ResetState();
            Debug.Log("[LevelController] Reset global state");
        }

        // Reset game state
        if (GameStateManager.instance != null)
        {
            GameStateManager.instance.ChangeState(GameState.MenuGameplay);
            Debug.Log("[LevelController] Changed to MenuGameplay state");
        }

        // Load Main Menu
        Debug.Log("[LevelController] Loading MainMenu scene...");
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    ///  End dialogue - Trigger game state change
    /// </summary>
    private void EndDialouge()
    {
        if (model.IsGameOver())
        {
            GameStateManager.instance.ChangeState(GameState.GameOver);
        }
        else
        {
            GameStateManager.instance.ChangeState(GameState.LevelComplete);
        }
    }

    /// <summary>
    /// Handle input for character dialogue
    /// </summary>
    private void Update()
    {
        if (view == null) return;

        // Character dialogue input
        if (view.characterDialoguePanel != null && view.characterDialoguePanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
            {
                view.OnCharacterDialogueContinue();
            }
        }
        // Skip typing in normal dialogue   
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            view.SkipTyping();
        }
    }
}

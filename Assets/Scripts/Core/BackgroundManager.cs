using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Manages background changes based on dialogue nodes
/// </summary>
public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager instance;

    [Header("Background Layer References")]
    [SerializeField] private Image currentBackgroundImage;
    [SerializeField] private Image nextBackgroundImage;

    [Header("Background Sprites")]
    [SerializeField] private Sprite background1; // Hallway
    [SerializeField] private Sprite background2; // Hidden spot
    [SerializeField] private Sprite background3; // Confrontation area

    [Header("Settings")]
    [SerializeField] private float transitionDuration = 1f;

    // Mapping of node IDs to backgrounds
    private Dictionary<string, Sprite> nodeBackgroundMap;
    private Sprite currentSprite;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitializeBackgroundMap();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeBackgroundMap()
    {
        nodeBackgroundMap = new Dictionary<string, Sprite>();

        // BG1 nodes: Main hallway, calm interactions
        string[] bg1Nodes = { "S0", "S2", "S4", "S5", "S7", "S9", "S12", "S13", "S16", 
                              "S19", "S20" };
        foreach (string node in bg1Nodes)
        {
            nodeBackgroundMap[node] = background1;
        }

        // BG2 nodes: Hidden observation spots
        string[] bg2Nodes = { "S1", "S3", "S8" };
        foreach (string node in bg2Nodes)
        {
            nodeBackgroundMap[node] = background2;
        }

        // BG3 nodes: Direct confrontation, high tension
        string[] bg3Nodes = { "S6", "S10", "S11", "S14", "S15", "S17", "S18", "S21" };
        foreach (string node in bg3Nodes)
        {
            nodeBackgroundMap[node] = background3;
        }

        Debug.Log($"[BackgroundManager] ✅ Initialized with {nodeBackgroundMap.Count} node mappings");

        // Set initial background
        if (currentBackgroundImage != null && background1 != null)
        {
            currentBackgroundImage.sprite = background1;
            currentSprite = background1;
            Debug.Log("[BackgroundManager] 🖼️ Set initial background to BG1");
        }
    }

    /// <summary>
    /// Get background sprite for a specific node
    /// </summary>
    public Sprite GetBackgroundForNode(string nodeId)
    {
        if (nodeBackgroundMap.TryGetValue(nodeId, out Sprite bg))
        {
            return bg;
        }

        Debug.LogWarning($"[BackgroundManager] ⚠️ No background mapping for node: {nodeId}, using BG1 as fallback");
        return background1; // Default fallback
    }

    /// <summary>
    /// Check if background should change for next node
    /// </summary>
    public bool ShouldChangeBackground(string currentNodeId, string nextNodeId)
    {
        Sprite currentBg = GetBackgroundForNode(currentNodeId);
        Sprite nextBg = GetBackgroundForNode(nextNodeId);

        return currentBg != nextBg;
    }

    /// <summary>
    /// Change background with crossfade animation
    /// ONLY changes sprite - does NOT modify scale/position!
    /// </summary>
    public void ChangeBackground(Sprite newBackground, float duration = -1f)
    {
        if (newBackground == null)
        {
            Debug.LogWarning("[BackgroundManager] ⚠️ New background sprite is null!");
            return;
        }

        if (currentBackgroundImage == null || nextBackgroundImage == null)
        {
            Debug.LogError("[BackgroundManager] ❌ Background Image references not assigned!");
            return;
        }

        // If same sprite, skip
        if (currentSprite == newBackground)
        {
            Debug.Log($"[BackgroundManager] ⏭️ Already showing {newBackground.name}, skipping");
            return;
        }

        float actualDuration = duration < 0 ? transitionDuration : duration;

        Debug.Log($"[BackgroundManager] 🖼️ Crossfading: {currentSprite?.name} → {newBackground.name} ({actualDuration}s)");

        // Set next background sprite (keep scale/position unchanged!)
        nextBackgroundImage.sprite = newBackground;

        // Use AnimationManager for crossfade
        if (AnimationManager.instance != null)
        {
            AnimationManager.instance.CrossFadeBackgrounds(
                currentBackgroundImage,
                nextBackgroundImage,
                () =>
                {
                    // Swap references after fade complete
                    Image temp = currentBackgroundImage;
                    currentBackgroundImage = nextBackgroundImage;
                    nextBackgroundImage = temp;

                    currentSprite = newBackground;

                    Debug.Log("[BackgroundManager] ✅ Background crossfade complete");
                },
                actualDuration
            );
        }
        else
        {
            Debug.LogError("[BackgroundManager] ❌ AnimationManager not found!");
        }
    }

    /// <summary>
    /// Get transition duration
    /// </summary>
    public float GetTransitionDuration()
    {
        return transitionDuration;
    }

    /// <summary>
    /// Get current background sprite (for debugging)
    /// </summary>
    public Sprite GetCurrentBackground()
    {
        return currentSprite;
    }
}

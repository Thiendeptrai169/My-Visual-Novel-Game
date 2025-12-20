using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;

/// <summary>
/// Generate CharacterDialogueSO assets for key story beats only (mode 2).
/// Usage: Tools > SpeakUp! > Generate Character Dialogue Assets (Key Scenes)
/// UPDATED VERSION - With automatic sprite loading
/// </summary>
public class CharacterDialogueAssetGenerator : EditorWindow
{
    [Header("Paths")]
    private string choiceDialoguePath = "Assets/DialogueAssets/Level1";
    private string charDialoguePath = "Assets/CharacterDialogues/Level1";
    private string characterSpritePath = "Assets/Art/Characters"; // Base path for character sprites

    [Header("Options")]
    private bool overwriteExisting = false;
    private bool includeEndings = false;
    private bool verboseLogging = true;
    private bool autoLoadSprites = true; // NEW: Auto-load sprites

    // Cache for loaded sprites
    private Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();

    [MenuItem("Tools/SpeakUp!/Generate Character Dialogue Assets (Key Scenes)")]
    public static void ShowWindow()
    {
        GetWindow<CharacterDialogueAssetGenerator>("Char Dialogue Generator");
    }

    private void OnGUI()
    {
        GUILayout.Label("SpeakUp! Character Dialogue Generator (Mode 2)", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        choiceDialoguePath = EditorGUILayout.TextField("DialougeSO Path:", choiceDialoguePath);
        charDialoguePath = EditorGUILayout.TextField("CharacterDialogueSO Path:", charDialoguePath);
        characterSpritePath = EditorGUILayout.TextField("Character Sprite Path:", characterSpritePath);

        EditorGUILayout.HelpBox(
            "Mode 2: Only key scenes will have CharacterDialogueSO.\n" +
            "Each generated scene returns to the matching DialougeSO node via nextChoiceNode.\n\n" +
            "✨ NEW: Tự động load sprite từ thư mục!\n" +
            "Format: CharacterName_Expression.png (ví dụ: Hung_Angry.png)",
            MessageType.Info
        );

        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing", overwriteExisting);
        includeEndings = EditorGUILayout.Toggle("Include Ending Scenes", includeEndings);
        autoLoadSprites = EditorGUILayout.Toggle("Auto Load Sprites", autoLoadSprites);
        verboseLogging = EditorGUILayout.Toggle("Verbose Logging", verboseLogging);

        EditorGUILayout.Space();

        // Validation UI
        GUI.color = AssetDatabase.IsValidFolder(choiceDialoguePath) ? Color.green : Color.red;
        EditorGUILayout.LabelField("DialougeSO Path exists: " + AssetDatabase.IsValidFolder(choiceDialoguePath));
        GUI.color = AssetDatabase.IsValidFolder(characterSpritePath) ? Color.green : Color.red;
        EditorGUILayout.LabelField("Character Sprite Path exists: " + AssetDatabase.IsValidFolder(characterSpritePath));
        GUI.color = Color.white;

        EditorGUILayout.Space();
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("✨ Generate Character Dialogue Assets", GUILayout.Height(50)))
        {
            Generate();
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space();
        if (GUILayout.Button("🔍 Debug: List All DialougeSO in Path", GUILayout.Height(30)))
        {
            DebugListDialogueAssets();
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("🖼️ Debug: List Available Sprites", GUILayout.Height(30)))
        {
            DebugListSprites();
        }
    }

    private void Generate()
    {
        // Validate paths first
        if (!ValidatePaths())
        {
            return;
        }

        Log("🚀 Starting generation process...");

        // Clear and load sprite cache if enabled
        if (autoLoadSprites)
        {
            LoadSpriteCache();
        }

        EnsureFolder(charDialoguePath);

        // Load all DialougeSO assets - OPTIMIZED VERSION
        Dictionary<string, DialougeSO> choiceNodes = LoadDialougeNodesOptimized(choiceDialoguePath);

        if (choiceNodes.Count == 0)
        {
            EditorUtility.DisplayDialog("Error",
                "❌ Không tìm thấy DialougeSO nào trong path:\n" + choiceDialoguePath +
                "\n\nHãy kiểm tra:\n" +
                "1. Path có đúng không?\n" +
                "2. Có file .asset nào trong folder đó không?\n" +
                "3. Các file có đúng type DialougeSO không?",
                "OK");
            return;
        }

        if (!choiceNodes.ContainsKey("S0"))
        {
            string availableNodes = string.Join(", ", choiceNodes.Keys.Take(10));
            EditorUtility.DisplayDialog("Missing S0",
                $"❌ Không tìm thấy node S0.\n\n" +
                $"Tìm thấy {choiceNodes.Count} nodes:\n{availableNodes}" +
                (choiceNodes.Count > 10 ? "..." : "") +
                "\n\nHãy kiểm tra node S0 có đúng nodeId không?",
                "OK");
            return;
        }

        Log($"✅ Loaded {choiceNodes.Count} DialougeSO nodes successfully");
        Log($"🖼️ Loaded {spriteCache.Count} character sprites");

        int createdCount = 0;
        int skippedCount = 0;

        // KEY SCENES LIST (Mode 2) - UPDATED WITH NEW SCRIPT
        createdCount += CreateScene_S0(choiceNodes, ref skippedCount);
        createdCount += CreateScene_S2(choiceNodes, ref skippedCount);
        createdCount += CreateScene_S4(choiceNodes, ref skippedCount);
        createdCount += CreateScene_S5(choiceNodes, ref skippedCount);
        createdCount += CreateScene_S6(choiceNodes, ref skippedCount);
        createdCount += CreateScene_S7(choiceNodes, ref skippedCount);
        createdCount += CreateScene_S8(choiceNodes, ref skippedCount);
        createdCount += CreateScene_S10(choiceNodes, ref skippedCount);
        createdCount += CreateScene_S12(choiceNodes, ref skippedCount);
        createdCount += CreateScene_S17(choiceNodes, ref skippedCount);
        createdCount += CreateScene_S18(choiceNodes, ref skippedCount);
        createdCount += CreateScene_S19(choiceNodes, ref skippedCount);
        createdCount += CreateScene_S20(choiceNodes, ref skippedCount);
        createdCount += CreateScene_S21(choiceNodes, ref skippedCount);

        if (includeEndings)
        {
            createdCount += CreateScene_E0(choiceNodes, ref skippedCount);
            createdCount += CreateScene_E1(choiceNodes, ref skippedCount);
            createdCount += CreateScene_E2(choiceNodes, ref skippedCount);
            createdCount += CreateScene_E3(choiceNodes, ref skippedCount);
            createdCount += CreateScene_E7(choiceNodes, ref skippedCount);
            createdCount += CreateScene_E8(choiceNodes, ref skippedCount);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Log($"✅ Done! Created: {createdCount}, Skipped: {skippedCount}");

        EditorUtility.DisplayDialog("Success!",
            $"✨ Generation Complete!\n\n" +
            $"✅ Created/Updated: {createdCount}\n" +
            $"⏭️ Skipped: {skippedCount}\n" +
            $"🖼️ Sprites loaded: {spriteCache.Count}\n\n" +
            $"Output: {charDialoguePath}",
            "OK");
    }

    #region Sprite Loading (NEW)

    /// <summary>
    /// Load all character sprites into cache
    /// Expected format: CharacterName_Expression.png
    /// Example: Hung_Angry.png, Nam_Worried.png
    /// </summary>
    private void LoadSpriteCache()
    {
        spriteCache.Clear();

        if (!AssetDatabase.IsValidFolder(characterSpritePath))
        {
            Debug.LogWarning($"⚠️ Character sprite path not found: {characterSpritePath}");
            return;
        }

        Log("🖼️ Loading character sprites...");

        // Get all character folders (Hung, Nam, Linh, Player, Teacher)
        string[] characterFolders = new string[] { "Hung", "Nam", "Linh", "Player", "Teacher" };

        foreach (string charName in characterFolders)
        {
            string charFolder = $"{characterSpritePath}/{charName}";

            if (!AssetDatabase.IsValidFolder(charFolder))
            {
                Log($"  ⚠️ Folder not found: {charFolder}");
                continue;
            }

            // Find all sprites in this character's folder
            string[] spritePaths = Directory.GetFiles(charFolder, "*.png", SearchOption.AllDirectories);

            foreach (string path in spritePaths)
            {
                string normalizedPath = path.Replace("\\", "/");
                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(normalizedPath);

                if (sprite != null)
                {
                    // Extract sprite name without extension
                    string spriteName = Path.GetFileNameWithoutExtension(normalizedPath);

                    // Store with key format: "CharacterName_Expression"
                    string key = spriteName; // Already in correct format

                    if (!spriteCache.ContainsKey(key))
                    {
                        spriteCache.Add(key, sprite);
                        Log($"  ✓ Loaded sprite: {key}");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Duplicate sprite key: {key}");
                    }
                }
            }
        }

        Log($"✅ Loaded {spriteCache.Count} sprites total");
    }

    /// <summary>
    /// Get sprite by character and expression
    /// </summary>
    private Sprite GetSprite(CharacterSprite character, CharacterExpression expression)
    {
        if (!autoLoadSprites || spriteCache.Count == 0)
            return null;

        // Build key: "CharacterName_Expression"
        string key = $"{character}_{expression}";

        if (spriteCache.TryGetValue(key, out Sprite sprite))
        {
            return sprite;
        }

        // Try fallback to Neutral if specific expression not found
        string fallbackKey = $"{character}_Neutral";
        if (expression != CharacterExpression.Neutral && spriteCache.TryGetValue(fallbackKey, out Sprite fallbackSprite))
        {
            Log($"  ⚠️ Using fallback Neutral for {key}");
            return fallbackSprite;
        }

        Log($"  ⚠️ Sprite not found: {key}");
        return null;
    }

    /// <summary>
    /// Debug: List all available sprites
    /// </summary>
    private void DebugListSprites()
    {
        LoadSpriteCache();

        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log($"🖼️ Available Sprites ({spriteCache.Count}):");
        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        foreach (var kvp in spriteCache.OrderBy(x => x.Key))
        {
            Debug.Log($"  • {kvp.Key} → {kvp.Value.name}");
        }

        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        EditorUtility.DisplayDialog("Sprite Debug",
            $"Found {spriteCache.Count} sprites.\n\n" +
            "Check Console for full list.",
            "OK");
    }

    #endregion

    #region Validation & Loading (OPTIMIZED)

    private bool ValidatePaths()
    {
        if (string.IsNullOrEmpty(choiceDialoguePath))
        {
            EditorUtility.DisplayDialog("Error", "❌ DialougeSO Path is empty!", "OK");
            return false;
        }

        if (string.IsNullOrEmpty(charDialoguePath))
        {
            EditorUtility.DisplayDialog("Error", "❌ CharacterDialogueSO Path is empty!", "OK");
            return false;
        }

        if (!AssetDatabase.IsValidFolder(choiceDialoguePath))
        {
            EditorUtility.DisplayDialog("Error",
                $"❌ Folder không tồn tại:\n{choiceDialoguePath}\n\n" +
                "Hãy tạo folder trước hoặc sửa lại path!",
                "OK");
            return false;
        }

        // Sprite path is optional but warn if not found
        if (autoLoadSprites && !AssetDatabase.IsValidFolder(characterSpritePath))
        {
            Debug.LogWarning($"⚠️ Character sprite folder not found: {characterSpritePath}");
        }

        return true;
    }

    /// <summary>
    /// OPTIMIZED: Load all DialougeSO assets much faster
    /// </summary>
    private Dictionary<string, DialougeSO> LoadDialougeNodesOptimized(string folderPath)
    {
        var map = new Dictionary<string, DialougeSO>();
        var loadedAssets = new List<DialougeSO>();

        Log($"🔍 Searching for DialougeSO in: {folderPath}");

        // Method 1: Direct asset loading (faster)
        string[] assetPaths = Directory.GetFiles(folderPath, "*.asset", SearchOption.AllDirectories);

        foreach (string path in assetPaths)
        {
            string normalizedPath = path.Replace("\\", "/");
            DialougeSO asset = AssetDatabase.LoadAssetAtPath<DialougeSO>(normalizedPath);

            if (asset != null)
            {
                loadedAssets.Add(asset);
                Log($"  ✓ Loaded: {asset.name} (nodeId: {asset.nodeId})");
            }
        }

        // If Method 1 fails, fallback to FindAssets
        if (loadedAssets.Count == 0)
        {
            Log("⚠️ Direct loading found 0 assets, trying FindAssets...");
            string[] guids = AssetDatabase.FindAssets("t:DialougeSO", new[] { folderPath });

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                DialougeSO asset = AssetDatabase.LoadAssetAtPath<DialougeSO>(path);

                if (asset != null)
                {
                    loadedAssets.Add(asset);
                    Log($"  ✓ Found via GUID: {asset.name} (nodeId: {asset.nodeId})");
                }
            }
        }

        // Build dictionary
        foreach (var asset in loadedAssets)
        {
            if (string.IsNullOrEmpty(asset.nodeId))
            {
                Debug.LogWarning($"⚠️ Asset {asset.name} has empty nodeId, skipping");
                continue;
            }

            if (map.ContainsKey(asset.nodeId))
            {
                Debug.LogWarning($"⚠️ Duplicate nodeId '{asset.nodeId}' found in {asset.name}, keeping first");
                continue;
            }

            map.Add(asset.nodeId, asset);
        }

        Log($"✅ Loaded {map.Count} unique DialougeSO nodes");
        return map;
    }

    /// <summary>
    /// Debug tool to list all found assets
    /// </summary>
    private void DebugListDialogueAssets()
    {
        if (!AssetDatabase.IsValidFolder(choiceDialoguePath))
        {
            Debug.LogError($"❌ Invalid folder: {choiceDialoguePath}");
            return;
        }

        var nodes = LoadDialougeNodesOptimized(choiceDialoguePath);

        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Debug.Log($"📋 Found {nodes.Count} DialougeSO assets:");
        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        foreach (var kvp in nodes.OrderBy(x => x.Key))
        {
            Debug.Log($"  • {kvp.Key} → {kvp.Value.name}");
        }

        Debug.Log($"━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        EditorUtility.DisplayDialog("Debug Info",
            $"Found {nodes.Count} DialougeSO assets.\n\n" +
            "Check Console for full list.",
            "OK");
    }

    private void Log(string message)
    {
        if (verboseLogging)
        {
            Debug.Log($"[CharDialogueGen] {message}");
        }
    }

    #endregion

    #region Create Helpers

    private CharacterDialogueSO CreateOrLoadCharDialogue(string fileName, string sceneId)
    {
        string path = $"{charDialoguePath}/{fileName}.asset";
        CharacterDialogueSO asset = AssetDatabase.LoadAssetAtPath<CharacterDialogueSO>(path);

        if (asset != null && !overwriteExisting)
        {
            Log($"⏭️ Skip {fileName} (already exists)");
            return asset;
        }

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<CharacterDialogueSO>();
            AssetDatabase.CreateAsset(asset, path);
            Log($"✨ Created new: {fileName}");
        }
        else
        {
            Log($"♻️ Overwriting: {fileName}");
        }

        asset.sceneId = sceneId;
        asset.dialogueLines ??= new List<DialogueLine>();
        asset.dialogueLines.Clear();
        asset.autoAdvance = true;

        EditorUtility.SetDirty(asset);
        return asset;
    }

    /// <summary>
    /// UPDATED: Now automatically loads sprite
    /// </summary>
    private void AddLine(CharacterDialogueSO scene,
        string speakerName,
        CharacterSprite sprite,
        CharacterPosition pos,
        CharacterExpression expr,
        string text)
    {
        // Get sprite automatically
        Sprite characterSprite = GetSprite(sprite, expr);

        scene.dialogueLines.Add(new DialogueLine
        {
            speakerName = speakerName,
            characterSprite = sprite,
            position = pos,
            expression = expr,
            dialogueText = text,
            characterImage = characterSprite // AUTO-FILLED!
        });

        if (characterSprite != null)
        {
            Log($"    🖼️ Assigned sprite: {sprite}_{expr}");
        }
    }

    private void EnsureFolder(string fullPath)
    {
        if (AssetDatabase.IsValidFolder(fullPath)) return;

        string[] parts = fullPath.Split('/');
        string cur = parts[0];

        for (int i = 1; i < parts.Length; i++)
        {
            string next = $"{cur}/{parts[i]}";
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(cur, parts[i]);
                Log($"📁 Created folder: {next}");
            }
            cur = next;
        }
    }

    private int FinalizeScene(CharacterDialogueSO scene, Dictionary<string, DialougeSO> nodes, string nextNodeIdOrNull, ref int skippedCount)
    {
        if (scene == null)
        {
            skippedCount++;
            return 0;
        }

        if (!string.IsNullOrEmpty(nextNodeIdOrNull))
        {
            if (nodes.TryGetValue(nextNodeIdOrNull, out var next))
            {
                scene.nextChoiceNode = next;
                Log($"  → Links to: {nextNodeIdOrNull}");
            }
            else
            {
                Debug.LogWarning($"⚠️ Node '{nextNodeIdOrNull}' not found for scene {scene.sceneId}");
                scene.nextChoiceNode = null;
            }
        }
        else
        {
            scene.nextChoiceNode = null;
        }

        EditorUtility.SetDirty(scene);
        return 1;
    }

    #endregion

    #region Key Scenes (Mode 2) - UPDATED WITH NEW SCRIPT

    private int CreateScene_S0(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_S0_Intro", "S0_Intro");
        if (scene == null) { skipped++; return 0; }

        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Mày tưởng tao không biết mày méc chuyện hôm bữa hả Nam?");
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Left, CharacterExpression.Worried,
            "Tao không méc… tao chỉ…");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Chỉ cái gì? Tao bị gọi lên phòng giám thị vì ai?");
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Left, CharacterExpression.Sad,
            "Tao sợ bị dính chung…");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Sợ thì đừng có chơi sau lưng!");

        return FinalizeScene(scene, nodes, "S0", ref skipped);
    }

    private int CreateScene_S2(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_S2_PlayerCallsOut", "S2_PlayerCallsOut");
        if (scene == null) { skipped++; return 0; }

        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Ê Hùng, có chuyện gì mà làm căng vậy?");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Mày xen vô làm gì?");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Tao chỉ thấy mày đang ép người ta quá rồi.");
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Left, CharacterExpression.Worried,
            "Bạn…");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Nó đáng lắm. Mày biết nó làm gì không?");

        return FinalizeScene(scene, nodes, "S2", ref skipped);
    }

    private int CreateScene_S4(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_S4_LinhArrives", "S4_LinhArrives");
        if (scene == null) { skipped++; return 0; }

        AddLine(scene, "Linh", CharacterSprite.Linh, CharacterPosition.Left, CharacterExpression.Worried,
            "Trời ơi, sao đông vậy? Có chuyện gì?");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Hùng với Nam đang căng. Mày đứng sau tao nha.");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Thêm người tới làm gì? Tụi bây tính hội đồng tao hả?");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Không ai hội đồng ai hết. Tụi tao chỉ muốn mày dừng tay lại trước.");
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Left, CharacterExpression.Worried,
            "…");

        return FinalizeScene(scene, nodes, "S4", ref skipped);
    }

    private int CreateScene_S5(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_S5_SoftIntervention", "S5_SoftIntervention");
        if (scene == null) { skipped++; return 0; }

        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Hùng, bình tĩnh. Có gì nói chuyện đàng hoàng.");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Đàng hoàng cái gì? Nó làm tao nhục muốn chết.");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Tao nghe đây. Nhưng mày đừng dí người ta kiểu đó.");
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Left, CharacterExpression.Worried,
            "Tao không cố ý…");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Im! Mày lúc nào cũng 'không cố ý'!");

        return FinalizeScene(scene, nodes, "S5", ref skipped);
    }

    private int CreateScene_S6(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_S6_Blocking", "S6_Blocking");
        if (scene == null) { skipped++; return 0; }

        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Dừng lại. Mày đứng nói chuyện thôi, đừng đụng tay.");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Tránh ra.");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Worried,
            "Tao không muốn gây chuyện. Nhưng mày làm vậy là quá rồi.");
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Left, CharacterExpression.Worried,
            "Đừng… đừng đánh…");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Mày bênh nó hả?");

        return FinalizeScene(scene, nodes, "S6", ref skipped);
    }

    private int CreateScene_S7(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_S7_DistractionLie", "S7_DistractionLie");
        if (scene == null) { skipped++; return 0; }

        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Hùng, tao vừa nghe nói cô đang đi ngang khu này đó.");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Thiệt không?");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Thiệt hay không không quan trọng. Quan trọng là mày làm vậy là tự đẩy mình vô rắc rối.");
        AddLine(scene, "Linh", CharacterSprite.Linh, CharacterPosition.Left, CharacterExpression.Worried,
            "Ừ… thôi mày dừng lại đi.");
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Left, CharacterExpression.Worried,
            "Hùng… tao xin…");

        return FinalizeScene(scene, nodes, "S7", ref skipped);
    }

    private int CreateScene_S8(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_S8_RecordingEscalates", "S8_RecordingEscalates");
        if (scene == null) { skipped++; return 0; }

        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Worried,
            "Tao quay để phòng khi cần làm chứng thôi, không phải để bêu ai.");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Tắt đi. Tao ghét nhất cái kiểu quay lén.");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Ok, tao có thể không dí sát vô mặt mày. Nhưng mày cũng dừng tay trước.");
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Left, CharacterExpression.Worried,
            "Đừng làm lớn chuyện…");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Càng nói tao càng tức!");

        return FinalizeScene(scene, nodes, "S8", ref skipped);
    }

    private int CreateScene_S10(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_S10_TeacherCalled", "S10_TeacherCalled");
        if (scene == null) { skipped++; return 0; }

        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Worried,
            "Alo cô, cô qua hành lang A giúp tụi em với ạ.");
        AddLine(scene, "Cô giám thị", CharacterSprite.Teacher, CharacterPosition.Right, CharacterExpression.Neutral,
            "Có chuyện gì?");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Worried,
            "Có bạn đang gây căng thẳng, em sợ sẽ xảy ra xô xát.");
        AddLine(scene, "Cô giám thị", CharacterSprite.Teacher, CharacterPosition.Right, CharacterExpression.Neutral,
            "Được. Cô tới ngay. Các em đứng xa ra, đừng kích động thêm.");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Dạ.");

        return FinalizeScene(scene, nodes, "S10", ref skipped);
    }

    private int CreateScene_S12(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_S12_HungExplains", "S12_HungExplains");
        if (scene == null) { skipped++; return 0; }

        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Rồi, nói tao nghe. Mày bực cái gì?");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Tao bị gọi lên phòng giám thị. Người ta nhìn tao như thằng tội đồ.");
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Left, CharacterExpression.Worried,
            "Tao không nói… tao chỉ lỡ hỏi…");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Nam, mày nói rõ cho Hùng nghe. Còn Hùng, mày nghe hết rồi hẵng nóng.");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Nghe cái gì nữa? Tao mất mặt rồi!");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Worried,
            "Mất mặt thì giải quyết cho ra lẽ. Đụng tay là tự thua.");

        return FinalizeScene(scene, nodes, "S12", ref skipped);
    }

    private int CreateScene_S17(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_S17_ViolenceTriggered", "S17_ViolenceTriggered");
        if (scene == null) { skipped++; return 0; }

        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Worried,
            "Dừng! Đừng ai đụng tay nữa!");
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Left, CharacterExpression.Worried,
            "Đừng mà!");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Tránh ra!");
        AddLine(scene, "Linh", CharacterSprite.Linh, CharacterPosition.Left, CharacterExpression.Worried,
            "Có người tới kìa!");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Worried,
            "Nam lùi lại sau tao!");

        return FinalizeScene(scene, nodes, "S17", ref skipped);
    }

    private int CreateScene_S18(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_S18_TeacherArrives", "S18_TeacherArrives");
        if (scene == null) { skipped++; return 0; }

        AddLine(scene, "Cô giám thị", CharacterSprite.Teacher, CharacterPosition.Center, CharacterExpression.Neutral,
            "DỪNG LẠI. Tất cả đứng cách ra.");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Left, CharacterExpression.Neutral,
            "Dạ cô, tụi em đang cố giữ cho không có xô xát.");
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Left, CharacterExpression.Worried,
            "Cô ơi…");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Tại nó…");
        AddLine(scene, "Cô giám thị", CharacterSprite.Teacher, CharacterPosition.Center, CharacterExpression.Neutral,
            "Không tranh cãi ở đây. Hai em theo cô lên phòng. Em còn lại ở đây tường trình.");

        return FinalizeScene(scene, nodes, "S18", ref skipped);
    }

    private int CreateScene_S19(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_S19_ValidateEmotion", "S19_ValidateEmotion");
        if (scene == null) { skipped++; return 0; }

        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Tao hiểu mày bực vì bị nghi oan.");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Đúng! Tao bị chơi dơ mà.");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Nhưng nếu mày dùng bạo lực, người ta chỉ nhớ mày là thằng đánh người.");
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Left, CharacterExpression.Sad,
            "Tao không muốn vậy…");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Giờ tụi mình chọn cách giải quyết mà mày không phải hối hận về sau.");

        return FinalizeScene(scene, nodes, "S19", ref skipped);
    }

    private int CreateScene_S20(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_S20_MoveAway", "S20_MoveAway");
        if (scene == null) { skipped++; return 0; }

        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Ra chỗ ít người. Nói chuyện cho ra chuyện.");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Neutral,
            "Tao vẫn tức.");
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Left, CharacterExpression.Worried,
            "Tao xin lỗi…");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Luật đơn giản: nói, không đụng tay. Ai nóng thì dừng lại hít thở.");
        AddLine(scene, "Linh", CharacterSprite.Linh, CharacterPosition.Left, CharacterExpression.Neutral,
            "Ừ, tao canh ở đây. Có gì tao gọi cô.");

        return FinalizeScene(scene, nodes, "S20", ref skipped);
    }

    private int CreateScene_S21(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_S21_ClearBoundary", "S21_ClearBoundary");
        if (scene == null) { skipped++; return 0; }

        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Muốn nói gì thì nói, nhưng tay bỏ khỏi người ta trước đã.");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Angry,
            "Mày tưởng mày là ai?");
        AddLine(scene, "Bạn", CharacterSprite.Player, CharacterPosition.Center, CharacterExpression.Neutral,
            "Tao là người đứng đây để chuyện này không biến thành đánh nhau.");
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Left, CharacterExpression.Worried,
            "Hùng…");
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Neutral,
            "… rồi. Tao bỏ.");

        return FinalizeScene(scene, nodes, "S21", ref skipped);
    }

    #endregion

    #region Optional Ending Scenes

    private int CreateScene_E0(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_E0_WalkAway", "E0_WalkAway");
        if (scene == null) { skipped++; return 0; }
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Center, CharacterExpression.Sad,
            "… mày thấy hết rồi mà vẫn đi hả?");
        return FinalizeScene(scene, nodes, null, ref skipped);
    }

    private int CreateScene_E1(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_E1_RecordOnly", "E1_RecordOnly");
        if (scene == null) { skipped++; return 0; }
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Center, CharacterExpression.Sad,
            "Clip đó… có thể giúp về sau. Nhưng lúc đó tao vẫn một mình…");
        return FinalizeScene(scene, nodes, null, ref skipped);
    }

    private int CreateScene_E2(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_E2_SupportLater", "E2_SupportLater");
        if (scene == null) { skipped++; return 0; }
        AddLine(scene, "Linh", CharacterSprite.Linh, CharacterPosition.Center, CharacterExpression.Neutral,
            "Không sao. Mày không phải tự chịu một mình nữa.");
        return FinalizeScene(scene, nodes, null, ref skipped);
    }

    private int CreateScene_E3(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_E3_RetreatWithVictim", "E3_RetreatWithVictim");
        if (scene == null) { skipped++; return 0; }
        AddLine(scene, "Nam", CharacterSprite.Nam, CharacterPosition.Left, CharacterExpression.Worried,
            "Cảm ơn… tao chỉ muốn rời khỏi đó.");
        AddLine(scene, "Linh", CharacterSprite.Linh, CharacterPosition.Right, CharacterExpression.Worried,
            "Ừ. Ra chỗ an toàn đã.");
        return FinalizeScene(scene, nodes, null, ref skipped);
    }

    private int CreateScene_E7(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_E7_GuidedResolution", "E7_GuidedResolution");
        if (scene == null) { skipped++; return 0; }
        AddLine(scene, "Hùng", CharacterSprite.Hung, CharacterPosition.Right, CharacterExpression.Neutral,
            "… thôi. Hẹn gặp giáo viên cố vấn. Nói cho ra lẽ.");
        return FinalizeScene(scene, nodes, null, ref skipped);
    }

    private int CreateScene_E8(Dictionary<string, DialougeSO> nodes, ref int skipped)
    {
        var scene = CreateOrLoadCharDialogue("CD_E8_TalkingCircle", "E8_TalkingCircle");
        if (scene == null) { skipped++; return 0; }
        AddLine(scene, "Linh", CharacterSprite.Linh, CharacterPosition.Center, CharacterExpression.Neutral,
            "Ok. Mỗi người nói một lượt. Không cắt lời. Không đụng tay.");
        return FinalizeScene(scene, nodes, null, ref skipped);
    }

    #endregion
}
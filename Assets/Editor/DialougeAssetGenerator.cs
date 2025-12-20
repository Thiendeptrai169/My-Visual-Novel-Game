using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Unity Editor Tool to auto-generate all dialogue assets for SpeakUp! V2
/// Usage: Tools > SpeakUp! > Generate All Dialogue Assets
/// UPDATED: Now auto-links CharacterDialogueSO assets + proper ending metadata!
/// </summary>
public class DialogueAssetGenerator : EditorWindow
{
    private string assetPath = "Assets/Resources/DialogueAssets/Level1";
    private string characterDialoguePath = "Assets/CharacterDialogues/Level1";
    private bool overwriteExisting = false;
    private bool autoLinkCharacterDialogue = true;

    [MenuItem("Tools/SpeakUp!/Generate All Dialogue Assets")]
    public static void ShowWindow()
    {
        GetWindow<DialogueAssetGenerator>("Dialogue Generator V2");
    }

    private void OnGUI()
    {
        GUILayout.Label("SpeakUp! Dialogue Asset Generator V2", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        assetPath = EditorGUILayout.TextField("Asset Path:", assetPath);
        characterDialoguePath = EditorGUILayout.TextField("Character Dialogue Path:", characterDialoguePath);

        EditorGUILayout.HelpBox("All dialogue assets will be created in this folder.", MessageType.Info);

        EditorGUILayout.Space();
        overwriteExisting = EditorGUILayout.Toggle("Overwrite Existing", overwriteExisting);
        autoLinkCharacterDialogue = EditorGUILayout.Toggle("Auto Link Character Dialogue", autoLinkCharacterDialogue);

        EditorGUILayout.Space();

        // Validation UI
        GUI.color = AssetDatabase.IsValidFolder(assetPath) ? Color.green : Color.red;
        EditorGUILayout.LabelField("Dialogue Path exists: " + AssetDatabase.IsValidFolder(assetPath));
        GUI.color = AssetDatabase.IsValidFolder(characterDialoguePath) ? Color.green : Color.red;
        EditorGUILayout.LabelField("Character Dialogue Path exists: " + AssetDatabase.IsValidFolder(characterDialoguePath));
        GUI.color = Color.white;

        EditorGUILayout.Space();

        if (GUILayout.Button("✨ Generate All Dialogue Assets", GUILayout.Height(50)))
        {
            GenerateAllDialogues();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Will create:\n" +
            "• 22 dialogue nodes (S0-S21)\n" +
            "• 9 ending nodes (E0-E8)\n" +
            "• All with tension, state effects, and conditional branches!\n" +
            "✨ NEW: Auto-link CharacterDialogueSO if found!",
            MessageType.Warning);
    }

    private void GenerateAllDialogues()
    {
        // Create folder if doesn't exist
        if (!AssetDatabase.IsValidFolder(assetPath))
        {
            string[] folders = assetPath.Split('/');
            string currentPath = folders[0];
            for (int i = 1; i < folders.Length; i++)
            {
                if (!AssetDatabase.IsValidFolder(currentPath + "/" + folders[i]))
                {
                    AssetDatabase.CreateFolder(currentPath, folders[i]);
                }
                currentPath += "/" + folders[i];
            }
        }

        Debug.Log("🚀 Starting dialogue generation...");

        // Load all CharacterDialogueSO assets if auto-link is enabled
        Dictionary<string, CharacterDialogueSO> characterDialogues = new Dictionary<string, CharacterDialogueSO>();
        if (autoLinkCharacterDialogue)
        {
            characterDialogues = LoadCharacterDialogues();
            Debug.Log($"📋 Loaded {characterDialogues.Count} CharacterDialogueSO assets");
        }

        // Store all created assets for linking
        Dictionary<string, DialougeSO> nodes = new Dictionary<string, DialougeSO>();

        // Create all nodes
        nodes["S0"] = CreateS0();
        nodes["S1"] = CreateS1();
        nodes["S2"] = CreateS2();
        nodes["S3"] = CreateS3();
        nodes["S4"] = CreateS4();
        nodes["S5"] = CreateS5();
        nodes["S6"] = CreateS6();
        nodes["S7"] = CreateS7();
        nodes["S8"] = CreateS8();
        nodes["S9"] = CreateS9();
        nodes["S10"] = CreateS10();
        nodes["S11"] = CreateS11();
        nodes["S12"] = CreateS12();
        nodes["S13"] = CreateS13();
        nodes["S14"] = CreateS14();
        nodes["S15"] = CreateS15();
        nodes["S16"] = CreateS16();
        nodes["S17"] = CreateS17();
        nodes["S18"] = CreateS18();
        nodes["S19"] = CreateS19();
        nodes["S20"] = CreateS20();
        nodes["S21"] = CreateS21();

        // Ending nodes
        nodes["E0"] = CreateE0();
        nodes["E1"] = CreateE1();
        nodes["E2"] = CreateE2();
        nodes["E3"] = CreateE3();
        nodes["E4"] = CreateE4();
        nodes["E5"] = CreateE5();
        nodes["E6"] = CreateE6();
        nodes["E7"] = CreateE7();
        nodes["E8"] = CreateE8();

        Debug.Log("📝 All nodes created. Now linking...");

        // Link nodes together
        LinkNodes(nodes);

        // Link CharacterDialogueSO if enabled
        if (autoLinkCharacterDialogue)
        {
            LinkCharacterDialogues(nodes, characterDialogues);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"✅ Successfully generated {nodes.Count} dialogue assets at {assetPath}!");
        EditorUtility.DisplayDialog("Success!",
            $"Generated {nodes.Count} dialogue assets!\n" +
            $"Linked {characterDialogues.Count} character dialogues!\n\n" +
            $"Path: {assetPath}",
            "Awesome!");
    }

    #region Character Dialogue Loading & Linking

    private Dictionary<string, CharacterDialogueSO> LoadCharacterDialogues()
    {
        var map = new Dictionary<string, CharacterDialogueSO>();

        if (!AssetDatabase.IsValidFolder(characterDialoguePath))
        {
            Debug.LogWarning($"⚠️ Character dialogue path not found: {characterDialoguePath}");
            return map;
        }

        string[] assetPaths = Directory.GetFiles(characterDialoguePath, "*.asset", SearchOption.AllDirectories);

        foreach (string path in assetPaths)
        {
            string normalizedPath = path.Replace("\\", "/");
            CharacterDialogueSO asset = AssetDatabase.LoadAssetAtPath<CharacterDialogueSO>(normalizedPath);

            if (asset != null && !string.IsNullOrEmpty(asset.sceneId))
            {
                string nodeId = ExtractNodeId(asset.sceneId);

                if (!string.IsNullOrEmpty(nodeId))
                {
                    if (!map.ContainsKey(nodeId))
                    {
                        map.Add(nodeId, asset);
                        Debug.Log($"  ✓ Found CharacterDialogue: {nodeId} → {asset.name}");
                    }
                    else
                    {
                        Debug.LogWarning($"⚠️ Duplicate CharacterDialogue for {nodeId}");
                    }
                }
            }
        }

        return map;
    }

    private string ExtractNodeId(string sceneId)
    {
        if (string.IsNullOrEmpty(sceneId))
            return null;

        string[] parts = sceneId.Split('_');
        return parts.Length > 0 ? parts[0] : null;
    }

    private void LinkCharacterDialogues(Dictionary<string, DialougeSO> nodes, Dictionary<string, CharacterDialogueSO> characterDialogues)
    {
        Debug.Log("🔗 Linking CharacterDialogueSO to DialougeSO nodes...");

        int linkedCount = 0;

        foreach (var kvp in characterDialogues)
        {
            string nodeId = kvp.Key;
            CharacterDialogueSO charDialogue = kvp.Value;

            if (nodes.TryGetValue(nodeId, out DialougeSO dialogueNode))
            {
                dialogueNode.characterDialogue = charDialogue;
                EditorUtility.SetDirty(dialogueNode);
                linkedCount++;
                Debug.Log($"  ✓ Linked {nodeId}: {dialogueNode.name} → {charDialogue.name}");
            }
            else
            {
                Debug.LogWarning($"⚠️ No DialougeSO found for CharacterDialogue: {nodeId}");
            }
        }

        Debug.Log($"✅ Linked {linkedCount} CharacterDialogueSO assets");
    }

    #endregion

    #region Node Creation Methods - COMPLETE WITH ALL CHOICES

    private DialougeSO CreateS0()
    {
        var node = CreateDialogueAsset("S0_BatGap", "S0");
        node.npcText = "Bạn vừa hết tiết, đi trên hành lang vắng, thấy Hùng dí Nam vào tường, nói lớn:\n\n\"Mày tưởng tao không biết mày méc chuyện hôm bữa hả?\"";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Giả vờ đi ngang, lắng nghe cho rõ đã",
                tensionImpact = 0,
                stateEffects = new List<StateEffect>()
            },
            new DialougeChoice
            {
                choiceText = "Hơi chạy lại nhanh, gọi \"Ê Hùng, gì vậy?\"",
                tensionImpact = 5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "playerReputation", floatValue = 0.1f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Rút điện thoại ra, mở camera nhưng để thấp",
                tensionImpact = 0,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "hasRecording", boolValue = true }
                }
            },
            new DialougeChoice
            {
                choiceText = "Quay đầu đi chỗ khác, giả vờ không thấy",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "playerWalkedAway", boolValue = true },
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "playerReputation", floatValue = -0.2f },
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "victimTrust", floatValue = -0.3f }
                }
            }
        };

        return node;
    }

    private DialougeSO CreateS1()
    {
        var node = CreateDialogueAsset("S1_DungNgheLén", "S1");
        node.npcText = "Bạn đứng khuất sau cột, nghe Hùng vừa chửi vừa đập tay vào tường cạnh đầu Nam.";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Đợi thêm xem mức độ căng thẳng tới đâu",
                tensionImpact = 5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "rumorLevel", floatValue = 0.1f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Nhắn tin nhanh cho Linh: \"Hành lang A, ra đây gấp\"",
                tensionImpact = 0,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyInt, variableName = "bystanderSupport", intValue = 1 }
                }
            },
            new DialougeChoice
            {
                choiceText = "Thở dài, nghĩ \"không nên xen vào chuyện người khác\" rồi bỏ đi",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "playerWalkedAway", boolValue = true },
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "playerReputation", floatValue = -0.2f },
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "victimTrust", floatValue = -0.4f }
                }
            }
        };

        return node;
    }

    private DialougeSO CreateS2()
    {
        var node = CreateDialogueAsset("S2_GoiTrucDien", "S2");
        node.npcText = "Bạn lên tiếng, Hùng quay sang, ánh mắt khó chịu, Nam liếc nhìn bạn như cầu cứu.";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Cười cười: \"Ê bớt bớt, ở đây đông người mà\"",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "playerReputation", floatValue = 0.1f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Nghiêm mặt: \"Buông bạn tao ra coi\"",
                tensionImpact = 10,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "aggressorFocusOnPlayer", boolValue = true },
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "victimTrust", floatValue = 0.1f }
                },
                useConditionalBranching = true,
                conditionalBranches = new List<ConditionalBranch>
                {
                    new ConditionalBranch
                    {
                        conditionVariable = "tension",
                        comparison = ConditionalBranch.ComparisonType.GreaterThan,
                        compareWithVariable = false,
                        comparisonValue = 70
                    }
                }
            },
            new DialougeChoice
            {
                choiceText = "Giả vờ hỏi chuyện khác: \"Ô, Hùng ơi, giáo viên chủ nhiệm đang tìm mày kìa\"",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "hungReputation", floatValue = -0.1f }
                }
            }
        };

        return node;
    }

    private DialougeSO CreateS3()
    {
        var node = CreateDialogueAsset("S3_DangQuayKin", "S3");
        node.npcText = "Điện thoại đang quay, bạn có thể vừa ghi hình vừa quyết định làm gì tiếp.";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Tiếp tục chỉ quay, chưa xen vào",
                tensionImpact = 5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "rumorLevel", floatValue = 0.1f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Gửi nhanh video cho Linh với caption \"Ra đây giúp\"",
                tensionImpact = 0,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyInt, variableName = "bystanderSupport", intValue = 1 }
                }
            },
            new DialougeChoice
            {
                choiceText = "Bỏ điện thoại vào túi, bước lại gần can thiệp",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "victimTrust", floatValue = 0.1f }
                }
            }
        };

        return node;
    }

    private DialougeSO CreateS4()
    {
        var node = CreateDialogueAsset("S4_LinhXuatHien", "S4");
        node.npcText = "Linh chạy tới, thở hổn hển: \"Có chuyện gì?\". Cả hai cùng nhìn về phía Hùng và Nam.";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Bàn nhanh với Linh: \"Tí nữa mày kéo Nam, tao nói chuyện với Hùng\"",
                tensionImpact = 0,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyInt, variableName = "bystanderSupport", intValue = 2 }
                }
            },
            new DialougeChoice
            {
                choiceText = "Bảo Linh đi gọi cô, bạn ở lại quan sát",
                tensionImpact = 0,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "calledHelp", boolValue = true },
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "teacherOnTheWay", boolValue = true },
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "teacherTrust", floatValue = 0.1f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Bảo Linh đứng quay video, bạn đứng ngoài coi",
                tensionImpact = 5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "hasRecording", boolValue = true },
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "rumorLevel", floatValue = 0.2f },
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "playerReputation", floatValue = -0.1f }
                }
            }
        };

        return node;
    }

    private DialougeSO CreateS5()
    {
        var node = CreateDialogueAsset("S5_CanThiepBangGiongNhe", "S5");
        node.npcText = "Bạn bước tới gần, cố giữ tông giọng bình thường:\n\n\"Ủa có chuyện gì mà gắt vậy, bình tĩnh tí đi?\"";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Hỏi Hùng: \"Có gì nói từ từ, mày bực cái gì?\"",
                tensionImpact = -10,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "hungReputation", floatValue = 0.1f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Quay sang Nam: \"Mày có ổn không? Muốn tao ở lại không?\"",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "victimTrust", floatValue = 0.3f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Lỡ miệng nói: \"Đánh nhau trong trường là lên sổ đầu bài đó\"",
                tensionImpact = 10,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "hungReputation", floatValue = -0.2f },
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "aggressorFocusOnPlayer", boolValue = true }
                }
            }
        };

        return node;
    }

    private DialougeSO CreateS6()
    {
        var node = CreateDialogueAsset("S6_DoiDauThang", "S6");
        node.npcText = "Bạn đứng chắn một phần giữa Hùng và Nam:\n\n\"Buông bạn tao ra đi.\"\n\nHùng nhìn bạn trừng trừng.";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Nhìn thẳng vào mắt Hùng, nói chậm rãi: \"Tao không muốn gây chuyện, nhưng mày làm hơi quá rồi đó\"",
                tensionImpact = 5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "playerReputation", floatValue = 0.2f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Hơi đẩy tay Hùng ra khỏi áo Nam",
                tensionImpact = 20,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "aggressorFocusOnPlayer", boolValue = true },
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "futureThreat", boolValue = true }
                },
                useConditionalBranching = true,
                conditionalBranches = new List<ConditionalBranch>
                {
                    new ConditionalBranch
                    {
                        conditionVariable = "tension",
                        comparison = ConditionalBranch.ComparisonType.GreaterThan,
                        compareWithVariable = false,
                        comparisonValue = 65
                    }
                }
            },
            new DialougeChoice
            {
                choiceText = "Rút lui nửa bước, chuyển sang giọng nhẹ nhàng hơn",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>()
            }
        };

        return node;
    }

    private DialougeSO CreateS7()
    {
        var node = CreateDialogueAsset("S7_DanhLacHuong", "S7");
        node.npcText = "Bạn bịa chuyện giáo viên tìm Hùng. Hùng hơi khựng lại, liếc quanh.";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Thêm: \"Hình như chuyện điếu thuốc trong nhà vệ sinh hôm qua\"",
                tensionImpact = 10,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "hungReputation", floatValue = -0.3f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Nhân lúc Hùng phân tâm, ra hiệu cho Nam lùi lại",
                tensionImpact = -10,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "victimSafe", boolValue = true },
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "victimTrust", floatValue = 0.2f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Khi Hùng hỏi: \"Thật không?\", bạn thú nhận \"Không… tao chỉ muốn mày dừng lại\"",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "teacherTrust", floatValue = 0.1f }
                }
            }
        };

        return node;
    }

    private DialougeSO CreateS8()
    {
        var node = CreateDialogueAsset("S8_ChiDungQuay", "S8");
        node.npcText = "Bạn tiếp tục quay. Hình ảnh trong khung hình càng lúc càng căng.";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Chỉ quay, sau đó định gửi cho Nam sau này",
                tensionImpact = 0,
                stateEffects = new List<StateEffect>()
            },
            new DialougeChoice
            {
                choiceText = "Dừng quay, gọi điện trực tiếp cho cô chủ nhiệm",
                tensionImpact = 0,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "calledHelp", boolValue = true },
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "teacherOnTheWay", boolValue = true }
                }
            },
            new DialougeChoice
            {
                choiceText = "Vừa quay vừa hét: \"Này đủ rồi đó Hùng!\"",
                tensionImpact = 10,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "recordingDiscovered", boolValue = true },
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "aggressorFocusOnPlayer", boolValue = true }
                }
            }
        };

        return node;
    }

    private DialougeSO CreateS9()
    {
        var node = CreateDialogueAsset("S9_PhoiHopVoiLinh", "S9");
        node.npcText = "Hai bạn chia việc, Linh chuẩn bị kéo Nam, bạn đối thoại với Hùng.";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Ra hiệu cho Linh kéo Nam khi bạn bắt đầu nói",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "victimSafe", boolValue = true },
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "victimTrust", floatValue = 0.3f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Đổi ý, bảo Linh đứng yên để tránh rối thêm, tự bạn nói riêng với Hùng",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>()
            }
        };

        return node;
    }

    private DialougeSO CreateS10()
    {
        var node = CreateDialogueAsset("S10_GoiNguoiLon", "S10");
        node.npcText = "Bạn gọi cô giám thị, nói vắn tắt:\n\n\"Hành lang A đang có vụ căng, cô ra giúp với.\"\n\nGiáo viên đang trên đường tới...";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Ở lại, tiếp tục nói chuyện để kéo thời gian",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>(),
                useConditionalBranching = true,
                conditionalBranches = new List<ConditionalBranch>
                {
                    new ConditionalBranch
                    {
                        conditionVariable = "tension",
                        comparison = ConditionalBranch.ComparisonType.LessThan,
                        compareWithVariable = false,
                        comparisonValue = 70
                    },
                    new ConditionalBranch
                    {
                        conditionVariable = "tension",
                        comparison = ConditionalBranch.ComparisonType.GreaterThan,
                        compareWithVariable = false,
                        comparisonValue = 69
                    }
                }
            },
            new DialougeChoice
            {
                choiceText = "Đứng xa quan sát",
                tensionImpact = 0,
                stateEffects = new List<StateEffect>()
            }
        };

        return node;
    }

    private DialougeSO CreateS11()
    {
        var node = CreateDialogueAsset("S11_LinhQuay", "S11");
        node.npcText = "Linh cầm máy quay, bạn khoanh tay đứng xem, cảnh tượng bắt đầu thu hút vài ánh nhìn xa xa.";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Thì thầm với Linh: \"Đừng đăng lên mạng, chỉ để làm bằng chứng thôi\"",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "rumorLevel", floatValue = 0.1f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Hào hứng: \"Để lát gửi cho nhóm chat coi, tụi nó sốc luôn\"",
                tensionImpact = 0,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "socialMediaPosted", boolValue = true }
                }
            }
        };

        return node;
    }

    private DialougeSO CreateS12()
    {
        var node = CreateDialogueAsset("S12_NgheHungKe", "S12");
        node.npcText = "Bạn cho Hùng cơ hội nói:\n\n\"Rồi, kể tao nghe chuyện gì trước đã.\"\n\nHùng bắt đầu xả: Nam bị nghi là đã kể chuyện Hùng hút thuốc.";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Thừa nhận: \"Nếu là tao, tao cũng tức, nhưng đánh người thì…\"",
                tensionImpact = -15,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "hungReputation", floatValue = 0.2f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Đổi câu chuyện sang hướng \"tụi mình cùng đang căng vì thi cử, đừng trút lên nhau\"",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "playerReputation", floatValue = 0.1f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Buột miệng: \"Thì mày hút thì chịu, mắc gì đổ cho người khác?\"",
                tensionImpact = 10,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "hungReputation", floatValue = -0.2f },
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "aggressorFocusOnPlayer", boolValue = true }
                }
            }
        };

        return node;
    }

    private DialougeSO CreateS13()
    {
        var node = CreateDialogueAsset("S13_DungVePhiaNam", "S13");
        node.npcText = "Bạn hỏi Nam ngay trước mặt Hùng:\n\n\"Mày muốn tao ở lại không?\"\n\nNam nhỏ giọng: \"Đừng đi…\"";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Nói rõ: \"Được, tao ở đây cho tới khi mày thấy an toàn đã\"",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "victimTrust", floatValue = 0.4f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Nháy mắt với Nam, ra hiệu lát gặp ở cầu thang sau để nói riêng",
                tensionImpact = 0,
                stateEffects = new List<StateEffect>()
            }
        };

        return node;
    }

    private DialougeSO CreateS14()
    {
        var node = CreateDialogueAsset("S14_LoLoiDoaKyLuat", "S14");
        node.npcText = "Bạn nhắc tới chuyện sổ đầu bài, kỷ luật. Hùng nhíu mày, giọng gắt hơn.";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Nhận ra mình hơi lố, nhanh chóng đổi lại: \"Ý tao là… đừng để tụi mình dính rắc rối thêm\"",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "playerReputation", floatValue = 0.1f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Vẫn giữ thái độ \"tao nói đúng luật mà\", không đổi giọng",
                tensionImpact = 10,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "futureThreat", boolValue = true }
                },
                useConditionalBranching = true,
                conditionalBranches = new List<ConditionalBranch>
                {
                    new ConditionalBranch
                    {
                        conditionVariable = "tension",
                        comparison = ConditionalBranch.ComparisonType.GreaterThan,
                        compareWithVariable = false,
                        comparisonValue = 75
                    }
                }
            }
        };

        return node;
    }

    private DialougeSO CreateS15()
    {
        var node = CreateDialogueAsset("S15_GiuLapTruong", "S15");
        node.npcText = "Bạn cố giữ vững quan điểm \"dừng lại\" nhưng tránh chọc vào tự ái của Hùng.";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Đề nghị cả ba ra khỏi hành lang, tìm chỗ ít người nói chuyện",
                tensionImpact = -10,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "victimSafe", boolValue = true }
                }
            },
            new DialougeChoice
            {
                choiceText = "Nói: \"Nếu mày vẫn muốn nói tiếp, cứ nói, nhưng tay mày bỏ khỏi cổ áo bạn tao trước đã\"",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "aggressorFocusOnPlayer", boolValue = true }
                }
            }
        };

        return node;
    }

    private DialougeSO CreateS16()
    {
        var node = CreateDialogueAsset("S16_KeoDuocNamRa", "S16");
        node.npcText = "Nhờ đánh lạc hướng, Nam bước lùi về phía bạn, mặt vẫn còn tái.";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "\"Mày muốn đi khỏi đây luôn không? Tao đi với mày\"",
                tensionImpact = 0,
                stateEffects = new List<StateEffect>()
            },
            new DialougeChoice
            {
                choiceText = "\"Đứng đây với tao, để tao nói chuyện với Hùng chút\"",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>()
            }
        };

        return node;
    }

    private DialougeSO CreateS17()
    {
        var node = CreateDialogueAsset("S17_BaoLucNoRa", "S17");
        node.isEndingNode = true;
        node.isGoodEnding = false;
        node.endingTitle = "Bạo lực nổ ra";
        node.npcText = "Không khí vỡ vụn. Có xô đẩy, có người ngã...\n\n⚠️ Mức căng thẳng đã vượt ngưỡng nguy hiểm!";
        node.choices = new List<DialougeChoice>();
        return node;
    }

    private DialougeSO CreateS18()
    {
        var node = CreateDialogueAsset("S18_NguoiLonXuatHienKip", "S18");
        node.isEndingNode = true;
        node.isGoodEnding = true;
        node.endingTitle = "Can thiệp người lớn";
        node.npcText = "Cô giám thị đi tới, nhìn thấy cảnh Hùng đang dí sát Nam.\n\nCô giáo: \"Chuyện gì đây? Các em theo cô lên phòng!\"";
        node.choices = new List<DialougeChoice>();
        return node;
    }

    private DialougeSO CreateS19()
    {
        var node = CreateDialogueAsset("S19_CongNhanCamXuc", "S19");
        node.npcText = "Bạn nói:\n\n\"Tao hiểu bị nghi là 'chơi dơ' khó chịu lắm. Nhưng nếu mày đánh người, mọi người chỉ thấy mày là thằng bạo lực thôi.\"";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Đề nghị tạm dừng hôm nay, hẹn Hùng và Nam nói chuyện với giáo viên cố vấn sau",
                tensionImpact = -15,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "hungReputation", floatValue = 0.1f },
                    new StateEffect { effectType = StateEffect.EffectType.ModifyFloat, variableName = "teacherTrust", floatValue = 0.3f }
                }
            },
            new DialougeChoice
            {
                choiceText = "Đề nghị để Nam đi trước, bạn ở lại nói riêng thêm với Hùng",
                tensionImpact = -10,
                stateEffects = new List<StateEffect>
                {
                    new StateEffect { effectType = StateEffect.EffectType.SetBool, variableName = "victimSafe", boolValue = true }
                }
            }
        };

        return node;
    }

    private DialougeSO CreateS20()
    {
        var node = CreateDialogueAsset("S20_NoiChuyenOChoKhac", "S20");
        node.npcText = "Cả nhóm di chuyển sang chỗ ít người, không còn cảnh \"dí vào tường\". Căng thẳng giảm, nhưng cảm xúc vẫn còn.";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Khuyến khích Hùng nói hết và Nam nghe, bạn đóng vai người \"giữ luật chơi\"",
                tensionImpact = -10,
                stateEffects = new List<StateEffect>()
            },
            new DialougeChoice
            {
                choiceText = "Đề nghị dừng tại đây, ai về lớp nấy, bạn hẹn Nam sau",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>()
            }
        };

        return node;
    }

    private DialougeSO CreateS21()
    {
        var node = CreateDialogueAsset("S21_DatDieuKienRoRang", "S21");
        node.npcText = "Bạn nói:\n\n\"Muốn nói gì thì nói, nhưng tay bỏ khỏi người ta trước đã.\"\n\nHùng gằn giọng, nhưng từ từ buông tay.";

        node.choices = new List<DialougeChoice>
        {
            new DialougeChoice
            {
                choiceText = "Sau khi Hùng buông, bạn nhắc Nam: \"Không ổn thì nói hiệu cho tao liền, tao gọi cô ngay\"",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>()
            },
            new DialougeChoice
            {
                choiceText = "Sau khi Hùng buông, bạn rút nhẹ Nam ra sau, coi như kết thúc",
                tensionImpact = -5,
                stateEffects = new List<StateEffect>()
            }
        };

        return node;
    }

    #endregion

    #region Ending Nodes - ALL WITH PROPER METADATA

    private DialougeSO CreateE0()
    {
        var node = CreateDialogueAsset("E0_BoDi", "E0");
        node.isEndingNode = true;
        node.isGoodEnding = false;
        node.endingTitle = "Bỏ đi";
        node.npcText = "Bạn đã chọn không xen vào.\nNam nhớ rất rõ việc bạn đã nhìn thấy mà vẫn quay lưng.";
        node.choices = new List<DialougeChoice>();
        return node;
    }

    private DialougeSO CreateE1()
    {
        var node = CreateDialogueAsset("E1_ImLangLamNhanChung", "E1");
        node.isEndingNode = true;
        node.isGoodEnding = false;
        node.endingTitle = "Im lặng làm nhân chứng";
        node.npcText = "Bạn quay lại toàn bộ nhưng không can thiệp.\nClip có thể giúp về sau, nhưng hôm đó Nam vẫn phải tự chịu trận.";
        node.choices = new List<DialougeChoice>();
        return node;
    }

    private DialougeSO CreateE2()
    {
        var node = CreateDialogueAsset("E2_HenGapSauDeHoTro", "E2");
        node.isEndingNode = true;
        node.isGoodEnding = true;
        node.endingTitle = "Hẹn gặp sau để hỗ trợ";
        node.npcText = "Bạn không giải quyết trọn vẹn ngay lúc đó,\nnhưng chủ động làm chỗ dựa cho Nam sau này.";
        node.choices = new List<DialougeChoice>();
        return node;
    }

    private DialougeSO CreateE3()
    {
        var node = CreateDialogueAsset("E3_RutLuiCungNanNhan", "E3");
        node.isEndingNode = true;
        node.isGoodEnding = true;
        node.endingTitle = "Rút lui cùng nạn nhân";
        node.npcText = "Bạn đưa Nam ra khỏi chỗ nguy hiểm.\nCâu chuyện với Hùng chưa được giải quyết, có nguy cơ bùng lại sau.";
        node.choices = new List<DialougeChoice>();
        return node;
    }

    private DialougeSO CreateE4()
    {
        var node = CreateDialogueAsset("E4_BaoLucBungNo", "E4");
        node.isEndingNode = true;
        node.isGoodEnding = false;
        node.endingTitle = "Bạo lực bùng nổ";
        node.npcText = "Tension vượt ngưỡng! Bạo lực đã xảy ra.\nNam đã bị đánh.";
        node.choices = new List<DialougeChoice>();
        return node;
    }

    private DialougeSO CreateE5()
    {
        var node = CreateDialogueAsset("E5_CanThiepKipThoi", "E5");
        node.isEndingNode = true;
        node.isGoodEnding = true;
        node.endingTitle = "Can thiệp kịp thời";
        node.npcText = "Giáo viên đã can thiệp kịp thời! Không ai bị đánh,\nmọi thứ chuyển sang 'họp xử lý' ở cấp nhà trường.";
        node.choices = new List<DialougeChoice>();
        return node;
    }

    private DialougeSO CreateE6()
    {
        var node = CreateDialogueAsset("E6_DramaMangXaHoi", "E6");
        node.isEndingNode = true;
        node.isGoodEnding = false;
        node.endingTitle = "Drama mạng xã hội";
        node.npcText = "Clip bị đăng lên mạng! Cả trường bàn tán.\nHùng bị bêu xấu, Nam bị 'coi như drama',\ncòn bạn mang tiếng 'thích quay drama'.";
        node.choices = new List<DialougeChoice>();
        return node;
    }

    private DialougeSO CreateE7()
    {
        var node = CreateDialogueAsset("E7_HoaGiaiDanHuong", "E7");
        node.isEndingNode = true;
        node.isGoodEnding = true;
        node.endingTitle = "Hòa giải dẫn hướng";
        node.npcText = "Bạn kéo câu chuyện về hướng 'hẹn gặp giáo viên cố vấn'.\nHùng bớt nóng và chịu thử nói chuyện đàng hoàng.";
        node.choices = new List<DialougeChoice>();
        return node;
    }

    private DialougeSO CreateE8()
    {
        var node = CreateDialogueAsset("E8_VongTronNoiChuyen", "E8");
        node.isEndingNode = true;
        node.isGoodEnding = true;
        node.endingTitle = "Vòng tròn nói chuyện";
        node.npcText = "Cả ba chuyển sang ngồi nói chuyện ở chỗ yên tĩnh.\nKhông đánh nhau, nhưng bạn vẫn phải điều phối cảm xúc hai bên.";
        node.choices = new List<DialougeChoice>();
        return node;
    }

    #endregion

    #region Helper Methods

    private DialougeSO CreateDialogueAsset(string fileName, string nodeId)
    {
        string path = $"{assetPath}/{fileName}.asset";

        DialougeSO asset = AssetDatabase.LoadAssetAtPath<DialougeSO>(path);

        if (asset != null && !overwriteExisting)
        {
            Debug.Log($"⏭️ Bỏ qua {fileName} (đã tồn tại)");
            return asset;
        }

        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<DialougeSO>();
            AssetDatabase.CreateAsset(asset, path);
        }

        asset.nodeId = nodeId;
        EditorUtility.SetDirty(asset);

        return asset;
    }

    private void LinkNodes(Dictionary<string, DialougeSO> nodes)
    {
        // S0 choices
        nodes["S0"].choices[0].nextNode = nodes["S1"];
        nodes["S0"].choices[1].nextNode = nodes["S2"];
        nodes["S0"].choices[2].nextNode = nodes["S3"];
        nodes["S0"].choices[3].nextNode = nodes["E0"];

        // S1 choices
        nodes["S1"].choices[0].nextNode = nodes["S3"];
        nodes["S1"].choices[1].nextNode = nodes["S4"];
        nodes["S1"].choices[2].nextNode = nodes["E0"];

        // S2 choices
        nodes["S2"].choices[0].nextNode = nodes["S5"];
        nodes["S2"].choices[1].nextNode = nodes["S6"];
        nodes["S2"].choices[1].conditionalBranches[0].targetNode = nodes["S17"];
        nodes["S2"].choices[2].nextNode = nodes["S7"];

        // S3 choices
        nodes["S3"].choices[0].nextNode = nodes["S8"];
        nodes["S3"].choices[1].nextNode = nodes["S4"];
        nodes["S3"].choices[2].nextNode = nodes["S5"];

        // S4 choices
        nodes["S4"].choices[0].nextNode = nodes["S9"];
        nodes["S4"].choices[1].nextNode = nodes["S10"];
        nodes["S4"].choices[2].nextNode = nodes["S11"];

        // S5 choices
        nodes["S5"].choices[0].nextNode = nodes["S12"];
        nodes["S5"].choices[1].nextNode = nodes["S13"];
        nodes["S5"].choices[2].nextNode = nodes["S14"];

        // S6 choices
        nodes["S6"].choices[0].nextNode = nodes["S15"];
        nodes["S6"].choices[1].nextNode = nodes["S17"];
        nodes["S6"].choices[1].conditionalBranches[0].targetNode = nodes["S17"];
        nodes["S6"].choices[2].nextNode = nodes["S12"];

        // S7 choices
        nodes["S7"].choices[0].nextNode = nodes["S14"];
        nodes["S7"].choices[1].nextNode = nodes["S16"];
        nodes["S7"].choices[2].nextNode = nodes["S12"];

        // S8 choices
        nodes["S8"].choices[0].nextNode = nodes["E1"];
        nodes["S8"].choices[1].nextNode = nodes["S10"];
        nodes["S8"].choices[2].nextNode = nodes["S6"];

        // S9 choices
        nodes["S9"].choices[0].nextNode = nodes["S16"];
        nodes["S9"].choices[1].nextNode = nodes["S12"];

        // S10 choices
        nodes["S10"].choices[0].conditionalBranches[0].targetNode = nodes["S18"];
        nodes["S10"].choices[0].conditionalBranches[1].targetNode = nodes["S17"];
        nodes["S10"].choices[0].nextNode = nodes["S18"];
        nodes["S10"].choices[1].nextNode = nodes["S8"];

        // S11 choices
        nodes["S11"].choices[0].nextNode = nodes["S12"];
        nodes["S11"].choices[1].nextNode = null; // Triggers E6 via state

        // S12 choices
        nodes["S12"].choices[0].nextNode = nodes["S19"];
        nodes["S12"].choices[1].nextNode = nodes["S15"];
        nodes["S12"].choices[2].nextNode = nodes["S14"];

        // S13 choices
        nodes["S13"].choices[0].nextNode = nodes["S15"];
        nodes["S13"].choices[1].nextNode = nodes["E2"];

        // S14 choices
        nodes["S14"].choices[0].nextNode = nodes["S15"];
        nodes["S14"].choices[1].conditionalBranches[0].targetNode = nodes["S17"];
        nodes["S14"].choices[1].nextNode = nodes["S15"];

        // S15 choices
        nodes["S15"].choices[0].nextNode = nodes["S20"];
        nodes["S15"].choices[1].nextNode = nodes["S21"];

        // S16 choices
        nodes["S16"].choices[0].nextNode = nodes["E3"];
        nodes["S16"].choices[1].nextNode = nodes["S15"];

        // S19 choices
        nodes["S19"].choices[0].nextNode = nodes["E7"];
        nodes["S19"].choices[1].nextNode = nodes["S20"];

        // S20 choices
        nodes["S20"].choices[0].nextNode = nodes["E8"];
        nodes["S20"].choices[1].nextNode = nodes["E2"];

        // S21 choices
        nodes["S21"].choices[0].nextNode = nodes["E2"];
        nodes["S21"].choices[1].nextNode = nodes["E3"];

        // Mark all as dirty
        foreach (var node in nodes.Values)
        {
            EditorUtility.SetDirty(node);
        }
    }

    #endregion
}
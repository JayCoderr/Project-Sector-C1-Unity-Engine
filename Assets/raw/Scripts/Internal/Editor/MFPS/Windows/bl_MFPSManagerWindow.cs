//#define AUTO_REFRESH
using MFPS.Internal.Structures;
using MFPSEditor;
using MFPSEditor.Addons;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// 
/// </summary>
public class bl_MFPSManagerWindow : EditorWindow
{
    #region Parameters
    public static Color primaryColor = new(0.098f, 0.098f, 0.098f, 1.00f);
    public static Color altColor = new(0.07f, 0.07f, 0.07f, 1.00f);
    public static Color altColorLight = new(0.5411765f, 0.5411765f, 0.5411765f, 1.00f);
    public static Color hightlightColor = new(1f, 0.9882353f, 0.003921569f, 1.00f);
    public static Color whiteColor = new(0.754717f, 0.754717f, 0.754717f, 1.00f);

    private readonly List<ManagerPanel> managerPanels = new()
    {
    new ManagerPanel(){Name = "Game Data", BodyFuncName = nameof(DrawGameDataSettings)},
    new ManagerPanel(){Name = "ULogin Pro", BodyFuncName = nameof(DrawULogin)},
    new ManagerPanel(){Name = "Player Selector", BodyFuncName = nameof(DrawPlayerSelector)},
    new ManagerPanel(){Name = "Shop", BodyFuncName = nameof(DrawShop)},
    new ManagerPanel(){Name = "Class Customization", BodyFuncName = nameof(DrawClassCustomizer)},
    new ManagerPanel(){Name = "Customizer", BodyFuncName = nameof(DrawCustomizer)},
    new ManagerPanel(){Name = "Anti-Cheat", BodyFuncName = nameof(DrawAntiCheat)},
    new ManagerPanel(){Name = "Vehicles", BodyFuncName = nameof(DrawVehicles)},
    new ManagerPanel(){Name = "Mobile", BodyFuncName = nameof(DrawMobileControl)},
    new ManagerPanel(){Name = "Level Manager", BodyFuncName = nameof(DrawlevelManager)},
    new ManagerPanel(){Name = "Input Manager", BodyFuncName = nameof(DrawInputManager)},
    new ManagerPanel(){Name = "Kill Streaks", BodyFuncName = nameof(DrawKillStreaks)},
    new ManagerPanel(){Name = "Third Person", BodyFuncName = nameof(DrawThirdPerson)},
    new ManagerPanel(){Name = "Minimap", BodyFuncName = nameof(DrawMinimap)},
    new ManagerPanel(){Name = "Clan System", BodyFuncName = nameof(DrawClan)},
    new ManagerPanel(){Name = "Floating Text", BodyFuncName = nameof(DrawFloatingText)},
    new ManagerPanel(){Name = "Emblems and Cards", BodyFuncName = nameof(DrawEmblems)},
    new ManagerPanel(){Name = "Game News", BodyFuncName = nameof(DrawGameNews)},
    new ManagerPanel(){Name = "Layout Customizer", BodyFuncName = nameof(DrawLayoutCustomizer)},
    };
    private WindowType currentWindow = WindowType.Home;
    private string currentPanelType = "";
    public ManagerPanel currentPanel = null;
    public Dictionary<string, GUIStyle> styles = new() { { "panelButton", null }, { "titleH2", null }, { "borders", null }, { "textC", null }, { "miniText", null }, { "text", null } };
    public bool m_initGUI = false;
    private bl_GameData gameData;
    private Vector2 bodyScroll = Vector2.zero;
    private Texture2D mfpsLogo, soldierIcon, discordIcon;
    SearchField weaponSearchfield, managerSearchField;
    private string weaponSearchKey, managerSearchKey = "";
    readonly string[] slotsNames = new string[] { "Assault", "Recon", "Support", "Engineer" };
    public int currentSlot, currentSlot2 = 0;
    private bl_PlayerClassLoadout selectedLoadout = null;
    private int selectedSlot = -1;
    private Vector2 weaponScroll, managersScroll = Vector2.zero;
    private PlayerClass currentPlayerClass = PlayerClass.Assault;
    private readonly Dictionary<ScriptableObject, Editor> cachedEditors = new();
    public int editWeaponId = -1;
    public bl_GunInfo editGunInfo;
    public GameModeSettings selectedGameMode = null;
    public SerializedObject serializedObject;
    private Rect windowRect;
    private Type cachedType;
    private GunType filterWeaponType = GunType.None;
    private int currentWSelectionId = -1;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        serializedObject = new SerializedObject(this);
        titleContent = new GUIContent(" MFPS", GetUnityIcon("Audio Mixer"));
        mfpsLogo = Resources.Load("content/Images/mfps-name", typeof(Texture2D)) as Texture2D;
        soldierIcon = AssetDatabase.LoadAssetAtPath("Assets/MFPS/Content/Art/UI/Icons/mfps-soldier.png", typeof(Texture2D)) as Texture2D;
        discordIcon = AssetDatabase.LoadAssetAtPath("Assets/MFPS/Scripts/Internal/Editor/MFPS/Resources/content/Images/discord_l_editor.png", typeof(Texture2D)) as Texture2D;
        minSize = new Vector2(720, 570);
        weaponSearchfield = new SearchField();
        managerSearchField = new SearchField();
        LovattoStats.SetStat("mfps-manager-window", 1);
        currentPlayerClass = currentPlayerClass.GetSavePlayerClass();
        currentSlot = currentSlot2 = (int)currentPlayerClass;
        cachedType = this.GetType();
        FetchCustomTabs();
        selectedGameMode = null;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnGUI()
    {
        var cColor = GUI.contentColor;
        GUI.contentColor = whiteColor;

        windowRect = position;
        EditorGUI.DrawRect(new Rect(0, 0, position.width, position.height), EditorGUIUtility.isProSkin ? altColor : altColorLight);
        if (!m_initGUI || styles["panelButton"] == null) InitGUI();

        GUILayout.BeginHorizontal();
        LeftPanel();
        EditorGUILayout.BeginVertical();
        Header();

        GUILayout.BeginHorizontal();
        GUILayout.Space(20);
        EditorGUILayout.BeginVertical();
        DrawBody();
        EditorGUILayout.EndVertical();
        GUILayout.Space(20);
        GUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUI.contentColor = cColor;

#if AUTO_REFRESH
        if (!Application.isPlaying) Repaint();
#endif

    }

    /// <summary>
    /// 
    /// </summary>
    void Header()
    {
        Rect r = EditorGUILayout.BeginHorizontal(GUILayout.Height(60));
        EditorGUI.DrawRect(r, primaryColor);
        GUILayout.FlexibleSpace();
        DrawWindowButtons();
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawWindowButtons()
    {
        WindowButton("MANAGERS", WindowType.Managers);
        WindowButton("WEAPONS", WindowType.Weapons);
        WindowButton("LOADOUTS", WindowType.Loadouts);
        WindowButton("GAME MODES", WindowType.GameModes);
        WindowButton("MAPS", WindowType.Maps);
#if LM
        WindowButton("LEVELS", WindowType.Levels);
#endif
        var r = GUILayoutUtility.GetRect(new GUIContent("HOME"), styles["textC"], GUILayout.Height(30), GUILayout.Width(40));
        r.y += 30;
        GUI.DrawTexture(r, discordIcon, ScaleMode.ScaleToFit);
        if (GUI.Button(r, GUIContent.none, GUIStyle.none))
        {
            Application.OpenURL("https://lovattostudio.com/u/discord");
            LovattoStats.SetStat("mfps-manager-window-discord", 1);
        }
        GUILayout.Space(25);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="title"></param>
    /// <param name="windowType"></param>
    void WindowButton(string title, WindowType windowType)
    {
        Rect r = GUILayoutUtility.GetRect(new GUIContent(title), styles["textC"], GUILayout.Height(30), GUILayout.Width(100));
        r.y += 30;
        if (currentWindow == windowType)
        {
            EditorGUI.DrawRect(r, altColor);
            title = $"<b>{title}</b>";
        }
        else
        {
            EditorGUI.DrawRect(r, GetHexColor("#12121255"));
        }
        if (GUI.Button(r, title, styles["textC"]))
        {
            currentWindow = windowType;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void LeftPanel()
    {
        Rect r = EditorGUILayout.BeginVertical(GUILayout.Width(150));
        EditorGUI.DrawRect(r, primaryColor);
        GUILayout.Label(mfpsLogo, GUILayout.Height(58), GUILayout.Width(150));
        DrawBottomLine(0.2f);
        if (currentWindow == WindowType.Managers || currentWindow == WindowType.Home)
            DrawManagerButtons();
        if (currentWindow == WindowType.Weapons) DrawWeaponPanel();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawBody()
    {
        GUILayout.Space(10);
        bodyScroll = GUILayout.BeginScrollView(bodyScroll, false, false);
        if (currentWindow == WindowType.Managers) DrawManagersBody();
        else if (currentWindow == WindowType.Weapons) DrawWeaponsBody();
        else if (currentWindow == WindowType.Loadouts) DrawLoadoutsBody();
        else if (currentWindow == WindowType.Home) DrawHomeBody();
        else if (currentWindow == WindowType.GameModes) DrawGameModes();
        else if (currentWindow == WindowType.Maps) DrawMaps();
#if LM
        else if (currentWindow == WindowType.Levels) LevelManagerWindowEditor.DrawLevels();
#endif
        GUILayout.FlexibleSpace();
        GUILayout.Space(100);
        GUILayout.EndScrollView();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawHomeBody()
    {
        GUILayout.Label("SELECT THE GAME SETTING WINDOW", styles["textC"]);
        GUILayout.Space(20);
        Rect r;
        for (int i = 0; i < managerPanels.Count; i++)
        {
            r = GUILayoutUtility.GetRect(new GUIContent(managerPanels[i].Name), styles["panelButton"], GUILayout.Height(30));
            if (currentPanelType == managerPanels[i].Name)
            {
                // EditorGUI.DrawRect(r, altColor);
                float w = r.width;
                r.width = 1;
                EditorGUI.DrawRect(r, hightlightColor);
                r.width = w;
            }
            else
            {
                EditorGUI.DrawRect(r, primaryColor);
            }
            if (GUI.Button(r, managerPanels[i].Name, styles["panelButton"]))
            {
                bodyScroll = Vector2.zero;
                currentPanelType = managerPanels[i].Name;
                currentPanel = managerPanels[i];
                currentWindow = WindowType.Managers;
            }
            GUILayout.Space(4);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawManagersBody()
    {
        if (currentPanel == null) return;

        currentPanel.DrawBody(cachedType, this);
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawWeaponsBody()
    {
        GUILayout.Space(10);
        if (editWeaponId != -1)
        {
            DrawEditWeapon();
            return;
        }

        float lw = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 75;
        var sls = GUI.skin.horizontalSlider;
        GUI.skin.horizontalSlider = MFPSEditorStyles.EditorSkin.customStyles[9];
        var slt = GUI.skin.horizontalSliderThumb;
        GUI.skin.horizontalSliderThumb = MFPSEditorStyles.EditorSkin.customStyles[10];

        var all = bl_GameData.Instance.AllWeapons;
        int rowID = 0;
        int SpaceBetweenRow = 20;
        int rowCapacity = Mathf.FloorToInt((position.width - 150) / (370 + SpaceBetweenRow));
        for (int i = 0; i < all.Count; i++)
        {
            if (!string.IsNullOrEmpty(weaponSearchKey))
            {
                if (all[i].Name.ToLower().Contains(weaponSearchKey.ToLower()))
                {
                    WeaponCard(all[i], i);
                    GUILayout.Space(SpaceBetweenRow);
                    continue;
                }
                else continue;
            }

            if (filterWeaponType != GunType.None)
            {
                if (all[i].Type == filterWeaponType)
                {
                    WeaponCard(all[i], i);
                    GUILayout.Space(SpaceBetweenRow);
                    continue;
                }
                else continue;
            }

            if (rowID == 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }
            WeaponCard(all[i], i);
            GUILayout.Space(10);
            if (rowID == (rowCapacity - 1) || i == all.Count - 1)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(SpaceBetweenRow);
            }
            rowID = (rowID + 1) % rowCapacity;
        }

        EditorGUIUtility.labelWidth = lw;
        GUI.skin.horizontalSlider = sls;
        GUI.skin.horizontalSliderThumb = slt;
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawEditWeapon()
    {
        serializedObject.UpdateIfRequiredOrScript();

        GUILayout.Space(18);
        var w = editGunInfo;

        DrawTitleText("General");
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Space(20);
            Rect iconR = GUILayoutUtility.GetRect(120, 70);
            if (w.GunIcon != null)
            {
                GUI.DrawTexture(iconR, w.GunIcon.texture, ScaleMode.ScaleToFit);
            }
            else
            {
                EditorGUI.DrawRect(iconR, Color.black);
                GUI.Label(iconR, "No Icon", MFPSEditorStyles.EditorSkin.label);
            }
            GUILayout.Space(20);
            w.GunIcon = EditorGUILayout.ObjectField(w.GunIcon, typeof(Sprite), false, GUILayout.Width(120), GUILayout.Height(70)) as Sprite;
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(18);

        w.Name = EditorGUILayout.TextField("Weapon Name", w.Name);
        w.Type = (GunType)EditorGUILayout.EnumPopup("Weapon Type", w.Type);

        GUILayout.Space(18);
        DrawTitleText("Specs");

        w.Damage = EditorGUILayout.IntField("Damage", w.Damage);
        EditorGUILayout.BeginHorizontal();
        w.FireRate = EditorGUILayout.FloatField(new GUIContent("Fire Rate", "Average Rate per weapon type:\n\nAR/Machineguns: 0.08 - 0.11\nPistols: 0.11 - 0.25\nShotguns: 1.0 - 3.0\nSnipers: 1.0 - 3.0\nMelee: 0.5 - 2.0\nGrenades: 2.0 - 4.0"), w.FireRate);
        if (w.FireRate > 0)
        {
            int rpm = Mathf.FloorToInt(60 / w.FireRate);
            DrawText(new GUIContent($"RPM: {rpm}", "Estimated Rounds Per Minute"), GUILayout.Width(100));
        }
        EditorGUILayout.EndHorizontal();
        w.ReloadTime = EditorGUILayout.FloatField("Reload Time", w.ReloadTime);
        w.Range = EditorGUILayout.IntField("Range", w.Range);
        w.Accuracy = EditorGUILayout.Slider("Accuracy", w.Accuracy, 0, 5);
        w.Weight = EditorGUILayout.Slider("Weight", w.Weight, 0, 4);

        GUILayout.Space(18);
        DrawTitleText("Unlockability");

        EditorGUILayout.PropertyField(serializedObject.FindProperty("editGunInfo").FindPropertyRelative("Unlockability"), true);

        GUILayout.Space(18);
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cancel", MFPSEditorStyles.EditorSkin.customStyles[11], GUILayout.Width(100)))
            {
                editWeaponId = -1;
                editGunInfo = null;
            }
            GUILayout.Space(10);
            string btnText = editWeaponId == 998 ? "Save New" : "Save";
            if (GUILayout.Button(btnText, MFPSEditorStyles.EditorSkin.customStyles[11], GUILayout.Width(110)))
            {
                if (editWeaponId == 998)
                {
                    int index = bl_GameData.Instance.AllWeapons.FindIndex(x => x.Name == editGunInfo.Name);
                    if (index != -1)
                    {
                        Debug.LogWarning($"The weapon {editGunInfo.Name} already exist, please change the name to save it as new weapon.");
                    }
                    else
                    {
                        bl_GameData.Instance.AllWeapons.Add(editGunInfo);
                    }
                }

                EditorUtility.SetDirty(bl_GameData.Instance);
                editWeaponId = -1;
                editGunInfo = null;
            }
            GUILayout.Space(18);
        }
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawLoadoutsBody()
    {
        GUILayout.Space(10);
#if !CLASS_CUSTOMIZER
        var player = bl_GameData.Instance.Player1.PlayerReferences;
        DrawPlayerLoadout(player, ref currentSlot);
        GUILayout.Space(15);
        player = bl_GameData.Instance.Player2.PlayerReferences;
        DrawPlayerLoadout(player, ref currentSlot2);
#else
        GUILayout.Label("<b>Class Customizer is enabled</b>\n<b><size=8><i>The player loadout is managed for a global loadouts that can be change in game instead of a fixed loadout per player prefab (MFPS default)</i></size></b>\n", styles["textC"]);
        GUILayout.Space(10);
        Rect r = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        {
            EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
            //Slot buttons
            using (new GUILayout.HorizontalScope(GUILayout.Height(30)))
            {
                GUILayout.Space(5);
                for (int i = 0; i < slotsNames.Length; i++)
                {
                    Rect br = GUILayoutUtility.GetRect(new GUIContent(slotsNames[i]), styles["textC"], GUILayout.Height(30));
                    if (i != currentSlot)
                        EditorGUI.DrawRect(br, primaryColor);
                    else
                    {
                        EditorGUI.DrawRect(br, altColor);
                        GUI.color = new Color(1, 1, 1, 0.2f);
                        GUI.Box(br, GUIContent.none, MFPSEditorStyles.OutlineButtonStyle);
                        GUI.color = Color.white;
                    }
                    if (GUI.Button(br, slotsNames[i], styles["textC"]))
                    {
                        if (currentSlot != i)
                        {
                            selectedLoadout = null;
                            selectedSlot = -1;
                        }
                        currentSlot = i;
                    }
                }
                GUILayout.Space(5);
            }
            GUILayout.Space(20);

            var loadout = bl_ClassManager.Instance.DefaultAssaultClass;
            if (currentSlot == 1) loadout = bl_ClassManager.Instance.DefaultReconClass;
            if (currentSlot == 2) loadout = bl_ClassManager.Instance.DefaultSupportClass;
            if (currentSlot == 3) loadout = bl_ClassManager.Instance.DefaultEngineerClass;
            //loadout
            DrawSlots(loadout);

        }
        EditorGUILayout.EndVertical();
#endif
        GUILayout.Space(10);
        var latpc = currentPlayerClass;
        currentPlayerClass = (PlayerClass)EditorGUILayout.EnumPopup("Current Player Class", currentPlayerClass);
        if (currentPlayerClass != latpc)
        {
            currentPlayerClass.SavePlayerClass();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawGameModes()
    {
        GUILayout.Space(10);

        if (selectedGameMode != null && !string.IsNullOrEmpty(selectedGameMode.ModeName))
        {
            DrawGameModeEdit();
            return;
        }

        float areaWidth = position.width - 160;
        float rowWidth = 160;
        int horizontalRows = Mathf.FloorToInt(areaWidth / rowWidth);

        var gameModes = bl_GameData.Instance.gameModes;
        int hRow = 0;
        for (int i = 0; i < gameModes.Count; i++)
        {
            if (hRow == 0) EditorGUILayout.BeginHorizontal();

            DrawGameModeBox(gameModes[i]);
            GUILayout.Space(10);
            hRow++;
            if (hRow >= horizontalRows || i == gameModes.Count - 1)
            {
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(10);
                hRow = 0;
            }

        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawGameModeBox(GameModeSettings gameMode)
    {
        var r = EditorGUILayout.BeginVertical(GUILayout.Width(150), GUILayout.Height(200));
        EditorGUI.DrawRect(r, primaryColor);

        var sr = r;
        GUILayout.Space(5);
        GUILayout.Label($"<size=12>{gameMode.ModeName.ToUpper()}</size>\n<color=#606060FF><size=9>({gameMode.gameMode})</size></color>", styles["titleH2"]);

        GUILayout.FlexibleSpace();
        string teamMode = IsOneTeamMode(gameMode.gameMode) ? "One vs All" : "Team vs Team";
        DrawText($"<b><size=10>TEAM MODE</size></b>\n<color=#A4A4A4FF><size=9>{teamMode}</size></color>");

        int maxPlayers = 1;
        foreach (var item in gameMode.maxPlayers)
        {
            maxPlayers = Mathf.Max(maxPlayers, item);
        }
        DrawText($"<b><size=10>REQUIRED PLAYERS</size></b>\n<color=#A4A4A4FF><size=9>{gameMode.RequiredPlayersToStart} of up to {maxPlayers}</size></color>");
        GUILayout.Space(10);
        EditorGUILayout.EndVertical();

        sr.width -= 20;
        if (GUI.Button(sr, GUIContent.none, GUIStyle.none))
        {
            selectedGameMode = gameMode;
        }

        // active icon
        sr = r;
        sr.x = sr.x + sr.width - 16;
        sr.y += 4;
        sr.width = sr.height = 12;

        GUI.color = gameMode.isEnabled ? Color.green : Color.grey;
        GUI.DrawTexture(sr, GetUnityIcon("ColorPicker-HueRing-Thumb-Fill"), ScaleMode.ScaleToFit);
        GUI.color = Color.white;

        if (GUI.Button(sr, GUIContent.none, GUIStyle.none))
        {
            gameMode.isEnabled = !gameMode.isEnabled;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawGameModeEdit()
    {
        serializedObject.UpdateIfRequiredOrScript();

        EditorGUI.BeginChangeCheck();

        var m = selectedGameMode;
        GUILayout.Space(10);
        DrawTitleText("GAME MODE INFO");
        m.ModeName = EditorGUILayout.TextField("Game Mode Name", m.ModeName);
        m.gameMode = (GameMode)EditorGUILayout.EnumPopup("Mode Identifier", m.gameMode);
        var r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.isEnabled = MFPSEditorStyles.FeatureToogle(r, m.isEnabled, "Is Enabled");

        var lw = EditorGUIUtility.labelWidth;
        EditorGUIUtility.labelWidth = 220;

        GUILayout.Space(10);
        DrawTitleText("SETTINGS");
        m.GoalName = EditorGUILayout.TextField("Mode Goal Name", m.GoalName);
        m.RequiredPlayersToStart = EditorGUILayout.IntSlider("Required players to start", m.RequiredPlayersToStart, 1, 64);

        m.onRoundStartedSpawn = (GameModeSettings.OnRoundStartedSpawn)EditorGUILayout.EnumPopup("When round has started", m.onRoundStartedSpawn);
        m.overrideSpawnSelectionMode = (SpawnPointSelectionMode)EditorGUILayout.EnumPopup("Override Spawn Selection Mode", m.overrideSpawnSelectionMode);
        m.onPlayerDie = (GameModeSettings.OnPlayerDie)EditorGUILayout.EnumPopup("When local player dies", m.onPlayerDie);
        m.RoundModeAllowed = (GameModeSettings.RoundModeAllowedOptions)EditorGUILayout.EnumPopup("Allowed round modes", m.RoundModeAllowed);
        m.AfterFinishMatch = (GameModeSettings.AfterFinishMatchLogic)EditorGUILayout.EnumPopup("After Finish Match", m.AfterFinishMatch);
        m.OverrideKillCamMode = (KillCameraType)EditorGUILayout.EnumPopup("Override KillCam Mode", m.OverrideKillCamMode);
        m.playersTeamSelection = (GameModeSettings.PlayerAutoTeamSelection)EditorGUILayout.EnumPopup("Players Auto Team Selection", m.playersTeamSelection);
        m.botsTeamSelection = (GameModeSettings.PlayerAutoTeamSelection)EditorGUILayout.EnumPopup("Bots Auto Team Selection", m.botsTeamSelection);

        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.supportBots = MFPSEditorStyles.FeatureToogle(r, m.supportBots, "Support bots?");
        GUILayout.Space(2);
        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.AllowKillAssist = MFPSEditorStyles.FeatureToogle(r, m.AllowKillAssist, "Allow Kill Assist?");
        GUILayout.Space(2);
        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.AutoTeamSelection = MFPSEditorStyles.FeatureToogle(r, m.AutoTeamSelection, "Force auto team selection?");
        GUILayout.Space(2);
        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.allowedPickupWeapons = MFPSEditorStyles.FeatureToogle(r, m.allowedPickupWeapons, "Allow pick up weapons?");
        GUILayout.Space(2);
        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.AllowSpectateEnemies = MFPSEditorStyles.FeatureToogle(r, m.AllowSpectateEnemies, "Allow Spectate Enemies?");
        GUILayout.Space(2);
        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.AllowSingleWeaponTypeOnly = MFPSEditorStyles.FeatureToogle(r, m.AllowSingleWeaponTypeOnly, "Allow Single Weapon Type Modes?");
        GUILayout.Space(2);
        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.AllowSwitchTeam = MFPSEditorStyles.FeatureToogle(r, m.AllowSwitchTeam, "Allow Switch Teams?");
        GUILayout.Space(2);
        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.ShowKillFeed = MFPSEditorStyles.FeatureToogle(r, m.ShowKillFeed, "Show KillFeed?");
        GUILayout.Space(2);
        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.UnlistGameAfterStarted = MFPSEditorStyles.FeatureToogle(r, m.UnlistGameAfterStarted, "Unlist/Close the room after start?");
        GUILayout.Space(2);
        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.label);
        m.ShowPlayerScoreboard = MFPSEditorStyles.FeatureToogle(r, m.ShowPlayerScoreboard, "Show Player Scoreboard?");
        EditorGUIUtility.labelWidth = lw;

        GUILayout.Space(10);
        DrawTitleText("MODE OPTIONS");

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedGameMode").FindPropertyRelative("maxPlayers"), true);
            r = GUILayoutUtility.GetLastRect();
            r.x += 150;
            r.y += 27;
            r.width -= 150;
            r.height = EditorGUIUtility.singleLineHeight;
            string options = "";
            foreach (var item in m.maxPlayers)
            {
                if (IsOneTeamMode(m.gameMode)) options += $"({item}) ";
                else options += $"({item / 2}vs{item / 2}) ";
            }
            EditorGUI.HelpBox(r, $"Current options: {options}", MessageType.Info);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedGameMode").FindPropertyRelative("GameGoalsOptions"), true);
            r = GUILayoutUtility.GetLastRect();
            r.x += 150;
            r.width -= 150;
            r.height = EditorGUIUtility.singleLineHeight;
            string options = "";
            foreach (var item in m.GameGoalsOptions)
            {
                options += $"({item} {m.GoalName}) ";
            }
            EditorGUI.HelpBox(r, $"Current options: {options}", MessageType.Info);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("selectedGameMode").FindPropertyRelative("timeLimits"), true);
            r = GUILayoutUtility.GetLastRect();
            r.x += 150;
            r.width -= 150;
            r.height = EditorGUIUtility.singleLineHeight;
            string options = "";
            foreach (var item in m.timeLimits)
            {
                if (item < 60) options += $"({item} Seconds) ";
                else options += $"({Mathf.FloorToInt(item / 60)} Minutes) ";
            }
            EditorGUI.HelpBox(r, $"Current options: {options}", MessageType.Info);
        }
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(18);
        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Back", MFPSEditorStyles.EditorSkin.customStyles[11], GUILayout.Width(100)))
            {
                selectedGameMode = null;
            }
            GUILayout.Space(18);
        }
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(50);

        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(bl_GameData.Instance);
        }

    }

    public bool IsOneTeamMode(GameMode mode)
    {
        return mode == GameMode.BR || mode == GameMode.GR || mode == GameMode.FFA;
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawMaps()
    {
        GUILayout.Space(10);
        var maps = bl_GameData.Instance.AllScenes;
        int rowID = 0;
        int SpaceBetweenRow = 20;
        int rowCapacity = Mathf.FloorToInt((position.width - 150) / (370 + SpaceBetweenRow));
        for (int i = 0; i < maps.Count; i++)
        {
            if (rowID == 0)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
            }
            MapCard(maps[i]);
            GUILayout.Space(10);
            if (rowID == (rowCapacity - 1) || i == maps.Count - 1)
            {
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(SpaceBetweenRow);
            }
            rowID = (rowID + 1) % rowCapacity;
        }
    }

    void MapCard(MapInfo map)
    {
        var r = EditorGUILayout.BeginVertical(GUILayout.Width(300), GUILayout.Height(160));
        EditorGUI.DrawRect(r, primaryColor);

        var sr = r;
        GUILayout.Space(5);
        GUILayout.Label($"<size=12>{map.ShowName.ToUpper()}</size>", styles["titleH2"]);

        if (map.Preview != null)
        {
            var ir = GUILayoutUtility.GetRect(200, 100);
            GUI.DrawTexture(ir, map.Preview.texture, ScaleMode.ScaleToFit);
        }

        GUILayout.FlexibleSpace();
        DrawText($"<b><size=10>SCENE NAME</size></b>\n<color=#A4A4A4FF><size=9>{map.RealSceneName}</size></color>");

        GUILayout.Space(10);
        EditorGUILayout.EndVertical();

        sr.width -= 20;
        if (GUI.Button(sr, GUIContent.none, GUIStyle.none))
        {
            EditorGUIUtility.PingObject(map.m_Scene);
        }

        if (map.m_Scene != null)
        {
            // open scene button
            sr = r;
            sr.x = sr.x + sr.width - 24;
            sr.y += 4;
            sr.width = sr.height = 20;
            sr.height = 20;
            GUI.DrawTexture(sr, GetUnityIcon("d_SceneAsset Icon"), ScaleMode.ScaleToFit);

            if (GUI.Button(sr, GUIContent.none, GUIStyle.none))
            {
                string path = AssetDatabase.GetAssetPath(map.m_Scene);
                EditorSceneManager.OpenScene(path);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="player"></param>
    /// <param name="slotID"></param>
    void DrawPlayerLoadout(bl_PlayerReferences player, ref int slotID)
    {
        EditorGUILayout.BeginHorizontal(GUILayout.MaxHeight(256));
        {
            GUILayout.Space(10);
            //soldier icon
            Rect r = EditorGUILayout.BeginVertical(styles["borders"], GUILayout.Width(200));
            {
                EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();
                    GUILayout.Label(soldierIcon, GUILayout.Height(200), GUILayout.Width(100));
                    GUILayout.FlexibleSpace();
                }
                GUILayout.Label(player.name, styles["textC"]);
            }
            EditorGUILayout.EndVertical();

            r = EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
            {
                EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
                //Slot buttons
                using (new GUILayout.HorizontalScope(GUILayout.Height(30)))
                {
                    GUILayout.Space(5);
                    for (int i = 0; i < slotsNames.Length; i++)
                    {
                        Rect br = GUILayoutUtility.GetRect(new GUIContent(slotsNames[i]), styles["textC"], GUILayout.Height(30));
                        if (i != slotID)
                            EditorGUI.DrawRect(br, primaryColor);
                        else
                        {
                            EditorGUI.DrawRect(br, altColor);
                            GUI.color = new Color(1, 1, 1, 0.2f);
                            GUI.Box(br, GUIContent.none, MFPSEditorStyles.OutlineButtonStyle);
                            GUI.color = Color.white;
                        }
                        if (GUI.Button(br, slotsNames[i], styles["textC"]))
                        {
                            if (slotID != i)
                            {
                                selectedLoadout = null;
                                selectedSlot = -1;
                            }
                            slotID = i;
                        }
                    }
                    GUILayout.Space(5);
                }
                GUILayout.Space(20);

                var loadout = player.gunManager.m_AssaultClass;
                if (slotID == 1) loadout = player.gunManager.m_ReconClass;
                if (slotID == 2) loadout = player.gunManager.m_SupportClass;
                if (slotID == 3) loadout = player.gunManager.m_EngineerClass;
                //loadout
                DrawSlots(loadout);

            }
            EditorGUILayout.EndVertical();

        }
        GUILayout.Space(10);
        EditorGUILayout.EndHorizontal();
    }

    void DrawSlots(bl_PlayerClassLoadout loadout)
    {
        if (loadout == null) return;

        if (selectedLoadout != null && selectedLoadout == loadout)
        {
            DrawWeaponSelectionList();
            return;
        }

        var gun = loadout.GetPrimaryGunInfo();
        DrawSlot(gun, "PRIMARY", loadout, 0, loadout.Primary);
        GUILayout.Space(10);
        gun = loadout.GetSecondaryGunInfo();
        DrawSlot(gun, "SECONDARY", loadout, 1, loadout.Secondary);
        GUILayout.Space(10);
        gun = loadout.GetPerksGunInfo();
        DrawSlot(gun, "PERK", loadout, 2, loadout.Perks);
        GUILayout.Space(10);
        gun = loadout.GetLetalGunInfo();
        DrawSlot(gun, "LETAL", loadout, 3, loadout.Letal);
        GUILayout.Space(10);
    }

    void DrawSlot(bl_GunInfo gun, string Title, bl_PlayerClassLoadout loadout, int slotID, int gunIndex)
    {
        if (gun == null) return;

        EditorGUILayout.BeginHorizontal();
        {
            GUILayout.Space(25);
            GUILayout.Label($"<b>{Title}:</b>", styles["textC"], GUILayout.Width(100));
            GUILayout.Label(gun.Name, styles["textC"], GUILayout.Width(150));
            if (gun.GunIcon != null)
                GUILayout.Label(gun.GunIcon.texture, GUILayout.Height(22), GUILayout.Width(100));

            Rect br = GUILayoutUtility.GetRect(new GUIContent("CHANGE"), styles["textC"], GUILayout.Width(100));
            EditorGUI.DrawRect(br, altColor);
            if (GUI.Button(br, "CHANGE", styles["textC"]))
            {
                selectedSlot = slotID;
                selectedLoadout = loadout;
                currentWSelectionId = gunIndex;
                weaponScroll.y = 4 + (gunIndex * 27);
            }
            GUILayout.FlexibleSpace();
        }
        EditorGUILayout.EndHorizontal();
    }

    void DrawWeaponSelectionList()
    {
        var all = bl_MFPS.AllWeapons;
        weaponScroll = GUILayout.BeginScrollView(weaponScroll);
        GUILayout.Space(10);
        Color c = new(0, 0, 0, 0.1f);
        Color c2 = new(0, 0, 0, 0.31f);
        for (int i = 0; i < all.Count; i++)
        {
            Rect r = EditorGUILayout.BeginHorizontal();
            EditorGUI.DrawRect(r, i == currentWSelectionId ? c2 : c);
            GUILayout.Space(20);
            GUILayout.Label(all[i].Name, styles["textC"], GUILayout.Width(120), GUILayout.Height(20));
            GUILayout.Space(25);
            if (all[i].GunIcon != null)
                GUILayout.Label(all[i].GunIcon.texture, GUILayout.Height(22), GUILayout.Width(100));

            GUILayout.Space(25);
            Rect br = GUILayoutUtility.GetRect(new GUIContent("SELECT"), styles["textC"], GUILayout.Width(100));
            EditorGUI.DrawRect(br, altColor);
            if (GUI.Button(br, "SELECT", styles["textC"]))
            {
                if (selectedSlot == 0) selectedLoadout.Primary = i;
                else if (selectedSlot == 1) selectedLoadout.Secondary = i;
                else if (selectedSlot == 2) selectedLoadout.Perks = i;
                else if (selectedSlot == 3) selectedLoadout.Letal = i;

                currentWSelectionId = -1;

                EditorUtility.SetDirty(selectedLoadout);
                AssetDatabase.SaveAssets();

                selectedSlot = -1;
                selectedLoadout = null;
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);
        }
        GUILayout.EndScrollView();
    }

    void WeaponCard(bl_GunInfo gun, int gunId)
    {
        EditorStyles.label.richText = true;
        Rect rect = EditorGUILayout.BeginVertical(styles["borders"], GUILayout.Width(370), GUILayout.Height(100));
        {
            EditorGUI.DrawRect(rect, new Color(1, 1, 1, 0.02f));

            EditorGUILayout.BeginHorizontal();
            {
                using (new GUILayout.VerticalScope())
                {
                    GUILayout.Space(5);
                    GUILayout.Label($"<size=14>{gun.Name}</size>", styles["titleH2"], GUILayout.Height(18));
                    GUILayout.Label($"<size=9><color=#727272>{gun.Type}</color></size>", styles["titleH2"], GUILayout.Height(20));
                    GUILayout.Space(18);
                    GUILayout.FlexibleSpace();
                    Rect iconR = GUILayoutUtility.GetRect(120, 70);
                    iconR.x += 2;
                    iconR.width -= 4;
                    if (gun.GunIcon != null) GUI.DrawTexture(iconR, gun.GunIcon.texture, ScaleMode.ScaleToFit);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.FlexibleSpace();
            }
            GUILayout.Space(5);
            EditorGUILayout.BeginVertical();
            {
                gun.Damage = EditorGUILayout.IntSlider("<size=10>DAMAGE</size>", gun.Damage, 1, 100);
                gun.FireRate = EditorGUILayout.Slider("<size=10>FIRE RATE</size>", gun.FireRate, 0.01f, 2f);
                gun.Accuracy = EditorGUILayout.Slider("<size=10>ACCURACY</size>", gun.Accuracy, 1, 5);
                gun.ReloadTime = EditorGUILayout.Slider("<size=10>RELOAD</size>", gun.ReloadTime, 0.5f, 10);
                gun.Range = EditorGUILayout.IntSlider("<size=10>RANGE</size>", gun.Range, 1, 700);
                gun.Weight = EditorGUILayout.Slider("<size=10>WEIGHT</size>", gun.Weight, 0, 4);
                GUILayout.Space(10);
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                GUILayout.Space(4);
                if (GUILayout.Button("EDIT", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    editWeaponId = gunId;
                    editGunInfo = gun;
                }

                var fr = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toolbarButton, GUILayout.Width(60));
                EditorGUI.DrawRect(fr, new Color(1, 1, 1, 0.1f));
                GUI.Label(fr, $"<size=10><color=#727272>Gun ID: <b>{gunId}</b></color></size>", styles["textC"]);
                GUILayout.Space(1);
                fr = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toolbarButton, GUILayout.Width(100));
                EditorGUI.DrawRect(fr, new Color(1, 1, 1, 0.1f));
                GUI.Label(fr, $"<size=10><color=#727272>Power Score: <b>{gun.CalculatePower():F}</b></color></size>", styles["textC"]);
                GUILayout.Space(1);
                fr = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toolbarButton, GUILayout.Width(32));
                EditorGUI.DrawRect(fr, new Color(1, 1, 1, 0.1f));
                if (gun.Unlockability.UnlockMethod == MFPS.Internal.Structures.MFPSItemUnlockability.UnlockabilityMethod.UnlockedByDefault)
                {
                    GUI.DrawTexture(fr, GetUnityIcon("IN LockButton act@2x"), ScaleMode.ScaleToFit);
                }
                else
                {
                    GUI.DrawTexture(fr, GetUnityIcon("IN LockButton on@2x"), ScaleMode.ScaleToFit);
                }

                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(4);
        }
        EditorGUILayout.EndVertical();
    }

    #region Manager Windows
    void DrawGameDataSettings()
    {
        if (gameData == null) gameData = bl_GameData.Instance;

        DrawTitleText("GAME DATA");
        if (gameData == null) return;
        DrawEditorOf(gameData);
    }

    void DrawPlayerSelector()
    {
        DrawTitleText("PLAYER SELECTOR");
#if PSELECTOR
        DrawEditorOf(bl_PlayerSelector.Data);
#else
        DrawDisableAddon("PLAYER SELECTOR", "PSELECTOR");
#endif
    }

    void DrawCustomizer()
    {
        DrawTitleText("Customizer");
#if CUSTOMIZER
        DrawEditorOf(bl_CustomizerData.Instance);
#else
        DrawDisableAddon("Class Customizer", "CUSTOMIZER");
#endif
    }

    void DrawMobileControl()
    {
        DrawTitleText("Mobile Control");
#if MFPSM
        DrawEditorOf(bl_MobileControlSettings.Instance);
#else
        DrawDisableAddon("MFPS Mobile", "MFPSM");
#endif
    }

    void DrawClassCustomizer()
    {
        DrawTitleText("Class Customizer");
#if CLASS_CUSTOMIZER
        DrawEditorOf(bl_ClassManager.Instance);
#else
        DrawDisableAddon("Class Customizer", "CLASS_CUSTOMIZER");
#endif
    }

    void DrawULogin()
    {
        DrawTitleText("ULogin Pro");
#if ULSP
        DrawEditorOf(bl_LoginProDataBase.Instance);
#else
        DrawDisableAddon("ULogin Pro", "ULSP");
#endif
    }

    void DrawlevelManager()
    {
        DrawTitleText("Level Manager");
#if LM
        DrawEditorOf(bl_LevelManager.Instance);
#else
        DrawDisableAddon("Level Manager", "LM");
#endif
    }

    void DrawMinimap()
    {
        DrawTitleText("MiniMap");
#if UMM
        DrawEditorOf(bl_MiniMapData.Instance);
#else
        DrawDisableAddon("MiniMap", "UMM");
#endif
    }

    void DrawClan()
    {
        DrawTitleText("Clan System");
#if CLANS
        DrawEditorOf(bl_ClanSettings.Instance);
#else
        DrawDisableAddon("Clan System", "CLANS");
#endif
    }

    void DrawFloatingText()
    {
        var fm = Resources.Load("FloatingTextManagerSettings", typeof(ScriptableObject)) as ScriptableObject;
        if (fm == null)
        {
            DrawDisableAddon("Floating Text", "FT");
            return;
        }

        DrawTitleText("Floating Text");
        DrawEditorOf(fm);
    }

    void DrawEmblems()
    {
        var fm = Resources.Load("EmblemsDataBase", typeof(ScriptableObject)) as ScriptableObject;
        if (fm == null)
        {
            DrawDisableAddon("Emblems And Calling Cards", "EACC");
            return;
        }

        DrawTitleText("Emblems And Calling Cards");
        DrawEditorOf(fm);
    }

    void DrawLayoutCustomizer()
    {
        var fm = Resources.Load("LayoutCustomizerSettings", typeof(ScriptableObject)) as ScriptableObject;
        if (fm == null)
        {
            DrawDisableAddon("Layout Customizer", "LCZ");
            return;
        }

        DrawTitleText("Layout Customizer");
        DrawEditorOf(fm);
    }

    void DrawVehicles()
    {
        var fm = Resources.Load("VehicleSettings", typeof(ScriptableObject)) as ScriptableObject;
        if (fm == null)
        {
            DrawDisableAddon("Vehicle", "");
            return;
        }

        DrawTitleText("General Vehicle Settings");
        DrawEditorOf(fm);
    }

    void DrawAntiCheat()
    {
        DrawTitleText("Anti-Cheat");
#if ACTK_IS_HERE
        DrawEditorOf(bl_AntiCheatSettings.Instance);
#else
        DrawDisableAddon("Anti-Cheat", "ACTK_IS_HERE");
#endif
    }

    void DrawGameNews()
    {
        var fm = Resources.Load("GameNewsSettings", typeof(ScriptableObject)) as ScriptableObject;
        if (fm == null)
        {
            DrawDisableAddon("Game News", "");
            return;
        }

        DrawTitleText("Game News");
        DrawEditorOf(fm);
    }

    void DrawThirdPerson()
    {
        DrawTitleText("Third Person");
#if MFPSTPV
        DrawEditorOf(bl_CameraViewSettings.Instance);
#else
        DrawDisableAddon("Third Person", "MFPSTPV");
#endif
    }

    void DrawInputManager()
    {
        DrawTitleText("Input Manager");

        DrawEditorOf(bl_Input.InputData);
        GUILayout.Space(10);
        if (bl_Input.InputData.DefaultMapped != null)
        {
            DrawTitleText($"Mapped ({bl_Input.InputData.DefaultMapped.name})");
            DrawEditorOf(bl_Input.InputData.DefaultMapped);
        }
    }

    void DrawShop()
    {
        DrawTitleText("SHOP");
#if SHOP
        DrawEditorOf(bl_ShopData.Instance);
#else
        DrawDisableAddon("Shop", "SHOP");
#endif

        GUILayout.Space(10);
        DrawTitleText("Unity IAP");
#if SHOP_UIAP
        DrawEditorOf(bl_UnityIAP.Instance);
#else
        DrawDisableAddon("Unity IAP", "SHOP_UIAP");
#endif

        GUILayout.Space(10);
        DrawTitleText("Paypal");
#if SHOP_PAYPAL
        DrawEditorOf(bl_Paypal.Settings);
#else
        DrawDisableAddon("Paypal", "SHOP_PAYPAL");
#endif
    }

    void DrawKillStreaks()
    {
        DrawTitleText("Kill Streaks");
#if KSA
        DrawEditorOf(bl_KillStreakData.Instance);
#else
        DrawDisableAddon("Kill Streaks", "KSA");
#endif

        GUILayout.Space(10);
        DrawTitleText("Kill Streak Notifier");
#if KILL_NOTIFIER
        DrawEditorOf(MFPS.Addon.KillStreak.bl_KillNotifierData.Instance);
#else
        DrawDisableAddon("Kill Streak Notifier", "KILL_NOTIFIER");
#endif
    }
    #endregion

    private bool lpAlt = false;
    void DrawManagerButtons()
    {
        GUILayout.Space(2);
        managerSearchKey = managerSearchField.OnToolbarGUI(managerSearchKey);
        Rect r;
        managersScroll = GUILayout.BeginScrollView(managersScroll);
        for (int i = 0; i < managerPanels.Count; i++)
        {
            if (!string.IsNullOrEmpty(managerSearchKey))
            {
                if (!managerPanels[i].Name.ToLower().Contains(managerSearchKey.ToLower())) continue;
            }

            lpAlt = !lpAlt;
            r = GUILayoutUtility.GetRect(new GUIContent(managerPanels[i].Name), styles["panelButton"], GUILayout.Height(26));
            EditorGUI.DrawRect(r, lpAlt ? GetHexColor("#1212122F") : GetHexColor("#1212120F"));
            if (currentPanelType == managerPanels[i].Name)
            {
                EditorGUI.DrawRect(r, altColor);
                float w = r.width;
                r.width = 1;
                EditorGUI.DrawRect(r, hightlightColor);
                r.width = w;
            }
            if (GUI.Button(r, managerPanels[i].Name, styles["panelButton"]))
            {
                bodyScroll = Vector2.zero;
                currentPanelType = managerPanels[i].Name;
                currentPanel = managerPanels[i];
                currentWindow = WindowType.Managers;
            }
        }
        GUILayout.Space(30);
        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawWeaponPanel()
    {
        GUILayout.Space(20);
        weaponSearchKey = weaponSearchfield.OnToolbarGUI(weaponSearchKey);
        GUILayout.Space(20);

        if (GUILayout.Button("Add New", styles["panelButton"]))
        {
            editGunInfo = new bl_GunInfo();
            editWeaponId = 998;
        }

        GUILayout.Space(20);
        GUILayout.Label("Filter by Type", EditorStyles.boldLabel);
        filterWeaponType = (GunType)EditorGUILayout.EnumPopup(filterWeaponType);
    }

    /// <summary>
    /// 
    /// </summary>
    void InitGUI()
    {
        m_initGUI = true;

        styles["panelButton"] = GetStyle("label", (ref GUIStyle style) =>
        {
            style.alignment = TextAnchor.MiddleLeft;
            style.normal.textColor = new Color(0.399f, 0.399f, 0.399f, 1.00f);
            style.fontStyle = FontStyle.Bold;
        });
        styles["textC"] = GetStyle("textmwc", (ref GUIStyle style) =>
        {
            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = new Color(0.399f, 0.399f, 0.399f, 1.00f);
            style.richText = true;
            style.fontSize = 10;
        }, "label");
        styles["titleH2"] = GetStyle("titleH2", (ref GUIStyle style) =>
        {
            style.alignment = TextAnchor.MiddleLeft;
            style.normal.textColor = Color.white;
            style.fontStyle = FontStyle.Bold;
            style.fontSize = 18;
        }, "label");
        styles["borders"] = GetStyle("grey_border", (ref GUIStyle style) =>
        {
            style.normal.textColor = new Color(0.399f, 0.399f, 0.399f, 1.00f);
            style.overflow.left = style.overflow.right = style.overflow.top = style.overflow.bottom = 5;
            style.padding.left = -5; style.padding.right = 5;
            style.margin.left = style.margin.right = 10;
        });
        styles["miniText"] = GetStyle(EditorStyles.miniLabel.name, (ref GUIStyle style) =>
        {
            style.normal.textColor = new Color(0.399f, 0.399f, 0.399f, 1.00f);
            style.richText = true;
        });
        styles["text"] = GetStyle("label", (ref GUIStyle style) =>
        {
            style.alignment = TextAnchor.MiddleLeft;
            style.normal.textColor = Color.white;
            style.fontSize = 12;
            style.richText = true;
        });
    }

    #region Drawers
    public void DrawTitleText(string title)
    {
        GUILayout.Label(title.ToUpper(), styles["titleH2"]);
        DrawBottomLine();
        GUILayout.Space(20);
    }

    public void DrawText(string text, params GUILayoutOption[] options)
    {
        GUILayout.Label(text, styles["text"], options);
    }

    public void DrawText(GUIContent text, params GUILayoutOption[] options)
    {
        GUILayout.Label(text, styles["text"], options);
    }

    void DrawBottomLine(float alpha = 0.7f) => DrawBottomLine(GUILayoutUtility.GetLastRect(), alpha);

    void DrawBottomLine(Rect fullRect, float alpha = 0.7f)
    {
        var color = whiteColor;
        color.a = alpha;
        fullRect.y += fullRect.height;
        fullRect.height = 1;
        EditorGUI.DrawRect(fullRect, color);
    }

    public void DrawEditorOf(ScriptableObject scriptable)
    {
        if (scriptable == null) return;

        Editor editor;
        if (cachedEditors.ContainsKey(scriptable))
        {
            editor = cachedEditors[scriptable];
        }
        else
        {
            editor = Editor.CreateEditor(scriptable);
            cachedEditors.Add(scriptable, editor);
        }

        if (editor == null) return;

        Rect r = EditorGUILayout.BeginVertical(styles["borders"]);
        EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
        editor.OnInspectorGUI();
        EditorGUILayout.EndVertical();
    }

    public void DrawDisableAddon(string addonName, string addonKey)
    {
        Rect r = EditorGUILayout.BeginVertical(styles["borders"]);
        EditorGUI.DrawRect(r, new Color(1, 1, 1, 0.02f));
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        EditorGUILayout.BeginVertical();
        GUILayout.Label(GetUnityIcon("console.warnicon"), styles["panelButton"]);
        GUILayout.Label($"<color=#fff><size=18><b>{addonName.ToUpper()}</b></size></color>", styles["textC"]);
        var addonInfo = GetAddonsInfo(addonKey);
        if (addonInfo.IsAddonInProject())
        {
            GUILayout.Label("Is disabled.", styles["textC"]);
        }
        else
        {
            GUILayout.Label("Is not available in your project.", styles["textC"]);
        }
        GUILayout.Space(10);
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUI.enabled = AssetData.AssetStoreCompliant() == false;
        if (MFPSEditorStyles.ButtonOutline("Addon Manager", Color.yellow, GUILayout.Width(110)))
        {
            EditorWindow.GetWindow<MFPSAddonsWindow>().OpenAddonPage(addonInfo.NiceName);
        }
        GUI.enabled = true;
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        GUILayout.Space(5);
        EditorGUILayout.EndVertical();
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    #endregion

    #region Utils
    private GUIStyle GetStyle(string name, TutorialWizard.Style.OnCreateStyleOp onCreate, string overlap = "") => TutorialWizard.Style.GetUnityStyle(name, onCreate, overlap);

    private static Dictionary<string, Texture2D> cachedUnityIcons;
    public static Texture2D GetUnityIcon(string iconName)
    {
        cachedUnityIcons ??= new Dictionary<string, Texture2D>();
        if (!cachedUnityIcons.ContainsKey(iconName))
        {
            var icon = (Texture2D)EditorGUIUtility.IconContent(iconName).image;
            cachedUnityIcons.Add(iconName, icon);
        }
        return cachedUnityIcons[iconName];
    }

    public static Color GetHexColor(string hex) => MFPSEditorStyles.GetColorFromHex(hex);

    private Dictionary<string, MFPSAddonsInfo> cachedAddonsInfo;
    public MFPSAddonsInfo GetAddonsInfo(string addonKey)
    {
        cachedAddonsInfo ??= new Dictionary<string, MFPSAddonsInfo>();
        if (!cachedAddonsInfo.ContainsKey(addonKey))
        {
            cachedAddonsInfo.Add(addonKey, MFPSAddonsData.Instance.GetAddonInfoByKey(addonKey));
        }
        return cachedAddonsInfo[addonKey];
    }

    private static Dictionary<string, Color> cachedColors;
    public static Color GetCachedColor(string key, Color color)
    {
        cachedColors ??= new Dictionary<string, Color>();
        if (!cachedColors.ContainsKey(key))
        {
            cachedColors.Add(key, color);
        }
        return cachedColors[key];
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="panelId"></param>
    /// <param name="paramaters"></param>
    public static void Open(WindowType panelId, string[] paramaters = null)
    {
        var window = GetWindow<bl_MFPSManagerWindow>();
        window.currentWindow = panelId;

        if (panelId == WindowType.Weapons)
        {
            if (paramaters != null && paramaters.Length > 0)
            {
                if (int.TryParse(paramaters[0], out int guiId))
                {
                    window.editWeaponId = guiId;
                    window.editGunInfo = bl_GameData.Instance.GetWeapon(guiId);
                }
            }
        }
    }

    [MenuItem("MFPS/Manager %m")]
    static void Open()
    {
        GetWindow<bl_MFPSManagerWindow>();
    }

    void FetchCustomTabs()
    {
        var classes = TypeCache.GetTypesWithAttribute<MFPSManagerTab>();
        foreach (var classInfo in classes)
        {
            // get the attribute information
            var attr = classInfo.GetCustomAttribute<MFPSManagerTab>();

            var panel = managerPanels.Find(x => x.Name == attr.name);
            if (panel != null)
            {
                if (panel.bodyFunc == null)
                {

                }
                else continue;
            }

            // Cast the MFPSManagerTabData type from the classInfo
            var tab = (MFPSManagerTabData)Activator.CreateInstance(classInfo);
            var data = tab.GetData();

            if (panel == null)
            {
                managerPanels.Add(new ManagerPanel()
                {
                    Name = attr.name,
                    bodyFunc = data.BodyDrawFunc,
                    isExternalDrawer = true,
                });
            }
            else
            {
                panel.bodyFunc = data.BodyDrawFunc;
            }
        }

    }

    [Serializable]
    public class ManagerPanel
    {
        public string Name;
        public string BodyFuncName;
        public Type CustomType;

        internal MethodInfo bodyFunc;
        internal bool isExternalDrawer = false;

        public void DrawBody(Type type, bl_MFPSManagerWindow target)
        {
            if (string.IsNullOrEmpty(BodyFuncName) && bodyFunc == null) { return; }

            if (bodyFunc == null)
            {
                bodyFunc = type.GetMethod(BodyFuncName, BindingFlags.NonPublic | BindingFlags.Instance);
            }

            if (isExternalDrawer)
            {
                bodyFunc?.Invoke(null, new object[] { target });
            }
            else
            {
                bodyFunc?.Invoke(target, null);
            }
        }
    }

    [Serializable]
    public enum WindowType
    {
        Home,
        Managers,
        Weapons,
        Players,
        Loadouts,
        Levels,
        GameModes,
        Maps,
    }
    #endregion
}

/// <summary>
/// 
/// </summary>
public abstract class MFPSManagerTabData
{
    public struct Data
    {
        public string Title;
        public string BodyFuncName;
        public MethodInfo BodyDrawFunc;
    }

    public abstract Data GetData();

    public virtual MethodInfo GetMethodInfoOf(string staticMethodName)
    {
        return this.GetType().GetMethod(staticMethodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
    }
}

/// <summary>
/// 
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class MFPSManagerTab : Attribute
{
    public string name;

    public MFPSManagerTab(string name)
    {
        this.name = name;
    }
}
using MFPS.Internal.Scriptables;
using MFPS.Internal.Structures;
using MFPS.Runtime.Settings;
using MFPS.Runtime.UI;
using MFPSEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class bl_GameData : ScriptableObject
{
    #region Public members
    [Header("Game Settings")]
    [ScriptableDrawer] public bl_GameCoreSettings coreSettings = null;

    [Header("Levels Manager")]
    [Tooltip("The scene that contains the room UI")]
    public MFPSSceneAsset UIScene;
    [Tooltip("Contains the basic data of all MFPS maps scenes.")]
    public List<MapInfo> AllScenes = new();

    [Header("Weapons")]
#if UNITY_2020_2_OR_NEWER
    [NonReorderable] // DO NOT reorder the weapons manually, it will mess up your whole setup!
#endif
    public List<bl_GunInfo> AllWeapons = new();

    [Header("Rewards")]
    [ScriptableDrawer] public bl_GameScoreSettings scoreSettings = null;

    [Header("Default In-Game Settings")]
    [ScriptableDrawer, SerializeField] private bl_RuntimeSettingsProfile defaultSettings;

    [Header("Game Modes Available"), FormerlySerializedAs("AllGameModes")]
    public List<GameModeSettings> gameModes = new();

    [Header("Teams")]
    [ScriptableDrawer] public MFPSTeam team1;
    [ScriptableDrawer] public MFPSTeam team2;

    [Header("Players")]
    public bl_PlayerNetworkBase Player1;
    public bl_PlayerNetworkBase Player2;
    [Tooltip("The default MFPS player prefabs don't include extra skins, so this value will only have an effect if you're using a player prefab with multiple skins that are set up to work with the MFPS skin system. Otherwise, it won’t have any impact")]
    public int Player1SkinID = 0;
    [Tooltip("The default MFPS player prefabs don't include extra skins, so this value will only have an effect if you're using a player prefab with multiple skins that are set up to work with the MFPS skin system. Otherwise, it won’t have any impact")]
    public int Player2SkinID = 0;

    [Header("Bots")]
    public bl_AIShooter BotTeam1;
    public bl_AIShooter BotTeam2;

    [Header("Coins")]
#if UNITY_2020_2_OR_NEWER
    [NonReorderable]
#endif
    [ScriptableDrawer] public List<MFPSCoin> gameCoins;

    [Header("Player Classes")]
    public List<PlayerClassData> playerClassesNames;

    [Header("PUN Regions")]
    public bl_ServerRegionDropdown.PhotonRegion[] punRegions;

    [Header("Weapon Only Rule"), Tooltip("The weapon type options that will appear in the room creator for the Weapon Only setting.")]
    public List<GunType> allowedWeaponOnlyOptions;

    [ScriptableDrawer] public MouseLookSettings mouseLookSettings;

    [ScriptableDrawer] public bl_WeaponSlotRuler weaponSlotRuler;

    [ScriptableDrawer, SerializeField] private TagsAndLayersSettings tagsAndLayersSettings;

    [SerializeField] private bl_PhotonNetworkStats networkStatsPrefab = null;

    public static bool isChatting = false;
    public static bool FirstAppLoad = false;
    #endregion

    #region Private Members
    [HideInInspector] public string _MFPSLicense = string.Empty;
    [HideInInspector] public int _MFPSFromStore = 2;
    [HideInInspector] public string _keyToken = "";
    private static bl_PhotonNetwork PhotonGameInstance = null;
    public static bool isDataCached = false;
    private static bool isCaching = false;
    private static bl_GameData m_instance;
    public static int WeaponListCount = 0;
    private static int validateId = 0;
    private int lastWeaponValidate = 0;
    private string[] weaponNamesCached;
    private string[] weaponNamesCatCached;
    private string _role = string.Empty;
    private static Dictionary<GameMode, GameModeSettings> gameModesDict;
    private static Dictionary<GameMode, bool> teamModeGameMode;
    #endregion

    /// <summary>
    /// 
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void OnRuntimeInit()
    {
        // check that there's an instance of the Photon object in scene
        if (PhotonGameInstance == null && Application.isPlaying)
        {
            PhotonGameInstance = bl_PhotonNetwork.Instance;
            if (PhotonGameInstance == null)
            {
                try
                {
                    var pgo = new GameObject("PhotonGame");
                    PhotonGameInstance = pgo.AddComponent<bl_PhotonNetwork>();

                    if (CoreSettings.ShowNetworkStats && FindAnyObjectByType<bl_PhotonNetworkStats>() == null)
                    {
                        Instantiate(Instance.networkStatsPrefab.gameObject);
                    }
                }
                catch (Exception)
                {
                }
            }
        }

        if (m_instance != null && !isCaching) { isDataCached = true; }
        bl_GameInput.InputFocus = MFPSInputFocus.Player;
        // cache the teammode game mode
        teamModeGameMode = new Dictionary<GameMode, bool>();
        foreach (var mode in Instance.gameModes)
        {
            teamModeGameMode.Add(mode.gameMode, mode.isTeamMode);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private static void OnDataCached()
    {
        if (!bl_UtilityHelper.isMobile && Instance.mouseLookSettings.customCursor != null)
        {
            Cursor.SetCursor(Instance.mouseLookSettings.customCursor, Vector2.zero, CursorMode.ForceSoftware);
        }

        if (!FirstAppLoad)
        {
            FirstAppLoad = true;
            var apm = bl_ResolutionSettings.ResolutionHandler == null ? bl_RuntimeSettingsProfile.ResolutionApplication.ApplyCurrent : bl_RuntimeSettingsProfile.ResolutionApplication.NoApply;
            bl_MFPS.Settings.ApplySettings(apm, false);
        }

        gameModesDict = Instance.gameModes.ToDictionary(x => x.gameMode, x => x);
    }

    /// <summary>
    /// 
    /// </summary>
    void OnDisable()
    {
        isDataCached = false;
        RolePrefix = "";
    }

    /// <summary>
    /// 
    /// </summary>
    public void ResetInstance()
    {
        m_settingProfile = null;
        isDataCached = false;
    }

    #region Getters
    /// <summary>
    /// Get a weapon info by they ID
    /// </summary>
    /// <param name="ID">the id of the weapon, this ID is the indexOf the weapon info in GameData</param>
    /// <returns></returns>
    public bl_GunInfo GetWeapon(int ID)
    {
        return ID < 0 || ID > AllWeapons.Count - 1 ? AllWeapons[0] : AllWeapons[ID];
    }

    /// <summary>
    /// Get a weapon info by they Name
    /// </summary>
    /// <param name="gunName"></param>
    /// <returns></returns>
    public int GetWeaponID(string gunName)
    {
        int id = -1;
        if (AllWeapons.Exists(x => x.Name == gunName))
        {
            id = AllWeapons.FindIndex(x => x.Name == gunName);
        }
        return id;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weaponName"></param>
    /// <param name="gunId"></param>
    /// <param name="info"></param>
    public static void GetWeaponDataByName(string weaponName, out int gunId, out bl_GunInfo info)
    {
        gunId = -1;
        info = null;

        if (Instance.AllWeapons.Exists(x => x.Name == weaponName))
        {
            gunId = Instance.AllWeapons.FindIndex(x => x.Name == weaponName);
            info = Instance.AllWeapons[gunId];
        }
    }

    /// <summary>
    /// Get the list of all weapon names as strings.
    /// </summary>
    /// <param name="cached">Whether to use cached data or not.</param>
    /// <param name="categorized">Whether to return the weapon names with categories or not.</param>
    /// <returns>An array of weapon names.</returns>
    public string[] AllWeaponStringList(bool cached = false, bool categorized = false)
    {
        if (categorized)
        {
            if (!cached)
            {
                var weapons = new string[AllWeapons.Count];
                for (int i = 0; i < AllWeapons.Count; i++)
                {
                    weapons[i] = $"{AllWeapons[i].Type}/{AllWeapons[i].Name}";
                }
                return weapons;
            }

            if (validateId != lastWeaponValidate || weaponNamesCatCached == null || weaponNamesCatCached.Length == 0)
            {
                weaponNamesCatCached = new string[AllWeapons.Count];
                for (int i = 0; i < AllWeapons.Count; i++)
                {
                    weaponNamesCatCached[i] = $"{AllWeapons[i].Type}/{AllWeapons[i].Name}";
                }
                lastWeaponValidate = validateId;
            }
            return weaponNamesCatCached;
        }

        if (!cached) return AllWeapons.Select(x => x.Name).ToList().ToArray();

        if (validateId != lastWeaponValidate || weaponNamesCached == null || weaponNamesCached.Length == 0)
        {
            weaponNamesCached = AllWeapons.Select(x => x.Name).ToList().ToArray();
            lastWeaponValidate = validateId;
        }
        return weaponNamesCached;
    }

    /// <summary>
    /// Get the display name of a player class
    /// </summary>
    /// <param name="playerClass"></param>
    /// <returns></returns>
    public static PlayerClassData GetPlayerClassData(PlayerClass playerClass)
    {
        int index = Instance.playerClassesNames.FindIndex(x => x.Identifier == playerClass);
        return index == -1 ? null : Instance.playerClassesNames[index];
    }

    /// <summary>
    /// Get the game mode settings for a specific game mode
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static GameModeSettings GetGameModeInfo(GameMode mode)
    {
        if (gameModesDict == null)
        {
            gameModesDict = Instance.gameModes.ToDictionary(x => x.gameMode, x => x);
        }
        return gameModesDict.TryGetValue(mode, out var settings) ? settings : null;
    }

    /// <summary>
    /// Checks if the specified game mode is a team mode.
    /// </summary>
    /// <param name="mode">Indicates the game mode to be evaluated for team play.</param>
    /// <returns>Returns true if the game mode is a team mode, otherwise false = solo mode.</returns>
    public static bool IsTeamMode(GameMode mode)
    {
        return teamModeGameMode[mode];
    }

    /// <summary>
    /// 
    /// </summary>
    public string RolePrefix
    {
        get
        {
            if (!bl_MFPSDatabase.IsUserLogged)
            {
                return string.Empty;
            }

            if (string.IsNullOrEmpty(_role))
            {
                _role = bl_MFPSDatabase.User.GetRolePrefix();
                if (bl_MFPSDatabase.Clan.IsLocalInClan())
                {
                    if (_role != null && _role.Length > 1) _role += " ";
                    _role += string.Format("{0}", bl_MFPSDatabase.Clan.GetClanTag());
                }
            }
            return _role;
        }
        set => _role = value;
    }

    private bl_RuntimeSettingsProfile m_settingProfile;
    public bl_RuntimeSettingsProfile RuntimeSettings
    {
        get
        {
            if (m_settingProfile == null) m_settingProfile = Instantiate(defaultSettings);
            return m_settingProfile;
        }
    }

    private static TagsAndLayersSettings m_tagsAndLayerSettings;
    public static TagsAndLayersSettings TagsAndLayerSettings
    {
        get
        {
            if (m_tagsAndLayerSettings == null) m_tagsAndLayerSettings = Instance.tagsAndLayersSettings;
            return m_tagsAndLayerSettings;
        }
    }

    public static bl_GameCoreSettings CoreSettings => Instance.coreSettings;

    public static bl_GameScoreSettings ScoreSettings => Instance.scoreSettings;

    /// <summary>
    /// 
    /// </summary>
    public static bl_GameData Instance
    {
        get
        {
            if (m_instance == null && !isCaching)
            {
                if (!isDataCached && Application.isPlaying)
                {
                    // Debug.Log("GameData was cached synchronous, that could cause bottleneck on load, try caching it asynchronous with AsyncLoadData()");
                    isDataCached = true;
                    OnDataCached();
                }
                m_instance = Resources.Load("GameData", typeof(bl_GameData)) as bl_GameData;
            }
            return m_instance;
        }
    }
    #endregion

    /// <summary>
    /// cache the GameData from Resources asynchronous to avoid overhead and freeze the main thread the first time we access to the instance
    /// </summary>
    /// <returns></returns>
    public static IEnumerator AsyncLoadData()
    {
        if (m_instance == null)
        {
            isCaching = true;
            ResourceRequest rr = Resources.LoadAsync("GameData", typeof(bl_GameData));
            while (!rr.isDone) { yield return null; }
            m_instance = rr.asset as bl_GameData;
            isCaching = false;
            OnDataCached();
        }
        isDataCached = true;
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        validateId = UnityEngine.Random.Range(0, 99999);
        for (int i = 0; i < AllScenes.Count; i++)
        {
            AllScenes[i].Validate();
        }

        UIScene.Validate();

        if (WeaponListCount != AllWeapons.Count)
        {
            List<string> strings = new();
            foreach (var item in AllWeapons)
            {
                if (string.IsNullOrEmpty(item.Key))
                {
                    item.Key = bl_StringUtility.GenerateKey();
                    UnityEditor.EditorUtility.SetDirty(Instance);
                }
                else if (strings.Contains(item.Key))
                {
                    item.Key = bl_StringUtility.GenerateKey();
                    UnityEditor.EditorUtility.SetDirty(Instance);
                }
                strings.Add(item.Key);
            }
            WeaponListCount = AllWeapons.Count;
        }
    }
#endif

    #region Local Classes
    [Serializable]
    public class PlayerClassData
    {
        public string DisplayName;
        public PlayerClass Identifier;
        [SpritePreview] public Sprite ClassIcon;
    }
    #endregion
}
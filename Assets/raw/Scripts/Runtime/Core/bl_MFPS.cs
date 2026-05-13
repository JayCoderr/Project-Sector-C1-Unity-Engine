using MFPS.Core;
using MFPS.Internal.Scriptables;
using MFPS.Internal.Structures;
using MFPS.Runtime.Settings;
using MFPSEditor;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

/// <summary>
/// Static class with lots of useful functions and variables for the game
/// </summary>
public static class bl_MFPS
{
    public static float MusicVolume = 1;

    /// <summary>
    /// Is the current scene a MAP multiplayer scene?
    /// </summary>
    public static bool IsMultiplayerScene = true;

    public static bl_RuntimeSettingsProfile Settings => bl_GameData.Instance.RuntimeSettings;
    public static bl_PlayerReferences LocalPlayerReferences
    {
        get
        {
            return bl_GameManager.Instance != null
                ? bl_GameManager.Instance.LocalPlayerReferences
                : GameObject.FindAnyObjectByType<bl_PlayerReferences>();
        }
    }
    public static List<bl_GunInfo> AllWeapons => bl_GameData.Instance.AllWeapons;

    /// <summary>
    /// The current match game mode script handler.
    /// </summary>
    public static bl_GameModeBase CurrentGameModeLogic => bl_GameManager.Instance.GameModeLogic;

    public const string LOCAL_PLAYER_TAG = "Player";
    public const string REMOTE_PLAYER_TAG = "Remote";
    public const string AI_TAG = "AI";
    public const string HITBOX_TAG = "BodyPart";

    /// <summary>
    /// Class helper with some useful function referenced to the local player only.
    /// </summary>
    public static class LocalPlayer
    {
        /// <summary>
        /// The current view ID of the local player
        /// NOTE: This view id can change if the player is killed and respawned
        /// </summary>
        public static int ViewID = -1;

        /// <summary>
        /// The current actor number of the local player
        /// This number is unique for each player in the room and never changes until the player leaves the room
        /// </summary>
        public static int ActorNumber = -1;

        /// <summary>
        /// The player unique ID
        /// This ID is unique for each player in the game, in case of using ULogin Pro, this is the User row id
        /// </summary>
        public static string ID;

        private static MFPSPlayer OfflineActor;
        /// <summary>
        /// The MFPS actor of the local player
        /// </summary>
        public static MFPSPlayer MFPSActor
        {
            get => bl_GameManager.Instance != null ? bl_GameManager.Instance.LocalActor : OfflineActor;
            set => OfflineActor = value;
        }

        /// <summary>
        /// The team of the local player
        /// </summary>
        public static Team Team
        {
            get;
            set;
        }

        /// <summary>
        /// Stats of the local player
        /// </summary>
        public static class Stats
        {
            /// <summary>
            /// Get the all time kill count of the local player
            /// </summary>
            /// <returns></returns>
            public static int GetAllTimeKills()
            {
                return bl_MFPSDatabase.GetInt("kills");
            }

            /// <summary>
            /// Get the all time death count of the local player
            /// </summary>
            /// <returns></returns>
            public static int GetAllTimeDeaths()
            {
                return bl_MFPSDatabase.GetInt("deaths");
            }

            /// <summary>
            /// Get the all time score of the local player
            /// </summary>
            /// <returns></returns>
            public static int GetAllTimeScore()
            {
                return bl_MFPSDatabase.GetInt("score");
            }
        }

        /// <summary>
        /// Make the local player die
        /// </summary>
        public static bool Suicide(bool increaseWarnings = true)
        {
            if (!bl_PhotonNetwork.InRoom || bl_GameManager.Instance.LocalPlayerReferences == null) return false;

            var pdm = bl_GameManager.Instance.LocalPlayerReferences.playerHealthManager;
            if (!pdm.Suicide()) return false;

            bl_UtilityHelper.LockCursor(true);
            if (increaseWarnings)
                bl_GameManager.SuicideCount++;
            //if player is a joker o abuse of suicide, them kick of room
            if (bl_GameManager.SuicideCount > bl_GameData.CoreSettings.maxSuicideAttempts)
            {
                IsAlive = false;
                bl_UtilityHelper.LockCursor(false);
                bl_RoomMenu.Instance.LeaveRoom(RoomLeaveCause.GameLogic);
            }
            return true;
        }

        /// <summary>
        /// Is the Local player alive?
        /// </summary>
        public static bool IsAlive
        {
            get
            {
                return bl_GameManager.Instance == null || bl_GameManager.Instance.LocalActor.isAlive;
            }
            set
            {
                if (bl_GameManager.Instance != null) bl_GameManager.Instance.LocalActor.isAlive = value;
            }
        }

        /// <summary>
        /// Will Return the local player nick name with the clan/crew tag (if any)
        /// </summary>
        /// <returns></returns>
        public static string FullNickName()
        {
            string nick = $"{bl_GameData.Instance.RolePrefix} {bl_PhotonNetwork.NickName}";
            return nick;
        }

        /// <summary>
        /// Get the local player nick name with the player ID
        /// </summary>
        /// <returns></returns>
        public static string GetNickWithID()
        {
            return $"{bl_PhotonNetwork.NickName}:{ID}";
        }

        /// <summary>
        /// Is the local player using third person view?
        /// </summary>
        /// <returns></returns>
        public static bool IsThirdPersonView()
        {
#if MFPSTPV
            return bl_CameraViewSettings.IsThirdPerson();
#else
            return false;
#endif
        }

        /// <summary>
        /// Make the local player ignore colliders
        /// </summary>
        public static void IgnoreColliders(Collider[] colliders, bool ignore)
        {
            if (LocalPlayerReferences == null) return;

            LocalPlayerReferences.playerRagdoll.IgnoreColliders(colliders, ignore);
        }

        /// <summary>
        /// Make the player detect an object when the camera look at it
        /// </summary>
        public static byte AddCameraRayDetection(bl_CameraRayBase.DetecableInfo detecableInfo)
        {
            return LocalPlayerReferences == null ? (byte)0 : LocalPlayerReferences.cameraRay.AddTrigger(detecableInfo);
        }

        /// <summary>
        /// 
        /// </summary>
        public static void RemoveCameraDetection(bl_CameraRayBase.DetecableInfo detecableInfo)
        {
            if (LocalPlayerReferences == null) return;

            LocalPlayerReferences.cameraRay.RemoveTrigger(detecableInfo);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class Network
    {
        /// <summary>
        /// Send a RPC-like call (without a Photon view required) to all other clients in the same room.
        /// </summary>
        public static void SendNetworkCall(byte code, Hashtable data) => bl_PhotonNetwork.Instance.SendDataOverNetwork(code, data);
    }

    /// <summary>
    /// Static functions for the MFPS coins
    /// </summary>
    public static class Coins
    {
        /// <summary>
        /// Get one of the listed coins in GameData by their ID.
        /// </summary>
        /// <param name="coinID"></param>
        /// <returns></returns>
        public static MFPSCoin GetCoinData(int coinID)
        {
            return coinID >= bl_GameData.Instance.gameCoins.Count ? null : bl_GameData.Instance.gameCoins[coinID];
        }

        /// <summary>
        /// Get the coin index/id by the coin data
        /// </summary>
        /// <param name="coin"></param>
        /// <returns></returns>
        public static int GetIndexOfCoin(MFPSCoin coin)
        {
            return GetAllCoins().FindIndex(x => x.CoinName == coin.CoinName);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static List<MFPSCoin> GetAllCoins() => bl_GameData.Instance.gameCoins;
    }

    /// <summary>
    /// 
    /// </summary>
    public static class Utility
    {

        /// <summary>
        /// Check if the given photon player is the local player
        /// </summary>
        /// <param name="player">Player to check if is local</param>
        /// <returns></returns>
        public static bool IsLocalPlayer(Player player)
        {
            return player.ActorNumber == bl_PhotonNetwork.LocalPlayer.ActorNumber;
        }

        /// <summary>
        /// Check if the given viewID is the local player
        /// </summary>
        /// <param name="player">ViewID to check if is local</param>
        /// <returns></returns>
        public static bool IsLocalPlayer(int viewID)
        {
            return viewID == LocalPlayer.ViewID;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class GameData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static bool UsingWaitingRoom()
        {
            return bl_GameData.CoreSettings.lobbyJoinMethod == MFPS.Internal.Structures.LobbyJoinMethod.WaitingRoom;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapName"></param>
        /// <returns></returns>
        public static MapInfo GetMapInfo(string mapName)
        {
            var map = bl_GameData.Instance.AllScenes.Find(x => x.RealSceneName == mapName || x.ShowName == mapName);
            return map;
        }

        /// <summary>
        /// Retrieves all MFPS game items with its basic information.
        /// An item is considered a game item that have an <see cref="MFPSItemUnlockability"/>, e.g weapons, player skins, weapon camos, etc.
        /// </summary>
        /// <returns>A list of MFPSGameItem objects.</returns>
        public static List<MFPSGameItem> GetAllGameItems(bool forceUpdated = false)
        {
            var resultList = new List<MFPSGameItem>();

            if (!forceUpdated && bl_GlobalReferences.I.gameItemCollectors.Count > 0)
            {
                foreach (var type in bl_GlobalReferences.I.gameItemCollectors)
                {
                    var t = Assembly.GetExecutingAssembly().GetType(type.typeName);
                    if (t == null) continue;

                    var method = t.GetMethod(type.methodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method == null) continue;
                    if (method.Invoke(null, null) is List<MFPSGameItem> result)
                    {
                        resultList.AddRange(result);
                    }
                }
            }
            else
            {
                var types = Assembly.GetExecutingAssembly().GetTypes();
                foreach (var type in types)
                {
                    var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(m => m.GetCustomAttributes(typeof(MFPSGameItemCollectorAttribute), false).Length > 0)
                        .Where(m => m.ReturnType == typeof(List<MFPSGameItem>));

                    foreach (var method in methods)
                    {
                        if (method.Invoke(null, null) is List<MFPSGameItem> result)
                        {
                            resultList.AddRange(result);
                        }
                    }
                }
            }

            return resultList;
        }

        /// <summary>
        /// Caches the game item collectors methods.
        /// </summary>
        public static void CacheGameItemCollectors()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            var cachedList = new List<bl_GlobalReferences.MethodInfoCache>();

            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(m => m.GetCustomAttributes(typeof(MFPSGameItemCollectorAttribute), false).Length > 0)
                    .Where(m => m.ReturnType == typeof(List<MFPSGameItem>));

                foreach (var method in methods)
                {
                    if (method.Invoke(null, null) is List<MFPSGameItem> result)
                    {
                        cachedList.Add(new bl_GlobalReferences.MethodInfoCache()
                        {
                            typeName = type.FullName,
                            methodName = method.Name
                        });
                    }
                }
            }

            bl_GlobalReferences.I.gameItemCollectors = cachedList;
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(bl_GlobalReferences.I);
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        [MFPSGameItemCollector]
        private static List<MFPSGameItem> CollectDefaultItems()
        {
            var list = new List<MFPSGameItem>();
            var allWeapons = bl_GameData.Instance.AllWeapons;

            for (int i = 0; i < allWeapons.Count; i++)
            {
                var w = allWeapons[i];
                list.Add(new MFPSGameItem()
                {
                    Name = w.Name,
                    ItemID = i,
                    Icon = w.GunIcon,
                    Unlockability = w.Unlockability
                });
            }
            return list;
        }
    }

    /// <summary>
    /// Functions and properties related to the current room game mode
    /// </summary>
    public static class RoomGameMode
    {
        /// <summary>
        /// Is the current game mode a team based mode or a solo player mode?
        /// </summary>
        public static bool IsOneTeamMode => CurrentGameModeID.IsOneTeamMode();

        /// <summary>
        /// Get the current mode identifier (<see cref="GameMode"/> enum)
        /// </summary>
        public static GameMode CurrentGameModeID => bl_PhotonNetwork.CurrentRoom.GetGameMode();

        /// <summary>
        /// Get the current game mode info (current game mode <see cref="GameModeSettings"/> instance)
        /// </summary>
        public static GameModeSettings CurrentGameModeData => CurrentGameModeID.GetGameModeInfo();

        /// <summary>
        /// Get the current game mode logic (current game mode <see cref="bl_GameModeBase"/> instance)
        /// </summary>
        /// <returns></returns>
        public static bl_GameModeBase CurrentGameModeLogic()
        {
            if (bl_GameManager.Instance == null)
            {
                Debug.LogWarning("The current scene is not a map scene or is not properly setup, can't get the current game mode logic");
                return null;
            }

            return bl_GameManager.Instance.GameModeLogic;
        }

        /// <summary>
        /// Should called when a non player object score in game, ex: AI
        /// This should be called by master client only
        /// If no <paramref name="team"/> is given, the local player team will be used
        /// </summary>
        public static void SetPointToGameMode(int points, GameMode mode, Team team = Team.None)
        {
            if (mode != CurrentGameModeID) return;

            if (team == Team.None)
            {
                team = bl_PhotonNetwork.LocalPlayer.GetPlayerTeam();
            }

            CurrentGameModeLogic().OnLocalPoint(points, team);
        }

        /// <summary>
        /// Assigns the specified number of points to the given team.
        /// </summary>
        /// <remarks>If <paramref name="team"/> is <see cref="Team.None"/>, the method does
        /// nothing.</remarks>
        /// <param name="points">The number of points to assign to the team. Must be a non-negative value.</param>
        /// <param name="team">The team to which the points will be assigned. Must not be <see cref="Team.None"/>.</param>
        public static void SetPointToTeam(int points, Team team)
        {
            if (team == Team.None) return;

            bl_PhotonNetwork.CurrentRoom.SetTeamScore(team, points);
        }

        public static void SetPointToTeam(Team team)
        {
            SetPointToTeam(1, team);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class Weapon
    {
        public static bl_GunInfo GetWeaponInfo(int gunId)
        {
            return gunId < 0 ? null : bl_GameData.Instance.GetWeapon(gunId);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class TagsAndLayer
    {
        /// <summary>
        /// Is the provided collider part of a player? (real player or bot)
        /// </summary>
        /// <param name="col"></param>
        /// <returns></returns>
        public static bool IsPlayerCollider(Collider col)
        {
            return col.CompareTag(LOCAL_PLAYER_TAG) || col.CompareTag(REMOTE_PLAYER_TAG) || col.CompareTag(AI_TAG) || col.CompareTag(HITBOX_TAG);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void OnBeforeSceneLoad()
    {
        LocalPlayer.Team = Team.None;
        LocalPlayer.ViewID = -1;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public static void OnSceneLoaded(Scene scene, LoadSceneMode loadMode)
    {
        bl_EventHandler.onSceneLoaded?.Invoke(new bl_EventHandler.NewSceneInfo()
        {
            IsLobby = bl_Lobby.Instance != null,
            IsRoom = bl_GameManager.Instance != null,
        });

        IsMultiplayerScene = bl_GameManager.Instance != null && bl_RoomSettings.Instance != null;
    }
}
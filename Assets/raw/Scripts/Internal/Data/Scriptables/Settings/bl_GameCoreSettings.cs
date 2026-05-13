using MFPS.Internal.Structures;
using MFPS.Runtime.AI;
using System;
using UnityEngine;

namespace MFPS.Internal.Scriptables
{
    [CreateAssetMenu(fileName = "Game Core Settings", menuName = "MFPS/Settings/Core Settings")]
    public class bl_GameCoreSettings : ScriptableObject
    {
        [Tooltip("Simulate all the network behaviour locally (not for LAN connections)")]
        [LovattoToogle] public bool offlineMode = false;
        [Tooltip("If true, the game will check if another player with the same name is currently connected in the initial connection.")]
        [LovattoToogle] public bool verifySingleSession = true;
        [Tooltip("If true, and Photon Chat is properly setup, the chat window will appear in the Lobby menu.")]
        [LovattoToogle] public bool UseLobbyChat = true;
        [Tooltip("If true, and Photon Voice is properly setup this will active the voice feature.")]
        [LovattoToogle] public bool UseVoiceChat = true;
        [Tooltip("If true, the player will drop the current equipped weapon on death so other players can pick it up.")]
        [LovattoToogle] public bool DropWeaponOnDeath = true;
        [Tooltip("If true, the grenades can inflict damage to the player who throw it.")]
        [LovattoToogle] public bool SelfGrenadeDamage = true;
        [Tooltip("If true, the players will be able to fire while running.")]
        [LovattoToogle] public bool CanFireWhileRunning = true;
        [Tooltip("If true, the players will be able to reload while running.")]
        [LovattoToogle] public bool CanReloadWhileRunning = true;
        [Tooltip("If true, the muzzleflash effect will also show when the player is aiming (in FP view)")]
        [LovattoToogle] public bool showMuzzleFlashWhileAiming = false;
        [Tooltip("True to allow regenerate health.")]
        [LovattoToogle] public bool HealthRegeneration = true;
        [Tooltip("True to show the current health above the teammates heads.")]
        [LovattoToogle] public bool ShowTeamMateHealthBar = true;
        [Tooltip("True to allow players switch teams during gameplay.")]
        [LovattoToogle] public bool canSwitchTeams = false;
        [Tooltip("True to show blood VFX")]
        [LovattoToogle] public bool ShowBlood = true;
        [Tooltip("If true, the game will detect AFK players and kick them from the match.")]
        [LovattoToogle] public bool DetectAFK = false;
        [Tooltip("If true, the mater client (the player who creates the room) can kick other players from the match.")]
        [LovattoToogle] public bool MasterCanKickPlayers = true;
        [Tooltip("Camera shake when the player receive damage?")]
        [LovattoToogle] public bool cameraShakeOnDamage = true;
        [Tooltip("True to show some network statistic in the top left corner of the screen.")]
        [LovattoToogle] public bool ShowNetworkStats = false;
        [Tooltip("If true, the player nick name will be saved locally so when players start a new session they don't have to write the name again.")]
        [LovattoToogle] public bool RememberPlayerName = true;
        [Tooltip("If true, the player equipped weapons will be shown in the bottom right corner.")]
        [LovattoToogle] public bool ShowWeaponLoadout = true;
        [Tooltip("Use a countdown when start a match/round?")]
        [LovattoToogle] public bool useCountDownOnStart = true;
        [Tooltip("Show the crosshair in-game?")]
        [LovattoToogle] public bool showCrosshair = true;
        [Tooltip("Show the the damage direction indicator UI")]
        [LovattoToogle] public bool showDamageIndicator = true;
        [Tooltip("Play the spawn effect on the FP Weapon and arms while spawn protection time is on?")]
        [LovattoToogle] public bool doSpawnHandMeshEffect = true;
        [Tooltip("If true, the player camera will play a delay rotation effect when move the mouse horizontally.")]
        [LovattoToogle] public bool playerViewRolling = true;
        [Tooltip("Players can get fall damage?")]
        [LovattoToogle] public bool allowFallDamage = true;
        [Tooltip("Apply forces to the players ragdolls?\nMomentum, Hit Position, Explosion, etc... ")]
        [LovattoToogle] public bool applyForceToRagdolls = true;
        [Tooltip("If true, drop force will be applied to the ballistic, false = hit-scan")]
        [LovattoToogle] public bool realisticBullets = true;
        [Tooltip("Show bullet decals in game?")]
        [LovattoToogle] public bool bulletDecals = true;
        [Tooltip("If true, the bullets will show the tracer renderer effect.")]
        [LovattoToogle] public bool BulletTracer = false;
        [Tooltip("If true, the projectiles (grenades, launcher, etc...) will show a tracer renderer effect.")]
        [LovattoToogle] public bool showProjectilesTrails = true;
        [Tooltip("If true, defined bad words will be replaced with a *")]
        [LovattoToogle] public bool filterProfanityWords = true;
        [Tooltip("If true, the spectators will be able to see the nameplates of all the players.")]
        [LovattoToogle] public bool showNameplatesOnSpectator = true;
        [Tooltip("If true, player will be able to do quick meele attacks even without a meele equipped in their loadout.")]
        [LovattoToogle] public bool allowFastMeeleWithoutEquip = false;
        [Tooltip("Allow the menu to be open once the match finish? (when the game result screen is shown)")]
        [LovattoToogle] public bool allowMenuWhenMatchOver = false;
        [Tooltip("Automatically load the Room UI additively when open an map scene in the editor?")]
        [LovattoToogle] public bool automaticallyLoadRoomUI = true;
        [Tooltip("If true, the player model will turn the upper body before the lower body (turn in place), this improve visuals and realist of the character animation but at performance cost, that's why is optional.")]
        [LovattoToogle] public bool useTurnInPlace = true;
#if PVOICE
        [LovattoToogle] public bool canHearEnemiesVoiceChat = true;
#endif

        [Tooltip("How to reload the weapons? by replacing the whole magazine, or by the missing bullets?")]
        public AmmoReserveType AmmoType = AmmoReserveType.Bullets;
        public KillFeedWeaponShowMode killFeedWeaponShowMode = KillFeedWeaponShowMode.WeaponIcon;
        [Tooltip("What happens after the player joins to a room?")]
        public LobbyJoinMethod lobbyJoinMethod = LobbyJoinMethod.WaitingRoom;
        public KillCameraType killCameraType = KillCameraType.ObserveDeath;
        [Tooltip("How to show the local player eliminations UI?")]
        public bl_LocalKillNotifier.LocalKillDisplay localKillsShowMode = bl_LocalKillNotifier.LocalKillDisplay.Queqe;
        [Tooltip("Define how to play the weapon change animation sequence.")]
        public ChangeWeaponStyle changeWeaponStyle = ChangeWeaponStyle.HideAndDraw;
        [Tooltip("FP Weapon sway effect")]
        public WeaponSwayMode weaponSwayMode = WeaponSwayMode.RollMode;
        [Tooltip("In which case automatically switch to the pick up weapon?")]
        public bl_GunManager.AutoChangeOnPickup switchToPickupWeapon = bl_GunManager.AutoChangeOnPickup.OnlyOnEmptySlots;
        [Tooltip("What happens when the player manually throws a weapon.")]
        public ThrowWeaponEquipLogic throwWeaponEquipLogic = ThrowWeaponEquipLogic.ChangeToNextAvailableWeaponOrUnarmed;
        [Tooltip("What happens when a player that joins as spectator leave the spectator mode?")]
        public bl_SpectatorModeBase.LeaveSpectatorModeAction onLeaveSpectatorMode = bl_SpectatorModeBase.LeaveSpectatorModeAction.AllowSelectTeam;
        [Tooltip("Determine how the bot eliminations are considered for real player stats")]
        /// <summary>
        /// Determine how the bot eliminations are considered for real player stats
        /// If you want to avoid players being able to level up exploiting bots eliminations to increase their personal stats.
        /// </summary>
        public BotKillConsideration howConsiderBotsEliminations = BotKillConsideration.SameAsRealPlayers;
        [Tooltip("Determine if the player can shoot or not in specific third person scenarios where the player sight to the crosshair is tampered.")]
        public TPFirePointPermission thirdPersonFirePointPermission;
        [Tooltip("If 'allowFastMeeleWithoutEquip' is true, this will be the meele to use for the quick meele attack when no meele is equipped.")]
        [GunID] public int quickMeeleWeaponID = 0;

        public string GameVersion = "1.0";
        public string guestNameFormat = "Guest {0}";
        public string botsNameFormat = "BOT {0}";
        [Tooltip("Time were the player can't receive damage after spawn.")]
        [Range(0, 10)] public int SpawnProtectedTime = 5;
        [Tooltip("Initial countdown time (when countdown is enabled)")]
        [Range(1, 60)] public int CountDownTime = 7;
        [Tooltip("Time that takes to respawn after the player die")]
        [Range(3, 10)] public float PlayerRespawnTime = 5.0f;
        [Tooltip("Maximum number of friends that players can add (and save)")]
        [Range(1, 100)] public int MaxFriendsAdded = 25;
        [Tooltip("Each how many frames detect bullets hits? 1 = each frame")]
        [Range(1, 10)] public int BulletHitDetectionStep = 2;
        [Tooltip("The time that the game resume shown after the match finish.")]
        public int afterMatchCountdown = 10;
        [Tooltip("Time limit that a player can be AFK in a match before get kick out.")]
        public float AFKTimeLimit = 60;
        [Tooltip("Max number of times that a player can change of team per match.")]
        public int MaxChangeTeamTimes = 3;
        [Tooltip("Max number of times that a player can suicide per match.")]
        public int maxSuicideAttempts = 3;
        public string MainMenuScene = "MainMenu";
        public string OnDisconnectScene = "MainMenu";

        public static bl_GameCoreSettings I
        {
            get => bl_GameData.CoreSettings;
        }
    }
}
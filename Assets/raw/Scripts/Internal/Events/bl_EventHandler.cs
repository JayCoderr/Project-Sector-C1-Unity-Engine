using MFPS.Core.Motion;
using MFPS.Internal.Scriptables;
using MFPS.Internal.Structures;
using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class bl_EventHandler
{
    [Serializable] public class UEvent : UnityEvent { }
    public delegate void ObjectsProccessorDelegate(ref object[] data);

    /// <summary>
    /// Event called when the Local player get a kill or get killed in game.
    /// </summary>
    public delegate void LocalKillEvent(KillInfo killInfo);
    public static LocalKillEvent onLocalKill;

    /// <summary>
    /// Event called when a game round finish
    /// </summary>
    public static Action onRoundEnd;

    /// <summary>
    /// Event called when the LOCAL player land a surface after falling
    /// </summary>
    public static Action<float> onPlayerLand;

    /// <summary>
    /// Event called when the LOCAL player die in game
    /// </summary>
    public static Action onLocalPlayerDeath;

    public struct PlayerDeathData
    {
        public MFPSPlayer Player;
        public string KillerName;
    }
    /// <summary>
    /// Event called when a REMOTE player die in game
    /// </summary>
    public static Action<PlayerDeathData> onRemotePlayerDeath;
    public static void DispatchRemotePlayerDeath(PlayerDeathData player) => onRemotePlayerDeath?.Invoke(player);

    /// <summary>
    /// Event called when any (real or bot, local or remote) player die in game
    /// </summary>
    public static Action<PlayerDeathData> onPlayerDeath;

    /// <summary>
    /// Event called when the LOCAL player spawn
    /// </summary>
    public static Action onLocalPlayerSpawn;

    /// <summary>
    /// 
    /// </summary>
    public delegate void LocalPlayerShakeEvent(ShakerPresent present, string key, float influence = 1);
    public static LocalPlayerShakeEvent onLocalPlayerShake;

    /// <summary>
    /// Event called when the local player change one or more post-process effect option in game.
    /// </summary>
    public delegate void EffectChange(bool chrab, bool anti, bool bloom, bool ssao, bool motionb);
    public static EffectChange onEffectChange;

    /// <summary>
    /// Event called when the local player pick up a weapon
    /// </summary>
    public delegate void PickUpWeapon(GunPickUpData e);
    public static PickUpWeapon onPickUpGun;

    /// <summary>
    /// Event Called when the local player change of weapon
    /// </summary>
    /// <param name="GunID"></param>
    public delegate void EventChangeWeapon(int GunID);
    public static EventChangeWeapon onChangeWeapon;

    public struct PlayerChangeData
    {
        public string PlayerName;
        public MFPSPlayer MFPSActor;
        public bool IsAlive;
        public PhotonView NetworkView;
    }
    /// <summary>
    /// Event called when a player that is not the local player spawn or die
    /// </summary>
    public static Action<PlayerChangeData> onRemoteActorChange;
    public static void DispatchRemoteActorChange(PlayerChangeData changeData) => onRemoteActorChange?.Invoke(changeData);

    /// <summary>
    /// Event Called when the room match start
    /// </summary>
    public static Action onMatchStart;
    public static void CallOnMatchStart() { onMatchStart?.Invoke(); }

    /// <summary>
    /// Event Called when the local player change their Aim state
    /// </summary>
    public static Action<bool> onLocalAimChanged;
    public static void DispatchLocalAimEvent(bool isAiming) { onLocalAimChanged?.Invoke(isAiming); }

    /// <summary>
    /// Event called when the local player change an in-game setting/option
    /// </summary>
    public static Action onGameSettingsChange;
    public static void DispatchGameSettingsChange() { onGameSettingsChange?.Invoke(); }

    /// <summary>
    /// Event called when the pause menu is open and hided
    /// <param name="paused">true if the game is paused, false if the game is resumed</param>
    /// </summary>
    public static Action<bool> onGamePause;
    public static void DispatchGamePauseEvent(bool paused) { onGamePause?.Invoke(paused); }

    /// <summary>
    /// Called when the local player shoot a weapon
    /// </summary>
    public static Action<int> onLocalPlayerFire;
    public static void DispatchLocalPlayerFire(int gunID) => onLocalPlayerFire?.Invoke(gunID);

    /// <summary>
    /// Called when the local player hit an enemy.
    /// </summary>
    public static Action<MFPSHitData> onLocalPlayerHitEnemy;
    public static void DispatchLocalPlayerHitEnemy(MFPSHitData hitData) => onLocalPlayerHitEnemy?.Invoke(hitData);

    /// <summary>
    /// Called when the match state change (Waiting, Player, Countdown, etc...), this is called in all the clients
    /// </summary>
    public static Action<MatchState> onMatchStateChanged;
    public static void DispatchMatchStateChange(MatchState state) => onMatchStateChanged?.Invoke(state);

    /// <summary>
    /// Called when the UIMask changed
    /// </summary>
    public static Action<RoomUILayers> onUIMaskChanged;
    public static void DispatchUIMaskChange(RoomUILayers layers) => onUIMaskChanged?.Invoke(layers);

    /// <summary>
    /// Called when a local notification is sent
    /// </summary>
    public static Action<MFPSLocalNotification> onLocalNotification;

    /// <summary>
    /// Called when the player controller state change (from state -> to state)
    /// </summary>
    public static Action<PlayerState, PlayerState> onLocalPlayerStateChanged;
    public static void DispatchLocalPlayerStateChange(PlayerState from, PlayerState to) => onLocalPlayerStateChanged?.Invoke(from, to);

    /// <summary>
    /// Called every time the local player ammo count of the equipped weapon changes
    /// </summary>
    public static Action<int> onLocalPlayerAmmoUpdate;
    public static void DispatchLocalPlayerAmmoUpdate(int newAmmocount) => onLocalPlayerAmmoUpdate?.Invoke(newAmmocount);

    /// <summary>
    /// Called every time that one coins change its value.
    /// </summary>
    public static Action<MFPSCoin> onCoinUpdate;
    public static void DispatchCoinUpdate(MFPSCoin updatedCoin) => onCoinUpdate?.Invoke(updatedCoin);

    /// <summary>
    /// Called when the room properties reset
    /// At the start of a game or new round.
    /// You can use to reset any custom property.
    /// </summary>
    public static Action onRoomPropertiesReset;
    public static void DispatchRoomPropertiesReset() => onRoomPropertiesReset?.Invoke();

    public struct KillAssistData
    {
        public string KilledPlayer;
    }

    /// <summary>
    /// Called when the local player assist with an enemy kill (not the final kill)
    /// </summary>
    public static Action<KillAssistData> onLocalKillAssist;
    public static void DispatchLocalKillAssist(KillAssistData killAssistData) => onLocalKillAssist?.Invoke(killAssistData);

    public struct NewSceneInfo
    {
        public bool IsLobby;
        public bool IsRoom;
    }
    /// <summary>
    /// Called each time a scene is loaded
    /// </summary>
    public static Action<NewSceneInfo> onSceneLoaded;

    /// <summary>
    /// Called when a special event is triggered by the LOCAL player, e.g: plant a bomb, Capture Flag, Kill 5 enemies, etc...
    /// This can be used to show a special notifications, manage a mission system, achievements, etc...
    /// </summary>
    public static Action<string> onGameplayPlayerEvent;
    public static void DispatchGameplayPlayerEvent(string eventID) => onGameplayPlayerEvent?.Invoke(eventID);

    /// <summary>
    /// Called when the LOCAL player change of weapon
    /// </summary>
    public static void ChangeWeaponEvent(int GunID) => onChangeWeapon?.Invoke(GunID);

    /// <summary>
    /// Called this when killed a new player
    /// </summary>
    public static void DispatchLocalKillEvent(KillInfo killInfo) => onLocalKill?.Invoke(killInfo);

    /// <summary>
    /// Call This when room is finish a round
    /// </summary>
    public static void DispatchRoundEndEvent() => onRoundEnd?.Invoke();

    /// <summary>
    /// 
    /// </summary>
    public static void DispatchPlayerLandEvent(float impactAmount) => onPlayerLand?.Invoke(impactAmount);

    /// <summary>
    /// 
    /// </summary>
    public static void DispatchPlayerLocalDeathEvent() => onLocalPlayerDeath?.Invoke();

    /// <summary>
    /// 
    /// </summary>
    public static void DispatchPlayerLocalSpawnEvent() => onLocalPlayerSpawn?.Invoke();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="present"></param>
    /// <param name="key"></param>
    /// <param name="influence"></param>
    public static void DoPlayerCameraShake(ShakerPresent present, string key, float influence = 1) => onLocalPlayerShake?.Invoke(present, key, influence);

    /// <summary>
    /// 
    /// </summary>
    public static void SetEffectChange(bool chrab, bool anti, bool bloom, bool ssao, bool motionb)
    {
        onEffectChange?.Invoke(chrab, anti, bloom, ssao, motionb);
    }

    /// <summary>
    /// 
    /// </summary>
    public static class Player
    {
        /// <summary>
        /// Called when the local player health changed
        /// @params: health / max health
        /// </summary>
        public static Action<int, int> onLocalHealthChanged;

        /// <summary>
        /// Called when the local player change his team
        /// NOT called the first time that joins to a team
        /// </summary>
        public static Action<Team> onLocalChangeTeam;

        /// <summary>
        /// Called when the local player has gain score during the match.
        /// </summary>
        public static Action<int> onAddedScore;

        /// <summary>
        /// Called when the local player do a double jump
        /// </summary>
        public static Action onLocalDoubleJump;

        /// <summary>
        /// Invoked on every local player spawn after the weapon loadout has been equipped and setup.
        /// </summary>
        public static Action onLocalWeaponLoadoutReady;

        /// <summary>
        /// Called when the local player has joined to the game as a player (not spectator)
        /// This is only called once per match, this is not called after switching teams.
        /// </summary>
        public static Action onLocalJoinAsPlayer;

        /// <summary>
        /// Called when the local player has joined to a team
        /// This is called every time the player join to a team, the first time and if switch teams during the match.
        /// </summary>
        public static Action<Team> onLocalJoinTeam;

        /// <summary>
        /// Called when the local player stamina changed
        /// @Param: current stamina, max stamina
        /// </summary>
        public static Action<float, float> onStaminaChange;

        /// <summary>
        /// Called when the local player successfully supply a teammate (ammo, health, etc...)
        /// only called after a teammate has been supplied (picked up) the item.
        /// @Param: The id of the item supplied, could be ammo, health, etc...
        /// </summary>
        public static Action<string> onLocalSupplyingTeammate;

        /// <summary>
        /// Called when the local player equippament changed.
        /// Since in some cases the <see cref="onChangeWeapon"/> event may not be called, e.g: when pick up a weapon or other events, this event is called on every change of the local player equipment.
        /// Note: this event is not called when the player switch the active weapon, only when the equipment has changed.
        /// </summary>
        public static Action onLocalEquipamentChanged;

        /// <summary>
        /// Called when the local player change his class/loadout
        /// </summary>
        public static Action<PlayerClass> onPlayerClassChanged;

        /// <summary>
        /// Called when the local player receive damage from an enemy or any other origin.
        /// </summary>
        public static Action<int> onlocalPlayerReceiveDamage;
    }

    /// <summary>
    /// 
    /// </summary>
    public static class Lobby
    {
        /// <summary>
        /// Called when the lobby connection is fully ready (connected to server and all assets loaded)
        /// </summary>
        public static Action onLobbyReady;

        /// <summary>
        /// Called when start joining a room (creating or joining)
        /// </summary>
        public static Action onBeforeJoiningRoom;

        /// <summary>
        /// Called when the player changes his nickname
        /// </summary>
        public static Action onPlayerNameChanged;
    }

    /// <summary>
    /// 
    /// </summary>
    public static class Match
    {
        /// <summary>
        /// Called when a match is completely over (all rounds ends)
        /// </summary>
        public static Action onMatchOver;

        /// <summary>
        /// Called when the match time is changed
        /// This is called every frame containing the current match time.
        /// @Param: The current match time in seconds.
        /// </summary>
        public static Action<int> onMatchTimeChanged;

        /// <summary>
        /// Called when the in-game menu is active or deactive
        /// </summary>
        public static Action<bool> onMenuActive;

        /// <summary>
        /// Called when the player data in this match is saved
        /// @Param: The player score earned in this match, the final score (kills+win+headshots, etc...)
        /// </summary>
        public static Action<int> onSaveMatchData;

        /// <summary>
        /// Called when the LOCAL player enter or exit from the spectator mode
        /// @Param: enter = true, exit = false
        /// </summary>
        public static Action<bool> onSpectatorModeChanged;

        /// <summary>
        /// Called when the LOCAL player leave a room from other reasons than disconnection.
        /// This is called before the Network leave callback event.
        /// </summary>
        public static Action<RoomLeaveCause> onLeaveRoom;

        /// <summary>
        /// This is called when the player joins to the game as a non-MasterClient player.
        /// The data contain the game state from the MasterClient.
        /// </summary>
        public static Action<Dictionary<string, object>> onGameStateSync;
    }

    /// <summary>
    /// 
    /// </summary>
    public static class Weapon
    {
        /// <summary>
        /// Called when a bullet is fired/instanced
        /// </summary>
        public static Action<bl_ProjectileBase> onBulletFired;

        /// <summary>
        /// Called when the local player start reloading a weapon
        /// @Param: The reload duration in seconds
        /// </summary>
        public static Action<float> onLocalReload;

        /// <summary>
        /// Called when a bullet hit any object
        /// </summary>
        public static Action<Vector3> onBulletHit;

        public struct NadeThrowData
        {
            public int ActorID;
            public int GunID;
            public Team FromTeam;
            public bool IsRemote;
            public Transform ProjectileTransform;

            public readonly bl_GunInfo GunInfo => bl_MFPS.Weapon.GetWeaponInfo(GunID);
        }

        /// <summary>
        /// Called when a player (local, remote, or bot) throw a grenade.
        /// </summary>
        public static Action<NadeThrowData> onNadeThrow;

        /// <summary>
        /// Called when the local player ammo amount changed.
        /// </summary>
        public static Action onLocalAmmoChanged;
    }

    public static class Bots
    {
        /// <summary>
        /// Invoked when a bot hit a player (real or another bot)
        /// </summary>
        public static Action<MFPSHitData> onBotHitPlayer;

        /// <summary>
        /// Called on all clients when a bot die
        /// </summary>
        public static Action<string> onBotDeath;

        /// <summary>
        /// Called once when all the bots info is fetched from the MasterClient
        /// Meaning that the local player have the synchronized bots info.
        /// </summary>
        public static Action onBotsInitializated;
    }

    public static class Gameplay
    {
        public struct AirDropData
        {
            public int ItemId;
            public Vector3 Position;
            public int ActorId;
            public string UniqueID;
        }

        /// <summary>
        /// Called when the local player request an air drop
        /// </summary>
        public static Action<AirDropData> onAirDropRequested;

        /// <summary>
        /// Event called when the LOCAL player pick up a health in game
        /// </summary>
        public delegate void ItemsPickUpEvent(int Amount);
        public static ItemsPickUpEvent onPickUpHealth;

        public struct AmmoPickUpData
        {
            public WeaponSupplyingMode SupplyMode;
            public AmmoType AmmoType;
            public int GunID;
            public int Bullets;
            public int Projectiles;
        }
        /// <summary>
        /// Event called when the LOCAL player pick up ammo in game
        /// </summary>
        public static Func<AmmoPickUpData, bool> onAmmoPickUp;

        /// <summary>
        /// Called whe the player save the game settings (using the in-game settings menu)
        /// </summary>
        public static Action onGameSettingsSaved;

        /// <summary>
        /// Called when the player revert the game settings to the default values.
        /// </summary>
        public static Action onGameSettingsReverted;
    }
}
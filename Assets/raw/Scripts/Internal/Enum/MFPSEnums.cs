using System;

/// <summary>
/// The type of round for the match
/// After finish the rounds, the match will finish and back to the lobby or should start a new match?
/// </summary>
public enum RoundStyle
{
    /// <summary>After finish the match, a new match will start right away.</summary>
    Rounds,
    /// <summary>After finish the match, all the players will return to the lobby</summary>
    OneMatch,
    /// <summary>One match of multiple rounds, after a certain number of rounds have reached, return to the lobby</summary>
    RoundsOneMatch,
}

/// <summary>
/// The state the time of the room
/// </summary>
public enum RoomTimeState
{
    None = 0,
    /// <summary>The time have started and is currently decreasing.</summary>
    Started = 1,
    /// <summary> A countdown is currently happening.</summary>
    Countdown = 2,
    /// <summary>The time is stopped or not started yet, is waiting.</summary>
    Waiting = 3,
    /// <summary>The time is staring after the countdown finish</summary>
    StartedAfterCountdown = 4,
}

/// <summary>
/// 
/// </summary>
[Serializable, Flags]
public enum RoomUILayers
{
    TopScoreBoard = 1,
    WeaponData = 2,
    PlayerStats = 4,
    KillFeed = 8,
    Time = 16,
    Loadout = 32,
    Scoreboards = 64,
    Misc = 128,
}

/// <summary>
/// The cause of the player leave the room
/// </summary>
public enum RoomLeaveCause
{
    /// <summary>The player leave the room intentionally</summary>
    UserAction,
    /// <summary>The game match finish</summary>
    GameFinish,
    /// <summary>Network disconnection</summary>
    Disconnect,
    /// <summary>Due to custom logic of the game (kicked, banned, etc...)</summary>
    GameLogic,
}

/// <summary>
/// The current state of the room match
/// </summary>
public enum MatchState
{
    /// <summary>The game is about to start</summary>
    Starting = 0,
    /// <summary>The match is currently playing</summary>
    Playing = 1,
    /// <summary>The match is waiting for more players to start</summary>
    Waiting = 2,
    /// <summary>The match has finish</summary>
    Finishing = 3,
}

/// <summary>
/// How to manage the bullets of the weapons
/// </summary>
public enum AmmoReserveType
{
    /// <summary>The weapons reloads magazines/clips, remaining bullets in the magazine are wasted after reload(realistic)</summary>
    Clips,
    /// <summary>The weapons reloads the bullets, remaining bullets are keep in the ammo remaining after reload</summary>
    Bullets,
}

/// <summary>
/// The type of recoil for the weapons
/// </summary>
public enum WeaponRecoilType
{
    /// <summary>Increase in vertical direction only</summary>
    SimpleVertical,
    /// <summary>Follows a pattern defined in the recoil settings</summary>
    Pattern,
    /// <summary>Increase vertically with random horizontal deviation.</summary>
    Random2D
}

/// <summary>
/// The input devices sources identifiers
/// By default only Keyboard and Mouse, Xbox and PlayStation controllers are supported.
/// </summary>
public enum MFPSInputSource
{
    KeyboardAndMouse = 0,
    Xbox = 1,
    PlayStation = 2,
    GenericHID = 4,
    SteamDeck = 5,
    SteamController = 6,
    NintendoSwitch = 7,
    Other = 3,
}

/// <summary>
/// Spawn Point selection modes
/// </summary>
[Serializable]
public enum SpawnPointSelectionMode
{
    None = 0,
    /// <summary>
    /// Select a random spawn point from all spawn points
    /// </summary>
    Random = 1,
    /// <summary>
    /// Loop through all spawn points in linear order
    /// </summary>
    Sequential = 2,
    /// <summary>
    /// Select the spawn point with the highest average distance to all players (Freests spawn point)
    /// </summary>
    Freests = 3,
    /// <summary>
    /// Select the spawn point with the highest distance to players
    /// </summary>
    Furthest = 4,
}

/// <summary>
/// Default MFPS KillCam modes
/// </summary>
public enum KillCameraType
{
    None = 0,
    /// <summary>
    /// Orbit the target, allow the player to move the camera around the target.
    /// </summary>
    OrbitTarget,
    /// <summary>
    /// Observe the local player ragdoll after death from a third person view behind the player.
    /// </summary>
    ObserveDeath,
    /// <summary>
    /// If you have your own custom kill cam, you can use this mode to prevent the default kill cam modes to handle it.
    /// </summary>
    Custom
}

/// <summary>
/// Determine how the weapon should be supplied
/// </summary>
public enum WeaponSupplyingMode
{
    ForAllWeapons,
    ForSpecificAmmoType,
    ForSpecificWeapon,
}

public enum ThrowWeaponEquipLogic
{
    /// <summary>
    /// The player will remain in the current slot but change to the unarmed arms.
    /// </summary>
    SameSlotUnarmed,
    /// <summary>
    /// The player will change to the next available weapon slot, if there is no available weapon slot, the player will change to the unarmed arms.
    /// </summary>
    ChangeToNextAvailableWeaponOrUnarmed,
    /// <summary>
    /// Remain in the current slot and do not change to the unarmed arms (empty slot).
    /// </summary>
    EmptySlot
}

[Flags]
public enum TPFirePointPermission
{
    HitTargetEvenIfBarrelIsObstructed = 1,
    HitTargetIfBarrelIsCloseToObstacle = 2,
}
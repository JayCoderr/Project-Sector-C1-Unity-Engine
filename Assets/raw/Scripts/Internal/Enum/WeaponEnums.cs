using System;

/// <summary>
/// The different type of weapons supported by the game.
/// </summary>
public enum GunType
{
    Shotgun = 0,
    Machinegun = 1,
    Sniper = 2,
    Pistol = 3,
    Grenade = 5,
    Melee = 6,
    Launcher = 7,
    None = 99,
}

/// <summary>
/// How the bullet should be instantiated.
/// </summary>
public enum BulletInstanceMethod
{
    Pooled,
    Instanced,
}

/// <summary>
/// How the weapon should be reloaded.
/// Bullet = Reload one bullet at a time.
/// Magazine = Reload the entire magazine.
/// </summary>
public enum ReloadPer
{
    Bullet,
    Magazine,
}

/// <summary>
/// How calculate the projectile direction.
/// </summary>
public enum ProjectileDirectionMode
{
    /// <summary>
    /// Using the parent direction.
    /// </summary>
    ParentDirection,
    /// <summary>
    /// Using the camera direction.
    /// </summary>
    CameraDirection,
    /// <summary>
    /// Handled by the projectile script.
    /// </summary>
    HandleByProjectile
}

/// <summary>
/// Weapon shell shot options.
/// </summary>
public enum ShellShotOptions
{
    /// <summary>
    /// One single shot.
    /// </summary>
    SingleShot,
    /// <summary>
    /// Multiple shots, e.g. shotguns.
    /// </summary>
    PelletShot,
}

/// <summary>
/// How is calculated the time that takes the quick fire.
/// </summary>
public enum WeaponQuickFireTimeMode
{
    /// <summary>
    /// Based on the quick fire animation.
    /// </summary>
    AnimationTime,
    /// <summary>
    /// A fixed time.
    /// </summary>
    FixedTime
}

/// <summary>
/// The different type of weapon ammos
/// </summary>
public enum AmmoType
{
    /// <summary>
    /// For light weapons like pistols, SMGs, etc.
    /// For example: 9mm, .45 ACP
    /// </summary>
    LightAmmo,
    /// <summary>
    /// For heavy weapons like rifles, machine guns, or heavy pistols.
    /// For example: 7.62mm or .50 AE in realistic shooters.
    /// </summary>
    HeavyAmmo,
    /// <summary>
    /// For sniper rifles.
    /// </summary>
    SniperAmmo,
    /// <summary>
    /// For shotguns.
    /// </summary>
    ShotgunAmmo,
    /// <summary>
    /// For grenade launchers/RPG.
    /// </summary>
    LauncherAmmo,
    /// <summary>
    /// For Arrows or bolts.
    /// </summary>
    BoltAmmo,
}

/// <summary>
/// 
/// </summary>
public enum FireReplicationFlag : byte
{
    None = 0,
    BashAttack = 1,
}

public enum ChangeWeaponStyle
{
    /// <summary>
    /// Play half of the hide animation and then draw the new weapon.
    /// </summary>
    HideAndDraw,
    /// <summary>
    /// Instantly hide the current weapon and then draw the new weapon.
    /// </summary>
    CounterStrike,
    /// <summary>
    /// Hide the current weapon and then draw the new weapon.
    /// </summary>
    HideCompletelyAndThenDraw,
}

public enum WeaponSwayMode
{
    NoSway,
    RollMode,
    YawAndPitchMode,
    CounterStrikeStyle,
}

[Flags]
public enum BulletInstanceFlags
{
    None = 0,
    FromMuzzle = 1,
    FromCamera = 2,
}

[Flags]
public enum ProjectileFlags
{
    None = 0,
    NoTrail = 1,
    NoImpact = 2,
    NoDecal = 4,
}
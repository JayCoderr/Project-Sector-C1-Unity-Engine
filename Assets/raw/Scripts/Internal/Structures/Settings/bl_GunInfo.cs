using MFPS.Internal.Structures;
using MFPSEditor;
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class bl_GunInfo
{
    [Header("Info")]
    /// <summary>
    /// Display name of this weapon
    // </summary>
    public string Name;
    /// <summary>
    /// Internal name of this weapon, use to identify this weapon in case the Gun ID change.
    /// </summary>
    [HideInInspector] public string Key;
    /// <summary>
    /// The weapon type of this weapon.
    /// </summary>
    public GunType Type = GunType.Machinegun;
    /// <summary>
    /// Since removing the weapon from GameData cause a lot of problems, this is the alternative way to disable a weapon.
    /// </summary>
    [LovattoToogle] public bool Active = true;

    [Header("Settings")]
    [Range(1, 100)] public int Damage;
    [Range(0.01f, 2f)] public float FireRate = 0.1f;
    [Range(0.5f, 10)] public float ReloadTime = 2.5f;
    [Tooltip("For bullets, this is the max distance the bullet will travel, for nades, this is the explosion radius.")]
    [Range(0, 1000)] public int Range;
    [Range(0.01f, 5)] public float Accuracy;
    [Range(0, 4)] public float Weight;
    public MFPSItemUnlockability Unlockability;

    [Header("References")]
    [SpritePreview(30, true)] public Sprite GunIcon;

    // define how much each specification should influence the overall power of a weapon
    // the first value is the weight of the specification, the second value is the maximum value of the specification
    private readonly Dictionary<string, (float, float)> weights = new()
        {
            {"Damage", (0.25f, 100)},
            {"FireRate", (0.20f, 1)},
            {"Accuracy", (0.20f, 5)},
            {"Weight", (0.10f, 4)},
            {"ReloadTime", (0.15f, 7)},
            {"Range", (0.10f, 1000)}
        };

    private readonly Dictionary<string, (float, float)> sniperWeights = new()
        {
            {"Damage", (0.18f, 110)},
            {"FireRate", (0.12f, 1.5f)},
            {"Accuracy", (0.17f, 5)},
            {"Weight", (0.21f, 5)},
            {"ReloadTime", (0.18f, 6)},
            {"Range", (0.14f, 1500)}
        };

    private readonly Dictionary<string, (float, float)> shotgunWeights = new()
        {
            {"Damage", (0.25f, 60)},
            {"FireRate", (0.10f, 1)},
            {"Accuracy", (0.05f, 5)},
            {"Weight", (0.22f, 5)},
            {"ReloadTime", (0.13f, 7)},
            {"Range", (0.25f, 300)}
        };

    /// <summary>
    /// Can show this weapons in the game lists like class customizer, customizer, unlocks, etc...
    /// </summary>
    /// <returns></returns>
    public bool CanShowWeapon()
    {
        return Active && Unlockability.UnlockMethod != MFPSItemUnlockability.UnlockabilityMethod.Hidden;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weights"></param>
    /// <returns></returns>
    public float CalculatePower()
    {
        var w = weights;
        if (Type == GunType.Sniper) w = sniperWeights;
        else if (Type == GunType.Shotgun) w = shotgunWeights;

        float normalizedDamage = Damage / w["Damage"].Item2;
        float normalizedFireRate = FireRate / w["FireRate"].Item2;
        float normalizedAccuracy = Accuracy / w["Accuracy"].Item2;
        float normalizedWeight = Weight / w["Weight"].Item2;
        float normalizedReloadTime = ReloadTime / w["ReloadTime"].Item2;
        float normalizedRange = Range / w["Range"].Item2;

        float power = (normalizedDamage * w["Damage"].Item1) +
                      ((1 - normalizedFireRate) * w["FireRate"].Item1) + // lower weight is better
                      (normalizedAccuracy * w["Accuracy"].Item1) +
                      ((1 - normalizedWeight) * w["Weight"].Item1) + // lower weight is better
                      ((1 - normalizedReloadTime) * w["ReloadTime"].Item1) + // lower weight is better
                      (normalizedRange * w["Range"].Item1);

        return power;
    }
}
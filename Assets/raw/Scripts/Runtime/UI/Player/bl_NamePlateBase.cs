using System;
using UnityEngine;

/// <summary>
/// Base class for Name Plate Drawer
/// Inherited from this script your custom script where you can handle how to draw the players name in game.
/// </summary>
public abstract class bl_NamePlateBase : bl_MonoBehaviour
{
    [Flags]
    public enum Flags
    {
        None = 0,
        /// <summary>
        /// Allow to show the name plate on enemies
        /// </summary>
        BypassEnemyBlock = 1,
    }

    public Flags DrawerFlags { get; set; } = Flags.None;

    public static bool BlockDraw = false;

    /// <summary>
    /// 
    /// </summary>
    public string PlayerName { get; set; }

    /// <summary>
    /// Set the name to draw
    /// </summary>
    public abstract void SetName(string playerName);

    /// <summary>
    /// 
    /// </summary>
    public abstract void SetActive(bool active);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="flags"></param>
    public virtual void SetFlag(Flags flags)
    {
        DrawerFlags = flags;
    }

    /// <summary>
    /// Set a custom color for the name plate
    /// </summary>
    /// <param name="color"></param>
    public abstract void SetColor(Color color);
}

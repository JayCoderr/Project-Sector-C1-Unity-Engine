using UnityEngine;

[System.Serializable]
public class KillInfo
{
    /// <summary>
    /// The player who killed
    /// </summary>
    public string Killer;
    /// <summary>
    /// The player who was killed
    /// </summary>
    public string Killed;
    public string KillMethod;
    public int GunID = -1;
    public DamageCause Cause;
    /// <summary>
    /// Origin position of the killer weapon
    /// </summary>
    public Vector3 FromPosition;
    /// <summary>
    /// Position of the killed player
    /// </summary>
    public Vector3 ToPosition;
    public bool byHeadShot = false;
    public bool Aiming = false;

    /// <summary>
    /// Distance between the killer and the killed player when the kill happened
    /// </summary>
    public float Distance
    {
        get
        {
            return Vector3.Distance(FromPosition, ToPosition);
        }
    }
}
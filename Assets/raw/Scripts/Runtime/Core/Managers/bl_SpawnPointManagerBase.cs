using System.Collections.Generic;
using UnityEngine;

public abstract class bl_SpawnPointManagerBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public static void AddSpawnPoint(bl_SpawnPointBase point)
    {
        if (Instance == null) return;

        Instance.RegisterSpawnPoint(point);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="point"></param>
    public static void RemoveSpawnPoint(bl_SpawnPointBase point)
    {
        if (Instance == null) return;
        Instance.UnregisterSpawnPoint(point);
    }

    /// <summary>
    /// Get the position and rotation to instance the player from one of the team spawn points in the scene
    /// </summary>
    public abstract void GetPlayerSpawnPosition(Team team, out Vector3 position, out Quaternion rotation);

    /// <summary>
    /// Register a spawnpoint as a valid spawn point
    /// </summary>
    /// <param name="point"></param>
    public abstract void RegisterSpawnPoint(bl_SpawnPointBase point);

    /// <summary>
    /// Remove a spawnpoint from the list of valid spawn points
    /// </summary>
    /// <param name="point"></param>
    public virtual void UnregisterSpawnPoint(bl_SpawnPointBase point)
    {
    }

    /// <summary>
    /// Get the spawnpoint from all the registered points based in the given selector mode. 
    /// </summary>
    /// <returns></returns>
    public virtual bl_SpawnPointBase GetSpawnPointForTeam(Team team) { return GetSpawnPointForTeam(team, SpawnPointSelectionMode.Sequential); }

    /// <summary>
    /// Get the spawnpoint from all the registered points based in the given selector mode. 
    /// </summary>
    /// <returns></returns>
    public abstract bl_SpawnPointBase GetSpawnPointForTeam(Team team, SpawnPointSelectionMode m_spawnMode);

    /// <summary>
    /// Get the list of all the spawnpoints available for the given team
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public abstract List<bl_SpawnPointBase> GetListOfPointsForTeam(Team team);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public virtual bl_SpawnPointBase GetRandomSpawnPoint(Team team) { return GetSpawnPointForTeam(team, SpawnPointSelectionMode.Random); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public virtual bl_SpawnPointBase GetSequentialSpawnPoint(Team team) { return GetSpawnPointForTeam(team, SpawnPointSelectionMode.Sequential); }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public virtual bl_SpawnPointBase GetFreestSpawnPoint(Team team)
    {
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="team"></param>
    /// <returns></returns>
    public virtual bl_SpawnPointBase FindFurthestSpawnPoint(Team team)
    {
        return null;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual bl_SpawnPointBase GetSingleRandom() { return null; }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public virtual bool DrawGizmos() => false;

    private static bl_SpawnPointManagerBase _instance;
    public static bl_SpawnPointManagerBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<bl_SpawnPointManagerBase>();
            }
            return _instance;
        }
    }
}
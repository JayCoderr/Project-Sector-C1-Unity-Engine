using System.Collections.Generic;
using UnityEngine;

public class bl_SpawnPointManager : bl_SpawnPointManagerBase
{
    public SpawnPointSelectionMode spawnSelectionMode = SpawnPointSelectionMode.Random;
    [LovattoToogle] public bool drawSpawnPoints = true;

    private readonly List<bl_SpawnPointBase> spawnPoints = new();
    private int currentSpawnpoint = -1;

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        /// Check if the current game mode has a override for the spawn selection mode
        SpawnPointSelectionMode gameModeSpawnMode = bl_MFPS.RoomGameMode.CurrentGameModeData.overrideSpawnSelectionMode;
        if (gameModeSpawnMode != SpawnPointSelectionMode.None)
        {
            spawnSelectionMode = gameModeSpawnMode;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="point"></param>
    public override void RegisterSpawnPoint(bl_SpawnPointBase point)
    {
        spawnPoints.Add(point);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="point"></param>
    public override void UnregisterSpawnPoint(bl_SpawnPointBase point)
    {
        if (spawnPoints.Contains(point))
        {
            spawnPoints.Remove(point);
        }
    }

    /// <summary>
    /// Get the position and rotation to instance the player from one of the team spawn points in the scene
    /// </summary>
    public override void GetPlayerSpawnPosition(Team team, out Vector3 position, out Quaternion rotation)
    {
        var point = GetSpawnPointForTeam(team);
        if (point == null)
        {
            position = Vector3.zero;
            rotation = Quaternion.identity;
            Debug.LogWarning($"Couldn't found spawnpoint for team {team} in this scene.");
            return;
        }

        point.GetSpawnPosition(out position, out rotation);
    }

    /// <summary>
    /// Get the spawnpoint from all the registered points based in the default selector mode. 
    /// </summary>
    /// <returns></returns>
    public override bl_SpawnPointBase GetSpawnPointForTeam(Team team) => GetSpawnPointForTeam(team, spawnSelectionMode);

    /// <summary>
    /// Get the spawnpoint from all the registered points based in the given selector mode. 
    /// </summary>
    /// <returns></returns>
    public override bl_SpawnPointBase GetSpawnPointForTeam(Team team, SpawnPointSelectionMode m_spawnMode)
    {
        if (spawnPoints.Count <= 0)
        {
            var all = FindObjectsByType<bl_SpawnPointBase>(FindObjectsSortMode.None);
            if (all.Length > 0)
            {
                for (int i = 0; i < all.Length; i++)
                {
                    all[i].Initialize();
                }
            }
            else
            {
                Debug.LogWarning("There's not spawnpoints in this scene.");
                return null;
            }
        }

        switch (m_spawnMode)
        {
            case SpawnPointSelectionMode.Random:
            default:
                return GetRandomSpawnPoint(team);
            case SpawnPointSelectionMode.Sequential:
                return GetSequentialSpawnPoint(team);
            case SpawnPointSelectionMode.Freests:
                return GetFreestSpawnPoint(team);
            case SpawnPointSelectionMode.Furthest:
                return FindFurthestSpawnPoint(team);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public override bl_SpawnPointBase GetRandomSpawnPoint(Team team)
    {
        var teamPoints = GetListOfPointsForTeam(team);
        return teamPoints.Count <= 0 ? null : teamPoints[Random.Range(0, teamPoints.Count)];
    }

    /// <summary>
    /// 
    /// </summary>
    public override bl_SpawnPointBase GetSequentialSpawnPoint(Team team)
    {
        var teamPoints = GetListOfPointsForTeam(team);
        if (teamPoints.Count <= 0) return null;

        if (currentSpawnpoint == -1) currentSpawnpoint = bl_MFPS.LocalPlayer.ViewID % teamPoints.Count;

        currentSpawnpoint = (currentSpawnpoint + 1) % teamPoints.Count;
        return teamPoints[currentSpawnpoint];
    }

    /// <summary>
    /// Return the spawn point with the highest average distance to all players
    /// </summary>
    /// <returns></returns>
    public override bl_SpawnPointBase GetFreestSpawnPoint(Team team)
    {
        bl_SpawnPointBase bestSpawnPoint = null;
        float highestAverageDistance = 0;
        var teamPoints = GetListOfPointsForTeam(team);
        var players = bl_GameManager.GetAllPlayers();

        foreach (bl_SpawnPointBase spawnPoint in teamPoints)
        {
            float totalDistance = 0;
            int playerCount = 0;

            foreach (var player in players)
            {
                float distance = bl_UtilityHelper.SquaredDistance(spawnPoint.transform.position, player.Actor.position);
                totalDistance += distance;
                playerCount++;
            }

            float averageDistance = playerCount > 0 ? totalDistance / playerCount : 0;

            if (averageDistance > highestAverageDistance)
            {
                highestAverageDistance = averageDistance;
                bestSpawnPoint = spawnPoint;
            }
        }

        return bestSpawnPoint;
    }

    /// <summary>
    /// Return the spawn point with the highest minimum distance to all players
    /// </summary>
    /// <returns></returns>
    public override bl_SpawnPointBase FindFurthestSpawnPoint(Team team)
    {
        bl_SpawnPointBase bestSpawnPoint = null;
        float highestMinDistance = 0;
        var players = bl_GameManager.GetAllPlayers();
        var teamPoints = GetListOfPointsForTeam(team);

        foreach (bl_SpawnPointBase spawnPoint in teamPoints)
        {
            float minDistance = float.MaxValue;  // Initialize to a high value

            // we check the distance of the closest player to this spawn point
            foreach (var player in players)
            {
                float distance = bl_UtilityHelper.SquaredDistance(spawnPoint.transform.position, player.Actor.position);
                if (distance < minDistance)
                {
                    minDistance = distance;  // Update minDistance if a closer player is found
                }
            }

            // then we compare this spawn point's minDistance to the highest minDistance found so far
            if (minDistance > highestMinDistance)
            {
                highestMinDistance = minDistance;  // Update highestMinDistance if this spawn point is farther from all players
                bestSpawnPoint = spawnPoint;
            }
        }

        return bestSpawnPoint;
    }

    /// <summary>
    /// Get the list of all the spawnpoints available for the given team
    /// </summary>
    public override List<bl_SpawnPointBase> GetListOfPointsForTeam(Team team)
    {
        if (team == Team.None) team = Team.All;

        var teamPoints = spawnPoints.FindAll(x => x.team == team);
        if (teamPoints.Count <= 0)
        {
            Debug.LogWarning("There's not spawnpoints for the team: " + team.GetTeamName());
            return null;
        }
        return teamPoints;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bl_SpawnPointBase GetSingleRandom() => spawnPoints[Random.Range(0, spawnPoints.Count)];

    public override bool DrawGizmos()
    {
        return drawSpawnPoints;
    }
}
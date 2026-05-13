using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class bl_AIAreas : bl_MonoBehaviour
{
    [Tooltip("Interval in seconds to refresh the hot areas.")]
    public float checkInterval = 2.0f; // Interval in seconds
    [Tooltip("Distance in meters to consider a bot in the same area.")]
    public float detectionRange = 10.0f;
    [Tooltip("Size of each grid cell in meters.")]
    public float cellSize = 10.0f; // Size of each grid cell
    [Tooltip("Number of grid cells in each direction.")]
    public int gridSubdivision = 6;
    [Tooltip("Minimum number of bots to consider an area hot")]
    public int hotAreaThreshold = 4; // Minimum number of bots to consider an area hot
    [Tooltip("Visualize the grid and show debug information in the editor.")]
    [LovattoToogle] public bool ShowDebug = false;

    private readonly Dictionary<Vector2Int, (int team1Count, int team2Count)> grid = new();
    private float timeSinceLastCheck = 0f;

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        timeSinceLastCheck += Time.deltaTime;
        if (timeSinceLastCheck >= checkInterval)
        {
            UpdateGrid();
            timeSinceLastCheck = 0f;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void UpdateGrid()
    {
        grid.Clear();
        var players = bl_GameManager.Instance.OthersActorsInScene;

        foreach (var player in players)
        {
            if (player.Actor == null) { continue; }

            Vector2Int cell = GetCell(player.Actor.position);

            if (!grid.ContainsKey(cell))
            {
                grid[cell] = (0, 0);
            }

            grid[cell] = player.Team == Team.Team2
                ? ((int team1Count, int team2Count))(grid[cell].team1Count, grid[cell].team2Count + 1)
                : ((int team1Count, int team2Count))(grid[cell].team1Count + 1, grid[cell].team2Count);
        }
    }

    /// <summary>
    /// Check if the bot destination is in an enemy hot area
    /// </summary>
    /// <param name="bot"></param>
    /// <param name="position"></param>
    /// <param name="hotAreaCenter"></param>
    /// <returns></returns>
    public static bool IsInEnemyHotArea(bl_AIShooter bot, Vector3 position, out Vector3 hotAreaCenter)
    {
        if (Instance == null) { hotAreaCenter = Vector3.zero; return false; }

        return Instance.IsBotInEnemyHotArea(bot, position, out hotAreaCenter);
    }

    public bool IsBotInEnemyHotArea(bl_AIShooter bot, Vector3 position, out Vector3 hotAreaCenter)
    {
        Vector2Int cell = GetCell(position);
        hotAreaCenter = Vector3.zero;

        if (grid.ContainsKey(cell))
        {
            Vector3 pos = CachedTransform.position;
            var (team1Count, team2Count) = grid[cell];
            if (bot.AITeam == Team.Team1 && team2Count >= hotAreaThreshold)
            {
                hotAreaCenter = new Vector3((cell.x * cellSize) + (cellSize / 2) + pos.x, 0, (cell.y * cellSize) + (cellSize / 2) + pos.z);
                return true;
            }
            if (bot.AITeam == Team.Team2 && team1Count >= hotAreaThreshold)
            {
                hotAreaCenter = new Vector3((cell.x * cellSize) + (cellSize / 2) + pos.x, 0, (cell.y * cellSize) + (cellSize / 2) + pos.z);
                return true;
            }
        }
        return false;
    }

    Vector2Int GetCell(Vector3 position)
    {
        Vector3 origin = CachedTransform.position;
        int x = Mathf.FloorToInt((position.x - origin.x) / cellSize);
        int z = Mathf.FloorToInt((position.z - origin.z) / cellSize);
        return new Vector2Int(x, z);
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!ShowDebug) return;
        if (grid == null) return;

        foreach (var kvp in grid)
        {
            if (kvp.Value.team1Count >= hotAreaThreshold)
            {
                DrawHotArea(kvp.Key, Team.Team1.GetTeamColor());
            }
            if (kvp.Value.team2Count >= hotAreaThreshold)
            {
                DrawHotArea(kvp.Key, Team.Team2.GetTeamColor());
            }
        }

        // Draw the grid
        DrawGrid();
    }

    void DrawHotArea(Vector2Int cell, Color color)
    {
        Handles.color = color;
        Vector3 center = new((cell.x * cellSize) + (cellSize / 2) + CachedTransform.position.x, CachedTransform.position.y, (cell.y * cellSize) + (cellSize / 2) + CachedTransform.position.z);
        Vector3 size = new(cellSize, 0, cellSize);
        Handles.DrawWireCube(center, size);
        color.a = 0.3f;
        Handles.color = color;
        DrawFilledRect(center, Quaternion.LookRotation(Vector3.up), cellSize);
    }

    void DrawGrid()
    {
        Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        Vector3 origin = CachedTransform.position;

        // Determine the bounds for the grid drawing
        float minX = origin.x - (cellSize * gridSubdivision);
        float maxX = origin.x + (cellSize * gridSubdivision);
        float minZ = origin.z - (cellSize * gridSubdivision);
        float maxZ = origin.z + (cellSize * gridSubdivision);

        for (float x = minX; x <= maxX; x += cellSize)
        {
            Handles.DrawLine(new Vector3(x, origin.y, minZ), new Vector3(x, origin.y, maxZ));
        }

        for (float z = minZ; z <= maxZ; z += cellSize)
        {
            Handles.DrawLine(new Vector3(minX, origin.y, z), new Vector3(maxX, origin.y, z));
        }
    }

    private void DrawFilledRect(Vector3 position, Quaternion rotation, float size)
    {
        var matrix = Handles.matrix;
        Handles.matrix = Matrix4x4.TRS(position, Quaternion.identity, new Vector3(1, 0.001f, 1));
        Handles.CubeHandleCap(0, Vector3.zero, rotation, size, EventType.Repaint);
        Handles.matrix = matrix;
    }
#endif

    private static bl_AIAreas _instance;
    public static bl_AIAreas Instance
    {
        get
        {
            if (_instance == null) { _instance = FindAnyObjectByType<bl_AIAreas>(); }
            return _instance;
        }
    }
}

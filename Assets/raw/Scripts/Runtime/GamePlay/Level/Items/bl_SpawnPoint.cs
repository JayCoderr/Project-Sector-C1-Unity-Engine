using UnityEngine;
using System.Collections.Generic;
using MFPSEditor;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Default spawnpoint implementation
/// If you wanna to use your own spawnpoint system simply inherited your script from bl_SpawnPointBase.cs
/// </summary>
public class bl_SpawnPoint : bl_SpawnPointBase
{

    [System.Serializable]
    public enum Shape
    {
        Cube,
        Dome
    }

    public Shape shape = Shape.Dome;
    public float SpawnSpace = 3f;
    [Tooltip("Detection ground limit")]
    public float groundSnapLimit = 1.5f;
    public float safePositionMargin = 0.1f;
    [Tooltip("Use only positions inside the spawn area that have a 'safe' position from other possible positions inside.")]
    [LovattoToogle] public bool useSafePositions = true;
    [Tooltip("Should the player spawn looking at the spawn direction or a random direction?")]
    [LovattoToogle] public bool randomRotation = false;
    public List<GameMode> forGameModes;

    [HideInInspector] public List<Vector3> safePositions;

    RaycastHit hitInfo;
    private const float BASE_VERTICAL_THRESHOLD = 0.01f; // if your players fall of the map after spawn, try to increase this value.
    private const float playerRadius = 0.42f;
    private const int MAX_SAFE_POSITIONS = 1000;

    /// <summary>
    /// 
    /// </summary>
    void OnEnable()
    {
        if (!AvailableForCurrentMode())
        {
            gameObject.SetActive(false);
            return;
        }

        // Make sure to add this spawnpoint to the list of available points
        // Otherwise your spawnpoint will never be used.
        Initialize();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        // if the spawnpoint is disabled, remove it from the list of available points
        bl_SpawnPointManagerBase.RemoveSpawnPoint(this);
    }

    /// <summary>
    /// returns a spawn position confined to this spawnpoint's settings
    /// </summary>
    public override void GetSpawnPosition(out Vector3 position, out Quaternion Rotation)
    {
        GetSpawnPosition(SpawnSpace, out position, out Rotation);
    }

    /// <summary>
    /// returns a spawn position with the given offset
    /// </summary>
    public void GetSpawnPosition(float radiusSpace, out Vector3 position, out Quaternion Rotation)
    {
        position = GetPositionInsideShape(radiusSpace);
        position = SnapPositionToGround(position);

        if (randomRotation) Rotation = Quaternion.Euler(Vector3.up * Random.Range(0, 360));
        else Rotation = transform.rotation;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Vector3 GetPositionInsideShape(float space)
    {
        Vector3 s = transform.position;
        if (shape == Shape.Dome)
        {
            if (!useSafePositions)
            {
                s = Random.insideUnitSphere * space;
            }
            else
            {
                s = safePositions.Count > 0 ? safePositions[Random.Range(0, safePositions.Count)] : transform.position;
            }
            s = transform.position + new Vector3(s.x, BASE_VERTICAL_THRESHOLD, s.z);
        }
        else if (shape == Shape.Cube)
        {
            if (!useSafePositions)
            {
                Vector3 localSize = transform.InverseTransformDirection(transform.localScale * 0.45f); // 0.05 of offset for the player collider radius
                s.x += Random.Range(-localSize.x, localSize.x);
                s.z += Random.Range(-localSize.z, localSize.z);
            }
            else
            {
                s = safePositions.Count > 0 ? safePositions[Random.Range(0, safePositions.Count)] : transform.position;
                s = transform.position + new Vector3(s.x, BASE_VERTICAL_THRESHOLD, s.z);
            }
        }

        return s;
    }

    /// <summary>
    /// 
    /// </summary>
    public Vector3 SnapPositionToGround(Vector3 position)
    {
        if (Physics.SphereCast(new Ray(position + (Vector3.up * groundSnapLimit), Vector3.down), 0.1f, out hitInfo, groundSnapLimit * 2.0f, bl_GameData.TagsAndLayerSettings.EnvironmentOnly))
        {
            if (hitInfo.collider != null) position.y = hitInfo.point.y + BASE_VERTICAL_THRESHOLD;
        }

        return position;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool AvailableForCurrentMode()
    {
        if (forGameModes == null || forGameModes.Count == 0) return true;

        return forGameModes.Exists(x => x == bl_MFPS.RoomGameMode.CurrentGameModeID);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public List<Vector3> CalculateFittingCirclePositions()
    {
        if (shape == Shape.Cube)
        {
            return CalculateFittingCirclePositionsInRectangle();
        }

        List<Vector3> positions = new();
        float bigCircleRadius = SpawnSpace;

        int count = 0;
        // Step by twice the radius + twice the margin to ensure no overlaps and account for the margin
        for (float x = -bigCircleRadius; x <= bigCircleRadius; x += (2 * playerRadius) + (2 * safePositionMargin))
        {
            for (float z = -bigCircleRadius; z <= bigCircleRadius; z += (2 * playerRadius) + (2 * safePositionMargin))
            {
                Vector3 position = new(x, 0, z);
                if (IsCircleInsideBigCircle(position, bigCircleRadius, playerRadius))
                {
                    positions.Add(position);
                    count++;
                    if (count >= MAX_SAFE_POSITIONS)
                    {
                        return positions;
                    }
                }
            }
        }

        return positions;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public List<Vector3> CalculateFittingCirclePositionsInRectangle()
    {
        List<Vector3> positions = new();
        var rectangleTransform = transform;

        Vector3 rectSize = rectangleTransform.localScale;

        // This considers both the diameter of the circle and the margin around it.
        float totalCircleDiameter = 2 * playerRadius + safePositionMargin;

        // Calculate the number of circles that will fit in width and height
        int countX = Mathf.FloorToInt(rectSize.x / totalCircleDiameter);
        int countZ = Mathf.FloorToInt(rectSize.z / totalCircleDiameter);

        // Calculate the effective size of the grid we'll be placing circles in
        float effectiveWidth = countX * totalCircleDiameter;
        float effectiveHeight = countZ * totalCircleDiameter;

        // Start from the bottom-left corner
        Vector3 startingLocalPos = new(-effectiveWidth / 2 + playerRadius, 0, -effectiveHeight / 2 + playerRadius);

        int count = 0;
        for (int i = 0; i < countX; i++)
        {
            for (int j = 0; j < countZ; j++)
            {
                Vector3 localPosition = startingLocalPos + new Vector3(i * totalCircleDiameter, 0, j * totalCircleDiameter);

                // Rotate the localPosition using rectangle's rotation
                localPosition = bl_MathUtility.RotatePointAroundOrigin(localPosition, -rectangleTransform.eulerAngles.y);
                positions.Add(localPosition);

                count++;
                if (count >= MAX_SAFE_POSITIONS)
                {
                    return positions;
                }
            }
        }

        return positions;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="position"></param>
    /// <param name="bigRadius"></param>
    /// <param name="smallRadius"></param>
    /// <returns></returns>
    private bool IsCircleInsideBigCircle(Vector3 position, float bigRadius, float smallRadius)
    {
        // Check distance from center (ignoring Y, which is always 0 in this case)
        float distanceToCenter = new Vector2(position.x, position.z).magnitude;

        // If the distance to the center plus the small circle's radius plus the margin is less than or equal to 
        // the big circle's radius, then the small circle (with the margin) is entirely inside the big circle
        return distanceToCenter + smallRadius + safePositionMargin <= bigRadius;
    }

#if UNITY_EDITOR
    DomeGizmo _gizmo = null;

    /// <summary>
    /// 
    /// </summary>
    void OnDrawGizmosSelected()
    {
        Draw();
        DrawPositionsInside();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmos()
    {
        if (bl_SpawnPointManagerBase.Instance == null || !bl_SpawnPointManagerBase.Instance.DrawGizmos()) return;
        Draw();
    }

    /// <summary>
    /// 
    /// </summary>
    public void BakeSafePositions()
    {
        safePositions = CalculateFittingCirclePositions();
    }

    /// <summary>
    /// 
    /// </summary>
    void DrawPositionsInside()
    {
        if (!useSafePositions) return;
        if (safePositions == null)
        {
            BakeSafePositions();
        }
        if (safePositions == null) return;

        Gizmos.color = new Color(1, 1, 1, 0.33f);
        foreach (var position in safePositions)
        {
            Vector3 pos = transform.position + position;
            bl_GizmosUtility.DrawWireArc(pos, 0.4f, 360, 8, Quaternion.identity);
        }
    }

    void Draw()
    {
        if (bl_SpawnPointManagerBase.Instance == null || Application.isPlaying) return;

        if (shape == Shape.Dome)
        {
            float h = 180;
            if (_gizmo == null || _gizmo.horizon != h)
            {
                _gizmo = new DomeGizmo(h);
            }
        }

        Color c = Color.white;
        if (team != Team.All && team != Team.None) { c = MFPSTeam.Get(team).TeamColor; }
        c.a = 0.3f;
        Gizmos.color = c;

        if (shape == Shape.Dome)
        {
            _gizmo.Draw(transform, c, SpawnSpace);

        }
        else if (shape == Shape.Cube)
        {
            var matrix = Gizmos.matrix;

            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
            Gizmos.color = c;
            Vector3 basePos = Vector3.zero;
            basePos.y += 1.1f;
            Gizmos.DrawCube(basePos, new Vector3(transform.localScale.x, 2.2f, transform.localScale.z));
            Gizmos.DrawWireCube(basePos, new Vector3(transform.localScale.x, 2.2f, transform.localScale.z));
            Gizmos.matrix = matrix;
        }

        if (bl_GlobalReferences.I.SpawnPointPlayerGizmo != null)
        {
            Gizmos.DrawMesh(bl_GlobalReferences.I.SpawnPointPlayerGizmo, transform.position, transform.rotation, Vector3.one * 2.75f);
        }
        if (!randomRotation)
            Gizmos.DrawLine(transform.position + ((transform.forward * SpawnSpace)), transform.position + (((transform.forward * 2f) * SpawnSpace)));
        Gizmos.color = Color.white;
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(bl_SpawnPoint))]
public class bl_SpawnPointEditor : Editor
{
    public bl_SpawnPoint script;

    private void OnEnable()
    {
        script = (bl_SpawnPoint)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();

        script.team = (Team)EditorGUILayout.EnumPopup("Team", script.team);
        script.shape = (bl_SpawnPoint.Shape)EditorGUILayout.EnumPopup("Shape", script.shape);
        if (script.shape == bl_SpawnPoint.Shape.Dome)
        {
            script.SpawnSpace = EditorGUILayout.FloatField("Spawn Radius", script.SpawnSpace);
        }

        var r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toggle);
        script.useSafePositions = MFPSEditorStyles.FeatureToogle(r, script.useSafePositions, "Use Safe Positions", "Calculate points inside the area to avoid stacking the players.");
        if (script.useSafePositions)
        {
            script.safePositionMargin = EditorGUILayout.FloatField("Safe Position Margin", script.safePositionMargin);
        }
        script.groundSnapLimit = EditorGUILayout.FloatField("Ground Snap Limit", script.groundSnapLimit);
        r = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toggle);
        script.randomRotation = MFPSEditorStyles.FeatureToogle(r, script.randomRotation, "Random Rotation", "Spawn the players using a random rotation, or to the foward direction of the spawn point?");
        EditorGUI.indentLevel++;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("forGameModes"), new GUIContent("Only For Game Modes", "If empty, this spawn point will be used in all game modes.\nIf you assign game modes in the list, the spawn point will only be available for those game modes."), true);
        EditorGUI.indentLevel--;

        if (script.shape == bl_SpawnPoint.Shape.Cube && script.useSafePositions)
        {
            GUILayout.Space(4);
            if (GUILayout.Button("Refresh Safe Positions"))
            {
                script.BakeSafePositions();
                serializedObject.ApplyModifiedProperties();
                EditorUtility.SetDirty(target);
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            script.BakeSafePositions();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }
}
#endif
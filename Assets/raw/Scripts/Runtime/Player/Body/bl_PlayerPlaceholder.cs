using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class bl_PlayerPlaceholder : MonoBehaviour, IRealPlayerReference
{
    public Camera playerCamera = null;
    [SerializeField] private Transform headTransform = null;
    [SerializeField] private Transform tpWeaponHolder = null;
    [SerializeField] private Transform fpWeaponHolder = null;

    public bool CalibratingAim { get; set; } = false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="container"></param>
    public void PlaceFPWeaponContainer(Transform container)
    {
        container.SetParent(fpWeaponHolder, true);
        container.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="container"></param>
    public void PlaceTPWeaponContainer(Transform container)
    {
        if (tpWeaponHolder.childCount > 0)
        {
            Debug.LogWarning("The placeholder player does already have a weapon container instance.");
        }

        container.SetParent(tpWeaponHolder, true);
        container.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public static bl_PlayerPlaceholder GetInstance()
    {
        var script = FindAnyObjectByType<bl_PlayerPlaceholder>();
        if (script != null) { return script; }

        var go = Instantiate(bl_GlobalReferences.I.PlayerPlaceholder);
        go.name = "Player Placeholder";

        return go.GetComponent<bl_PlayerPlaceholder>();
    }

    #region Debug Crosshair Draw
    /// <summary>
    /// 
    /// </summary>
    private void OnGUI()
    {
        DoDrawCenterLines();
    }

    /// <summary>
    /// 
    /// </summary>
    void DoDrawCenterLines()
    {
        if (!CalibratingAim) return;

        /*float lineThickness = 1;
        var rect = new Rect(0, (Screen.height * 0.5f) - (lineThickness * 0.5f), Screen.width, lineThickness);
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);

        rect = new Rect((Screen.width * 0.5f) - (lineThickness * 0.5f), 0, lineThickness, Screen.height);
        GUI.DrawTexture(rect, Texture2D.whiteTexture, ScaleMode.StretchToFill);*/

        // Ensure drawing occurs only during repaint events for efficiency
        if (Event.current.type != EventType.Repaint)
            return;

        float lineThickness = 1f;

        // Draw horizontal line
        DrawCrosshairLineHorizontal(lineThickness);

        // Draw vertical line
        DrawCrosshairLineVertical(lineThickness);
    }

    private void DrawCrosshairLineHorizontal(float lineThickness)
    {
        float y = Screen.height * 0.5f - lineThickness * 0.5f;

        // Define the lengths
        float totalWidth = Screen.width;
        float centerLength = 50f; // Length of the center dotted line in pixels

        // Calculate the start and end positions of the center dotted line
        float centerStartX = (totalWidth * 0.5f) - (centerLength * 0.5f);
        float centerEndX = (totalWidth * 0.5f) + (centerLength * 0.5f);

        // Draw left solid line
        Rect leftRect = new Rect(0, y, centerStartX, lineThickness);
        GUI.DrawTexture(leftRect, Texture2D.whiteTexture);

        // Draw right solid line
        Rect rightRect = new Rect(centerEndX, y, totalWidth - centerEndX, lineThickness);
        GUI.DrawTexture(rightRect, Texture2D.whiteTexture);

        // Draw center dotted line
        float dotWidth = 4f; // Width of each dot
        float gapWidth = 4f; // Width of each gap
        DrawDottedLineHorizontal(centerStartX, centerEndX, y, dotWidth, lineThickness, gapWidth);
    }

    private void DrawCrosshairLineVertical(float lineThickness)
    {
        float x = (Screen.width * 0.5f) - (lineThickness * 0.5f);

        // Define the lengths
        float totalHeight = Screen.height;
        float centerLength = 50f; // Length of the center dotted line in pixels

        // Calculate the start and end positions of the center dotted line
        float centerStartY = (totalHeight * 0.5f) - (centerLength * 0.5f);
        float centerEndY = (totalHeight * 0.5f) + (centerLength * 0.5f);

        // Draw top solid line
        Rect topRect = new Rect(x, 0, lineThickness, centerStartY);
        GUI.DrawTexture(topRect, Texture2D.whiteTexture);

        // Draw bottom solid line
        Rect bottomRect = new Rect(x, centerEndY, lineThickness, totalHeight - centerEndY);
        GUI.DrawTexture(bottomRect, Texture2D.whiteTexture);

        // Draw center dotted line
        float dotHeight = 4f; // Height of each dot
        float gapHeight = 4f; // Height of each gap
        DrawDottedLineVertical(x, centerStartY, centerEndY, lineThickness, dotHeight, gapHeight);
    }

    private void DrawDottedLineHorizontal(float startX, float endX, float y, float dotWidth, float dotHeight, float gapWidth)
    {
        // Use the built-in white texture
        Texture2D dotTexture = Texture2D.whiteTexture;

        // Calculate total dotted line length
        float dottedLineLength = endX - startX;

        // Calculate number of dots and gaps
        float patternLength = dotWidth + gapWidth;
        int patternCount = Mathf.FloorToInt(dottedLineLength / patternLength);

        // Adjust startX to center the pattern
        float totalPatternLength = patternCount * patternLength;
        float extraSpace = dottedLineLength - totalPatternLength;
        float adjustedStartX = startX + (extraSpace / 2);

        // Loop to draw each dot along the horizontal line
        for (int i = 0; i < patternCount; i++)
        {
            float x = adjustedStartX + (i * patternLength);
            Rect rect = new Rect(x, y, dotWidth, dotHeight);
            GUI.DrawTexture(rect, dotTexture);
        }
    }

    private void DrawDottedLineVertical(float x, float startY, float endY, float dotWidth, float dotHeight, float gapHeight)
    {
        // Use the built-in white texture
        Texture2D dotTexture = Texture2D.whiteTexture;

        // Calculate total dotted line length
        float dottedLineLength = endY - startY;

        // Calculate number of dots and gaps
        float patternLength = dotHeight + gapHeight;
        int patternCount = Mathf.FloorToInt(dottedLineLength / patternLength);

        // Adjust startY to center the pattern
        float totalPatternLength = patternCount * patternLength;
        float extraSpace = dottedLineLength - totalPatternLength;
        float adjustedStartY = startY + (extraSpace / 2);

        // Loop to draw each dot along the vertical line
        for (int i = 0; i < patternCount; i++)
        {
            float y = adjustedStartY + (i * patternLength);
            Rect rect = new Rect(x, y, dotWidth, dotHeight);
            GUI.DrawTexture(rect, dotTexture);
        }
    }
    #endregion

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_WorldWeaponsContainer GetTPContainerInstance()
    {
        return GetComponentInChildren<bl_WorldWeaponsContainer>(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bl_ViewWeaponsContainer GetFPContainerInstance()
    {
        return GetComponentInChildren<bl_ViewWeaponsContainer>(true);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public Camera GetPlayerCamera()
    {
        return playerCamera;
    }

    public bl_PlayerReferences GetReferenceRoot() => null;

    private void OnDrawGizmos()
    {
        if (playerCamera == null) return;

        Gizmos.color = new Color(0.38394f, 1f, 0.1273585f, 0.33f);
        Gizmos.DrawLine(transform.position, headTransform.position);
        Gizmos.DrawLine(headTransform.position, headTransform.position + (headTransform.forward * 0.4f));

#if UNITY_EDITOR
        // show a handle circle at the head position
        Handles.color = new Color(0.38394f, 1f, 0.1273585f, 0.33f);
        Handles.DrawWireDisc(headTransform.position, headTransform.forward, 0.2f);
        // show a handle circle at the transform position facing up
        Handles.DrawWireDisc(transform.position, transform.up, 0.5f);
#endif
    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(bl_PlayerPlaceholder))]
public class bl_PlayerPlaceholderEditor : Editor
{
    public bl_PlayerPlaceholder script;
    public bool showSky = false;
    private bool wasShowingSky = false;

    private void OnEnable()
    {
        script = (bl_PlayerPlaceholder)target;
        if (script.playerCamera != null)
        {
            showSky = script.playerCamera.clearFlags == CameraClearFlags.Skybox;
        }
        wasShowingSky = showSky;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GUILayout.Space(EditorGUIUtility.singleLineHeight);

        showSky = GUILayout.Toggle(showSky, "Show Skybox", EditorStyles.miniButton);
        if (showSky != wasShowingSky)
        {
            if (script.playerCamera != null)
            {
                script.playerCamera.clearFlags = showSky ? CameraClearFlags.Skybox : CameraClearFlags.SolidColor;
                script.playerCamera.cullingMask = showSky ? -1 : 1 << 8;
            }
            wasShowingSky = showSky;
        }
    }
}
#endif
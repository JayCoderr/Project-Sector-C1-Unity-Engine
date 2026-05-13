using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class bl_GizmosUtility
{

    /// <summary>
    /// Draws a wireframe cylinder as a Gizmo, starting from the base position.
    /// </summary>
    /// <param name="basePosition">The base position of the cylinder (bottom center).</param>
    /// <param name="radius">Radius of the cylinder.</param>
    /// <param name="height">Height of the cylinder.</param>
    /// <param name="segments">Number of segments to define the circular bases.</param>
    /// <param name="gizmoColor">Color of the cylinder.</param>
    public static void DrawWireCylinderFromBase(Vector3 basePosition, float radius, float height, int segments, Color gizmoColor)
    {
        // Set Gizmo color
        Gizmos.color = gizmoColor;

        // The bottom center is the base position, and the top center is offset by the height
        Vector3 bottomCenter = basePosition;
        Vector3 topCenter = basePosition + Vector3.up * height;

        // Draw the top and bottom circles of the cylinder
        DrawCircle(bottomCenter, radius, segments);
        DrawCircle(topCenter, radius, segments);

        // Draw lines connecting the top and bottom circles
        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2;
            float nextAngle = ((i + 1) / (float)segments) * Mathf.PI * 2;

            Vector3 bottomPoint = bottomCenter + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Vector3 topPoint = topCenter + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;

            Gizmos.DrawLine(bottomPoint, topPoint);
        }
    }

    // Helper function to draw a circle in the XZ plane
    private static void DrawCircle(Vector3 center, float radius, int segments)
    {
        for (int i = 0; i < segments; i++)
        {
            float angle = (i / (float)segments) * Mathf.PI * 2;
            float nextAngle = ((i + 1) / (float)segments) * Mathf.PI * 2;

            Vector3 pointA = center + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius;
            Vector3 pointB = center + new Vector3(Mathf.Cos(nextAngle), 0, Mathf.Sin(nextAngle)) * radius;

            Gizmos.DrawLine(pointA, pointB);
        }
    }

    /// <summary>
    /// 
    /// </summary>

    public static void DrawWireCircle(Vector3 center, float radius, int segments = 20, Quaternion rotation = default)
    {
        DrawWireArc(center, radius, 360, segments, rotation);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void DrawWireArc(Vector3 center, float radius, float angle, int segments = 20, Quaternion rotation = default)
    {
        var old = Gizmos.matrix;
        Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);
        Vector3 from = Vector3.forward * radius;
        var step = Mathf.RoundToInt(angle / segments);
        for (int i = 0; i <= angle; i += step)
        {
            var to = new Vector3(radius * Mathf.Sin(i * Mathf.Deg2Rad), 0, radius * Mathf.Cos(i * Mathf.Deg2Rad));
            Gizmos.DrawLine(from, to);
            from = to;
        }

        Gizmos.matrix = old;
    }

    /// <summary>
    /// 
    /// </summary>
    public static void DrawArc(Vector3 center, float radius, float angle, Vector3 normal, Color color = default, float alpha = 0.4f)
    {
#if UNITY_EDITOR
        if (color == default) color = Color.white;
        color.a *= alpha;
        Handles.color = color;
        Handles.DrawSolidArc(center, normal, Quaternion.Euler(0, 0, -angle / 2) * Vector3.forward, angle, radius);
        Handles.color = Color.white;
#endif
    }

    /// <summary>
    /// Draw a solid plain rectangle at the base of the object.
    /// </summary>
    public static void DrawSolidRectangle(Vector3 center, Vector3 size, Quaternion rotation)
    {
        center.y -= size.y / 2;
        size.y = 0.001f;
        Gizmos.matrix = Matrix4x4.TRS(center, rotation, size);
        Gizmos.DrawCube(Vector3.zero, Vector3.one);
        Gizmos.matrix = Matrix4x4.identity;
    }
}
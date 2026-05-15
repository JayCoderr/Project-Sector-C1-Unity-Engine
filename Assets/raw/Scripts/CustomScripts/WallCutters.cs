using UnityEngine;

[ExecuteAlways]
public class WallCutter : MonoBehaviour
{
    public Vector3 size = new Vector3(2, 2, 0.5f);

    public Color gizmoColor =
        new Color(1, 0, 0, 0.25f);

    // =====================================================
    // ORIENTED BOUNDS DATA
    // =====================================================

    public Matrix4x4 GetMatrix()
    {
        return Matrix4x4.TRS(
            transform.position,
            transform.rotation,
            size
        );
    }

    public Vector3 Right => transform.right;
    public Vector3 Up => transform.up;
    public Vector3 Forward => transform.forward;

    // =====================================================
    // CHECK IF WORLD POSITION IS INSIDE ROTATED BOX
    // =====================================================

    public bool ContainsPoint(Vector3 worldPoint)
    {
        Vector3 local =
            transform.InverseTransformPoint(worldPoint);

        Vector3 half = size * 0.5f;

        return
            Mathf.Abs(local.x) <= half.x &&
            Mathf.Abs(local.y) <= half.y &&
            Mathf.Abs(local.z) <= half.z;
    }

    // =====================================================
    // DRAW GIZMOS WITH ROTATION
    // =====================================================

    void OnDrawGizmos()
    {
        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(
            transform.position,
            transform.rotation,
            Vector3.one
        );

        Gizmos.color = gizmoColor;

        Gizmos.DrawCube(
            Vector3.zero,
            size
        );

        Gizmos.color = Color.red;

        Gizmos.DrawWireCube(
            Vector3.zero,
            size
        );

        Gizmos.matrix = oldMatrix;
    }
}
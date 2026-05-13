using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace MFPSEditor
{
    public class MeshSizeChecker : MonoBehaviour
    {
        public Bounds bounds;
        private int callCount = 0;
        public enum CalculateMethod
        {
            MeshBounds,
            MeshVertices
        }
        public CalculateMethod calculateMethod = CalculateMethod.MeshBounds;

        /// <summary>
        /// 
        /// </summary>
        public void Check()
        {
            var meshes = GetComponentsInChildren<SkinnedMeshRenderer>();
            if (meshes == null || meshes.Length <= 0) return;

            bounds = new Bounds(transform.position, Vector3.zero);
            foreach (var item in meshes)
            {
                bounds.Encapsulate(item.bounds);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Bounds CalculateBounds()
        {
            Renderer[] renderers = GetComponentsInChildren<Renderer>();

            if (renderers == null || renderers.Length == 0)
                return new Bounds();

            Vector3 minPoint = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maxPoint = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            foreach (Renderer renderer in renderers)
            {
                Mesh mesh = null;

                if (renderer is MeshRenderer)
                {
                    MeshFilter meshFilter = renderer.gameObject.GetComponent<MeshFilter>();
                    if (meshFilter)
                        mesh = meshFilter.sharedMesh;
                }
                else if (renderer is SkinnedMeshRenderer)
                {
                    SkinnedMeshRenderer skinnedMeshRenderer = renderer as SkinnedMeshRenderer;
                    mesh = skinnedMeshRenderer.sharedMesh;
                }

                if (mesh)
                {
                    Vector3[] vertices = mesh.vertices;

                    foreach (Vector3 vertex in vertices)
                    {
                        // Convert local vertex position to world position
                        Vector3 worldVertex = renderer.transform.TransformPoint(vertex);

                        minPoint = Vector3.Min(minPoint, worldVertex);
                        maxPoint = Vector3.Max(maxPoint, worldVertex);
                    }
                }
            }

            Vector3 center = (minPoint + maxPoint) / 2f;
            Vector3 size = maxPoint - minPoint;

            return new Bounds(center, size);
        }

        public void CalculateHeight(bool forced = false)
        {
            if (calculateMethod == CalculateMethod.MeshVertices)
            {
                if (bounds == null || callCount % 60 == 0 || forced)
                {
                    bounds = CalculateBounds();
                }
                callCount++;
            }
            else
            {
                Check();
            }
        }

        public float Height => bounds == null ? 1 : bounds.size.y;

        private void OnDrawGizmos()
        {
#if UNITY_EDITOR

            CalculateHeight();
            if (bounds == null) return;

            GUI.skin.label.richText = true;
            Vector3 right = transform.TransformDirection(Vector3.right);

            var bottomRightSide = (bounds.center + (right * (bounds.extents.x + 0.1f))) + (Vector3.down * bounds.extents.y);
            var topRightSide = bottomRightSide + (Vector3.up * bounds.size.y);

            if (bounds.size.y >= 1.91f && bounds.size.y <= 2.24f) Gizmos.color = Color.green;
            else Gizmos.color = Color.yellow;

            Gizmos.DrawLine(bottomRightSide, topRightSide);
            Gizmos.DrawLine(bottomRightSide + (-right), bottomRightSide + (right * 0.1f));
            Gizmos.DrawLine(topRightSide + (-right), topRightSide + (right * 0.1f));
            Handles.Label(bottomRightSide + (Vector3.up * bounds.extents.y), $"  <color=yellow>Model Size\n  {bounds.size.y.ToString("0.00")}m</color>");

            Gizmos.color = new Color(0.5f, 1, 0.5f, 0.5f);

            bottomRightSide = bottomRightSide + right * 0.3f;
            topRightSide = topRightSide + right * 0.3f;
            topRightSide.y = bottomRightSide.y + 2;
            Gizmos.DrawLine(bottomRightSide, topRightSide);
            Gizmos.DrawLine(bottomRightSide + (-right * 0.1f), bottomRightSide + (right * 0.1f));
            Gizmos.DrawLine(topRightSide + (-right * 0.1f), topRightSide + (right * 0.1f));
            Handles.Label(topRightSide + new Vector3(0.07f, 0.08f, 0), $"<color=#80FF80>2m</color>");

            Gizmos.color = Color.white;
#endif
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class ProceduralWall : MonoBehaviour
{
    public float height = 3f;
    public float thickness = 0.2f;

    public float pointSpacing = 2f;

    public Transform reference;

    public List<Vector3> points = new List<Vector3>();

    public List<WallCutter> cutters =
        new List<WallCutter>();

    private List<GameObject> generated =
        new List<GameObject>();

    public float connectDistance = 1.5f;

    private List<(int a, int b)> connectionPairs =
        new List<(int, int)>();

    private GameObject floorObject;
    private GameObject ceilingObject;

    void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            GenerateWall();
            GenerateFloorAndCeiling();

            SceneView.RepaintAll();
        }
#endif
    }

    // =========================================================
    // ADD POINT
    // =========================================================

    public void AddPoint()
    {
        Vector3 basePos;

        if (points.Count == 0)
            basePos = reference
                ? reference.position
                : transform.position;
        else
            basePos = points[points.Count - 1];

        Vector3 dir =
            reference
            ? reference.forward
            : Vector3.forward;

        points.Add(basePos + dir * pointSpacing);
    }

    // =========================================================
    // ADD CUTTER
    // =========================================================

    public void AddWindowCutter()
    {
#if UNITY_EDITOR

        GameObject obj =
            new GameObject("Wall Cutter");

        obj.transform.position =
            points.Count > 0
            ? transform.TransformPoint(points[0])
            : transform.position;

        obj.transform.rotation =
            Quaternion.identity;

        WallCutter cutter =
            obj.AddComponent<WallCutter>();

        cutters.Add(cutter);

        Selection.activeGameObject = obj;

#endif
    }

    // =========================================================
    // CONNECT CLOSEST
    // =========================================================

    public void ConnectClosestPoints()
    {
        if (points == null || points.Count < 2)
            return;

        float bestDist = float.MaxValue;

        int aIndex = -1;
        int bIndex = -1;

        for (int i = 0; i < points.Count; i++)
        {
            for (int j = i + 1; j < points.Count; j++)
            {
                float d =
                    Vector3.Distance(
                        points[i],
                        points[j]);

                if (d < bestDist)
                {
                    bestDist = d;

                    aIndex = i;
                    bIndex = j;
                }
            }
        }

        if (aIndex == -1 || bIndex == -1)
            return;

        connectionPairs.Add((aIndex, bIndex));

        GenerateWall();
        GenerateFloorAndCeiling();
    }

    // =========================================================
    // GENERATE WALL
    // =========================================================

    public void GenerateWall()
    {
        ClearGenerated();

        if (points == null || points.Count < 2)
            return;

        // =====================================================
        // MAIN SEGMENTS (WITH CUTTING)
        // =====================================================

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 localA = points[i];
            Vector3 localB = points[i + 1];

            bool wasCut = false;

            for (int c = 0; c < cutters.Count; c++)
            {
                if (cutters[c] == null)
                    continue;

                WallCutter cutter = cutters[c];

                // IMPORTANT: pass cutter directly (no Bounds anymore)
                if (TrySplitWall(localA, localB, cutter))
                {
                    wasCut = true;
                    break;
                }
            }

            if (!wasCut)
            {
                CreateSegment(
                    localA,
                    localB,
                    $"Wall_{i}");
            }
        }

        // =====================================================
        // CONNECTIONS (UNCHANGED)
        // =====================================================

        for (int i = 0; i < connectionPairs.Count; i++)
        {
            int aIndex = connectionPairs[i].a;
            int bIndex = connectionPairs[i].b;

            if (aIndex < 0 || bIndex < 0)
                continue;

            if (aIndex >= points.Count ||
                bIndex >= points.Count)
                continue;

            CreateSegment(
                points[aIndex],
                points[bIndex],
                $"Connection_{i}");
        }
    }

    // =========================================================
    // SPLIT WALL
    // =========================================================

    bool TrySplitWall(
        Vector3 localA,
        Vector3 localB,
        WallCutter cutter)
    {
        Vector3 worldA =
            transform.TransformPoint(localA);

        Vector3 worldB =
            transform.TransformPoint(localB);

        Vector3 wallDir =
            (worldB - worldA).normalized;

        float wallLength =
            Vector3.Distance(worldA, worldB);

        Vector3 wallUp = Vector3.up;

        Vector3 wallRight =
            Vector3.Cross(wallUp, wallDir).normalized;

        Vector3 toCenter =
            cutter.transform.position - worldA;

        float forward =
            Vector3.Dot(toCenter, wallDir);

        float right =
            Vector3.Dot(toCenter, wallRight);

        float up =
            Vector3.Dot(toCenter, wallUp);

        // =====================================================
        // MUST BE ON WALL SEGMENT
        // =====================================================

        if (forward < -1f || forward > wallLength + 1f)
            return false;

        // =====================================================
        // PROPER CUTTER SIZE USAGE
        // =====================================================

        float halfWidth =
            cutter.size.x * 0.5f;

        float halfHeight =
            cutter.size.y * 0.5f;

        float halfDepth =
            cutter.size.z * 0.5f;

        // =====================================================
        // INSIDE CHECK (2D + thickness)
        // =====================================================

        if (Mathf.Abs(right) > halfWidth)
            return false;

        if (Mathf.Abs(up) > halfHeight)
            return false;

        if (Mathf.Abs(forward - wallLength * 0.5f) > wallLength * 0.6f)
        {
            // optional loosened check (prevents over-rejection)
        }

        // =====================================================
        // CUT RANGE (FIXED - USE DEPTH NOT WIDTH)
        // =====================================================

        float cutSize = Mathf.Max(halfWidth, halfDepth);

        float startDist =
            forward - cutSize;

        float endDist =
            forward + cutSize;

        startDist =
            Mathf.Clamp(startDist, 0, wallLength);

        endDist =
            Mathf.Clamp(endDist, 0, wallLength);

        // =====================================================
        // SAFETY: ENSURE REAL SPLIT EXISTS
        // =====================================================

        if (Mathf.Abs(endDist - startDist) < 0.01f)
            return false;

        // =====================================================
        // LEFT PIECE
        // =====================================================

        if (startDist > 0.01f)
        {
            Vector3 leftEnd =
                worldA + wallDir * startDist;

            CreateSegmentWorld(
                worldA,
                leftEnd,
                "LeftSplit");
        }

        // =====================================================
        // RIGHT PIECE
        // =====================================================

        if (endDist < wallLength - 0.01f)
        {
            Vector3 rightStart =
                worldA + wallDir * endDist;

            CreateSegmentWorld(
                rightStart,
                worldB,
                "RightSplit");
        }

        return true;
    }

    // =========================================================
    // CREATE SEGMENT
    // =========================================================

    void CreateSegment(
        Vector3 aLocal,
        Vector3 bLocal,
        string name)
    {
        Vector3 a =
            transform.TransformPoint(aLocal);

        Vector3 b =
            transform.TransformPoint(bLocal);

        CreateSegmentWorld(a, b, name);
    }

    void CreateSegmentWorld(
        Vector3 a,
        Vector3 b,
        string name)
    {
        GameObject segment =
            GameObject.CreatePrimitive(
                PrimitiveType.Cube);

        DestroyImmediate(
            segment.GetComponent<Collider>());

        segment.name = name;

        segment.transform.parent =
            transform;

        Vector3 dir = b - a;

        float length =
            dir.magnitude;

        segment.transform.position =
            (a + b) * 0.5f;

        segment.transform.rotation =
            Quaternion.LookRotation(
                dir.normalized);

        segment.transform.localScale =
            new Vector3(
                thickness,
                height,
                length);

        segment.transform.position +=
            Vector3.up * (height * 0.5f);

        generated.Add(segment);
    }

    // =========================================================
    // CLEAR GENERATED
    // =========================================================

    public void ClearGenerated()
    {
        for (int i = 0; i < generated.Count; i++)
        {
            if (generated[i] != null)
            {
#if UNITY_EDITOR
                DestroyImmediate(generated[i]);
#else
                Destroy(generated[i]);
#endif
            }
        }

        generated.Clear();
    }

    public void ClearConnections()
    {
        connectionPairs.Clear();
    }

    // =========================================================
    // FLOOR / CEILING
    // =========================================================

    public void GenerateFloorAndCeiling()
    {
        ClearFloorCeiling();

        if (points == null || points.Count < 3)
            return;

        Vector3[] floorVerts =
            new Vector3[points.Count];

        Vector3[] ceilingVerts =
            new Vector3[points.Count];

        for (int i = 0; i < points.Count; i++)
        {
            floorVerts[i] =
                transform.TransformPoint(points[i]);

            ceilingVerts[i] =
                floorVerts[i] +
                Vector3.up * height;
        }

        floorObject =
            CreateMeshObject(
                "Floor",
                floorVerts,
                false);

        ceilingObject =
            CreateMeshObject(
                "Ceiling",
                ceilingVerts,
                true);
    }

    GameObject CreateMeshObject(
        string name,
        Vector3[] vertsWorld,
        bool flipTriangles = false)
    {
        GameObject obj =
            new GameObject(name);

        obj.transform.parent =
            transform;

        MeshFilter mf =
            obj.AddComponent<MeshFilter>();

        MeshRenderer mr =
            obj.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();

        Vector3[] verts =
            new Vector3[vertsWorld.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            verts[i] =
                obj.transform.InverseTransformPoint(
                    vertsWorld[i]);
        }

        List<int> tris =
            new List<int>();

        for (int i = 1; i < verts.Length - 1; i++)
        {
            if (!flipTriangles)
            {
                tris.Add(0);
                tris.Add(i);
                tris.Add(i + 1);
            }
            else
            {
                tris.Add(0);
                tris.Add(i + 1);
                tris.Add(i);
            }
        }

        mesh.vertices = verts;
        mesh.triangles = tris.ToArray();

        mesh.RecalculateNormals();

        mf.sharedMesh = mesh;

        return obj;
    }

    public void ClearFloorCeiling()
    {
#if UNITY_EDITOR

        if (floorObject != null)
            DestroyImmediate(floorObject);

        if (ceilingObject != null)
            DestroyImmediate(ceilingObject);

#else

        if (floorObject != null)
            Destroy(floorObject);

        if (ceilingObject != null)
            Destroy(ceilingObject);

#endif
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    void OnDrawGizmos()
    {
        if (points == null || points.Count < 2)
            return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < points.Count - 1; i++)
        {
            Vector3 a =
                transform.TransformPoint(points[i]);

            Vector3 b =
                transform.TransformPoint(points[i + 1]);

            Gizmos.DrawLine(a, b);
        }
    }
}

#if UNITY_EDITOR

[CustomEditor(typeof(ProceduralWall))]
public class ProceduralWallEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ProceduralWall wall =
            (ProceduralWall)target;

        GUILayout.Space(10);

        // =====================================================
        // ADD POINT
        // =====================================================

        if (GUILayout.Button("+ Add Point"))
        {
            Undo.RecordObject(
                wall,
                "Add Point");

            wall.AddPoint();

            EditorUtility.SetDirty(wall);
        }

        // =====================================================
        // CONNECTION
        // =====================================================

        if (GUILayout.Button(
            "🔗 Create Connection"))
        {
            Undo.RecordObject(
                wall,
                "Create Connection");

            wall.ConnectClosestPoints();

            EditorUtility.SetDirty(wall);
        }

        // =====================================================
        // ADD CUTTER
        // =====================================================

        if (GUILayout.Button(
            "🪟 Add Window Cutter"))
        {
            Undo.RecordObject(
                wall,
                "Add Cutter");

            wall.AddWindowCutter();

            EditorUtility.SetDirty(wall);
        }

        // =====================================================
        // REBUILD
        // =====================================================

        if (GUILayout.Button(
            "🔄 Rebuild"))
        {
            wall.GenerateWall();
            wall.GenerateFloorAndCeiling();

            EditorUtility.SetDirty(wall);
        }

        // =====================================================
        // CLEAR GENERATED
        // =====================================================

        if (GUILayout.Button(
            "❌ Clear Generated"))
        {
            wall.ClearGenerated();
            wall.ClearConnections();
            wall.ClearFloorCeiling();

            EditorUtility.SetDirty(wall);
        }

        // =====================================================
        // CLEAR POINTS
        // =====================================================

        if (GUILayout.Button(
            "🗑 Clear Points"))
        {
            Undo.RecordObject(
                wall,
                "Clear Points");

            wall.points.Clear();

            EditorUtility.SetDirty(wall);
        }
    }

    void OnSceneGUI()
    {
        HandleUtility.AddDefaultControl(
            GUIUtility.GetControlID(
                FocusType.Passive));

        ProceduralWall wall =
            (ProceduralWall)target;

        if (wall.points == null)
            return;

        bool changed = false;

        for (int i = 0; i < wall.points.Count; i++)
        {
            Vector3 worldPos =
                wall.transform.TransformPoint(
                    wall.points[i]);

            EditorGUI.BeginChangeCheck();

            Vector3 newWorldPos =
                Handles.PositionHandle(
                    worldPos,
                    Quaternion.identity);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(
                    wall,
                    "Move Point");

                wall.points[i] =
                    wall.transform.InverseTransformPoint(
                        newWorldPos);

                EditorUtility.SetDirty(wall);

                changed = true;
            }
        }

        if (changed)
        {
            wall.GenerateWall();
            wall.GenerateFloorAndCeiling();
        }

        Event e = Event.current;

        if (e.type == EventType.KeyDown &&
            e.keyCode == KeyCode.C)
        {
            wall.ConnectClosestPoints();

            wall.GenerateWall();
            wall.GenerateFloorAndCeiling();

            EditorUtility.SetDirty(wall);

            e.Use();
        }
    }
}

#endif
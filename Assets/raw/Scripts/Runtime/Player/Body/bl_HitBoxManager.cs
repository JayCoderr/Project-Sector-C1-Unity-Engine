using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using MFPSEditor;
#endif

public class bl_HitBoxManager : bl_HitboxManagerBase
{
    public List<bl_HitBoxBase> hitBoxes = new();
    public bool managerPerSegments = true;
    public bl_PlayerHealthManagerBase healthManager;

    /// <summary>
    /// Called when a hitbox get hit
    /// </summary>
    public override void OnHit(DamageData damageData, bl_HitBoxBase hitbox)
    {
        if (healthManager != null)
            healthManager.DoDamage(damageData);
    }

    /// <summary>
    /// 
    /// </summary>
    public override void FetchAllChildHitBoxes(bool includeDeactive = true, bool selfAssign = true)
    {
        var all = transform.GetComponentsInChildren<bl_HitBoxBase>(includeDeactive);
        if (selfAssign)
        {
            foreach (var item in all)
            {
                item.hitBoxManager = this;
            }
        }
        hitBoxes.Clear();
        hitBoxes.AddRange(all);
    }

    public override bl_HitBoxBase GetHitbox(HumanBodyBones bone)
    {
        return hitBoxes.Find(x => x.hitBoxInfo.Bone == bone);
    }

    /// <summary>
    /// Called from editor only
    /// Do not use in runtime.
    /// </summary>
    public void SetupHitboxes(Animator animator)
    {
        hitBoxes.Clear();
        List<Collider> colliders = new();
        colliders.AddRange(transform.GetComponentsInChildren<Collider>());

        if (colliders.Count > 0)
        {
            CreateHitBox(HumanBodyBones.LeftUpperLeg, animator);
            CreateHitBox(HumanBodyBones.LeftLowerLeg, animator);
            CreateHitBox(HumanBodyBones.RightUpperLeg, animator);
            CreateHitBox(HumanBodyBones.RightLowerLeg, animator);
            CreateHitBox(HumanBodyBones.Spine, animator);
            CreateHitBox(HumanBodyBones.Hips, animator);
            CreateHitBox(HumanBodyBones.LeftUpperArm, animator);
            CreateHitBox(HumanBodyBones.LeftLowerArm, animator);
            CreateHitBox(HumanBodyBones.Head, animator);
            CreateHitBox(HumanBodyBones.RightUpperArm, animator);
            CreateHitBox(HumanBodyBones.RightLowerArm, animator);
        }
        else
        {
            Debug.LogWarning("This player has not been ragdolled yet.");
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="bone"></param>
    /// <param name="animator"></param>
    void CreateHitBox(HumanBodyBones bone, Animator animator)
    {
        var box = new HitBoxInfo();
        if (!animator.GetBoneTransform(bone).TryGetComponent<Collider>(out var col))
        {
            Debug.LogWarning("The bone: " + bone.ToString() + " doesn't have a collider.");
            return;
        }

        col.gameObject.layer = LayerMask.NameToLayer("Player");
        col.gameObject.tag = bl_MFPS.HITBOX_TAG;
        box.Bone = bone;
        box.collider = col;
        if (bone == HumanBodyBones.Head) { box.DamageMultiplier = 5; }

        var bp = col.GetComponent<bl_HitBoxBase>() ?? col.gameObject.AddComponent<bl_HitBox>();
        bp.hitBoxManager = this;
        bp.hitBoxInfo = box;
#if UNITY_EDITOR
        EditorUtility.SetDirty(col);
        EditorUtility.SetDirty(col.gameObject);
#endif
        hitBoxes.Add(bp);
    }
}

//EDITOR DRAWER
#if UNITY_EDITOR
[CustomEditor(typeof(bl_HitBoxManager))]
public class bl_BodyHitBoxManagerEditor : Editor
{
    bl_HitBoxManager script;
    readonly float[] multipliers = new float[4] { 1, 1, 1, 1 };
    readonly Color backColor = new(0, 0, 0, 0.35f);

    private void OnEnable()
    {
        script = (bl_HitBoxManager)target;
        if (script.hitBoxes.Exists(x => x.hitBoxInfo.Bone == HumanBodyBones.Head)) { multipliers[0] = script.hitBoxes.Find(x => x.hitBoxInfo.Bone == HumanBodyBones.Head).hitBoxInfo.DamageMultiplier; }
        if (script.hitBoxes.Exists(x => x.hitBoxInfo.Bone == HumanBodyBones.LeftUpperArm)) { multipliers[1] = script.hitBoxes.Find(x => x.hitBoxInfo.Bone == HumanBodyBones.LeftUpperArm).hitBoxInfo.DamageMultiplier; }
        if (script.hitBoxes.Exists(x => x.hitBoxInfo.Bone == HumanBodyBones.Spine)) { multipliers[2] = script.hitBoxes.Find(x => x.hitBoxInfo.Bone == HumanBodyBones.Spine).hitBoxInfo.DamageMultiplier; }
        if (script.hitBoxes.Exists(x => x.hitBoxInfo.Bone == HumanBodyBones.LeftUpperLeg)) { multipliers[3] = script.hitBoxes.Find(x => x.hitBoxInfo.Bone == HumanBodyBones.LeftUpperLeg).hitBoxInfo.DamageMultiplier; }
    }

    public override void OnInspectorGUI()
    {
        script = (bl_HitBoxManager)target;
        EditorGUI.BeginChangeCheck();

        script.healthManager = EditorGUILayout.ObjectField("Health Manager", script.healthManager, typeof(bl_PlayerHealthManagerBase), true) as bl_PlayerHealthManagerBase;
        if (script.healthManager == null)
        {
            EditorGUILayout.HelpBox("No Health Manager has been assigned, therefore none damage will be applied from the hitboxes", MessageType.Warning);
        }
        var rect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.toolbarButton);
        script.managerPerSegments = MFPSEditorStyles.FeatureToogle(rect, script.managerPerSegments, "Multiplier value per segment?");
        DrawBodyMultiplier();
        DrawHitBoxesList();

        if (GUILayout.Button("Refresh Hitboxes"))
        {
            script.hitBoxes.Clear();
            var all = script.transform.GetComponentsInChildren<bl_HitBoxBase>(false);
            for (int i = 0; i < all.Length; i++)
            {
                var b = all[i];
                if (b == null) continue;
                b.hitBoxInfo.collider = b.transform.GetComponent<Collider>();
                b.hitBoxManager = script;
                EditorUtility.SetDirty(b);
            }
            script.hitBoxes.AddRange(all);
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }

        if (script.hitBoxes == null || script.hitBoxes.Count <= 0)
        {
            if (GUILayout.Button("Setup HitBoxes"))
            {
                var playerRef = script.transform.GetComponentInParent<bl_PlayerReferencesCommon>();
                if (playerRef == null) return;

                script.SetupHitboxes(playerRef.PlayerAnimator);
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            UpdateMultipliers();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);
        }
    }

    void DrawHitBoxesList()
    {
        GUILayout.Box("HITBOXES");
        var all = script.hitBoxes;
        var prop = serializedObject.FindProperty("hitBoxes");
        for (int i = 0; i < all.Count; i++)
        {
            var box = all[i];
            if (box == null) continue;
            var hbi = box.hitBoxInfo;

            EditorGUILayout.BeginVertical("box");
            var sp = prop.GetArrayElementAtIndex(i);

            var rect = EditorGUILayout.BeginHorizontal();
            {
                EditorGUI.DrawRect(rect, backColor);
                GUILayout.Label(hbi.collider == null ? "" : hbi.collider.name);
                if (!sp.isExpanded) hbi.Bone = (HumanBodyBones)EditorGUILayout.EnumPopup(hbi.Bone, GUILayout.Width(110));
                GUILayout.Space(10);
                EditorGUILayout.ObjectField(box, typeof(bl_HitBoxBase), true, GUILayout.Width(120));
                GUILayout.Space(10);
            }
            EditorGUILayout.EndHorizontal();

            if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
            {
                sp.isExpanded = !sp.isExpanded;
            }
            if (sp.isExpanded)
            {
                if (script.managerPerSegments) GUI.enabled = false;
                hbi.DamageMultiplier = EditorGUILayout.Slider("Damage Multiplier", hbi.DamageMultiplier, 0.1f, 7);
                GUI.enabled = true;
                hbi.Bone = (HumanBodyBones)EditorGUILayout.EnumPopup("Bone", hbi.Bone);
                hbi.collider = EditorGUILayout.ObjectField("Collider", hbi.collider, typeof(Collider), true) as Collider;
            }
            else
            {

            }
            EditorGUILayout.EndVertical();
        }
    }

    void DrawBodyMultiplier()
    {
        if (!script.managerPerSegments) return;

        GUILayout.BeginHorizontal("box");
        Show(9);
        GUILayout.Space(10);
        GUILayout.BeginVertical("box");
        GUILayout.Space(10);
        GUILayout.Label("HEAD DAMAGE MULTIPLIER", EditorStyles.miniBoldLabel);
        multipliers[0] = EditorGUILayout.Slider(multipliers[0], 0.1f, 10);
        GUILayout.Space(30);
        GUILayout.Label("ARMS DAMAGE MULTIPLIER", EditorStyles.miniBoldLabel);
        multipliers[1] = EditorGUILayout.Slider(multipliers[1], 0.1f, 5);
        GUILayout.Space(15);
        GUILayout.Label("CHEST DAMAGE MULTIPLIER", EditorStyles.miniBoldLabel);
        multipliers[2] = EditorGUILayout.Slider(multipliers[2], 0.1f, 5);
        GUILayout.Space(60);
        GUILayout.Label("LEGS DAMAGE MULTIPLIER", EditorStyles.miniBoldLabel);
        multipliers[3] = EditorGUILayout.Slider(multipliers[3], 0.1f, 5);
        GUILayout.Space(20);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    void UpdateMultipliers()
    {
        if (!script.managerPerSegments) return;
        SetValueToBox(HumanBodyBones.Head, multipliers[0]);
        SetValueToBox(HumanBodyBones.LeftUpperArm, multipliers[1]);
        SetValueToBox(HumanBodyBones.LeftLowerArm, multipliers[1]);
        SetValueToBox(HumanBodyBones.RightUpperArm, multipliers[1]);
        SetValueToBox(HumanBodyBones.RightLowerArm, multipliers[1]);
        SetValueToBox(HumanBodyBones.Spine, multipliers[2]);
        SetValueToBox(HumanBodyBones.LeftUpperLeg, multipliers[3]);
        SetValueToBox(HumanBodyBones.LeftLowerLeg, multipliers[3]);
        SetValueToBox(HumanBodyBones.RightUpperLeg, multipliers[3]);
        SetValueToBox(HumanBodyBones.RightLowerLeg, multipliers[3]);
    }

    void SetValueToBox(HumanBodyBones bone, float value)
    {
        if (script.hitBoxes.Exists(x => x.hitBoxInfo.Bone == bone))
        {
            script.hitBoxes.Find(x => x.hitBoxInfo.Bone == bone).hitBoxInfo.DamageMultiplier = value;
        }
    }

    static class Styles
    {
        public static GUIContent UnityDude = EditorGUIUtility.IconContent("AvatarInspector/BodySIlhouette");
        public static GUIContent PickingTexture = EditorGUIUtility.IconContent("AvatarInspector/BodyPartPicker");

        public static GUIContent[] BodyPart =
        {
                EditorGUIUtility.IconContent("AvatarInspector/MaskEditor_Root"),
                EditorGUIUtility.IconContent("AvatarInspector/Torso"),

                EditorGUIUtility.IconContent("AvatarInspector/Head"),

                EditorGUIUtility.IconContent("AvatarInspector/LeftLeg"),
                EditorGUIUtility.IconContent("AvatarInspector/RightLeg"),

                EditorGUIUtility.IconContent("AvatarInspector/LeftArm"),
                EditorGUIUtility.IconContent("AvatarInspector/RightArm"),

                EditorGUIUtility.IconContent("AvatarInspector/LeftFingers"),
                EditorGUIUtility.IconContent("AvatarInspector/RightFingers"),

                EditorGUIUtility.IconContent("AvatarInspector/LeftFeetIk"),
                EditorGUIUtility.IconContent("AvatarInspector/RightFeetIk"),

                EditorGUIUtility.IconContent("AvatarInspector/LeftFingersIk"),
                EditorGUIUtility.IconContent("AvatarInspector/RightFingersIk"),
            };
    }

    protected static Color[] m_MaskBodyPartPicker =
      {
            new(255 / 255.0f,   144 / 255.0f,     0 / 255.0f), // root
            new(0 / 255.0f, 174 / 255.0f, 240 / 255.0f), // body
            new(171 / 255.0f, 160 / 255.0f,   0 / 255.0f), // head

            new(0 / 255.0f, 255 / 255.0f,     255 / 255.0f), // ll
            new(247 / 255.0f,   151 / 255.0f, 121 / 255.0f), // rl

            new(0 / 255.0f, 255 / 255.0f, 0 / 255.0f), // la
            new(86 / 255.0f, 116 / 255.0f, 185 / 255.0f), // ra

            new(255 / 255.0f,   255 / 255.0f,     0 / 255.0f), // lh
            new(130 / 255.0f,   202 / 255.0f, 156 / 255.0f), // rh

            new(82 / 255.0f,    82 / 255.0f,      82 / 255.0f), // lfi
            new(255 / 255.0f,   115 / 255.0f,     115 / 255.0f), // rfi
            new(159 / 255.0f,   159 / 255.0f,     159 / 255.0f), // lhi
            new(202 / 255.0f,   202 / 255.0f, 202 / 255.0f), // rhi

            new(101 / 255.0f,   101 / 255.0f, 101 / 255.0f), // hi
        };

    public void Show(int count)
    {
        if (Styles.UnityDude.image)
        {
            Rect rect = GUILayoutUtility.GetRect(Styles.UnityDude, GUIStyle.none, GUILayout.MaxWidth(Styles.UnityDude.image.width));

            Color oldColor = GUI.color;
            GUI.color = new Color(0.2f, 0.2f, 0.2f, 1.0f);
            GUI.DrawTexture(rect, Styles.UnityDude.image);

            for (int i = 1; i < count; i++)
            {
                GUI.color = GetPartColor(i);
                if (Styles.BodyPart[i].image)
                {
                    GUI.DrawTexture(rect, Styles.BodyPart[i].image);
                }
            }
            GUI.color = oldColor;
        }
    }

    private Color GetPartColor(int id)
    {
        return id switch
        {
            2 => Color.Lerp(Color.yellow, Color.red, multipliers[0] / 5),
            1 => Color.Lerp(Color.yellow, Color.red, multipliers[2] / 5),
            6 or 7 or 8 or 5 => Color.Lerp(Color.yellow, Color.red, multipliers[1] / 5),
            _ => Color.Lerp(Color.yellow, Color.red, multipliers[3] / 5),
        };
    }
}
#endif
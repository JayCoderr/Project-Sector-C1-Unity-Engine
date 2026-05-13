using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public abstract class bl_PlayerModelBase : MonoBehaviour
{
    public Animator animator;
    public bl_PlayerAnimationsBase playerAnimations;
    public bl_PlayerRagdollBase playerRagdoll;
    public bl_HitboxManagerBase hitBoxManager;
    public bl_PlayerIKBase playerIK;
    public bl_RemoteWeapons remoteWeapons;
    public Vector3 positionOffset;
    public Vector3 rotationOffset;
    public Vector3 scale;

    /// <summary>
    /// Fetch the information of the player model
    /// The player model have to be inside the player prefab to get the correct information.
    /// </summary>
    public virtual void FetchInfo()
    {
        animator = GetComponentInChildren<Animator>();
        playerIK = GetComponentInChildren<bl_PlayerIKBase>();
        remoteWeapons = GetComponentInChildren<bl_RemoteWeapons>(true);
        playerAnimations = GetComponent<bl_PlayerAnimationsBase>();
        playerRagdoll = GetComponent<bl_PlayerRagdollBase>();
        hitBoxManager = GetComponent<bl_HitboxManagerBase>();
        positionOffset = transform.localPosition;
        rotationOffset = transform.localEulerAngles;
        scale = transform.localScale;
    }

    /// <summary>
    /// 
    /// </summary>
    public virtual void SetupInPlayer(bl_PlayerReferencesCommon player)
    {
        transform.parent = player.GetThirdPersonModelParent();
        transform.localPosition = positionOffset;
        transform.localEulerAngles = rotationOffset;
        transform.localScale = scale;

        if (player.IsRealPlayer())
        {
            var pRefs = player as bl_PlayerReferences;
            pRefs.playerAnimations = playerAnimations;
            pRefs.playerRagdoll = playerRagdoll;
            pRefs.hitBoxManager = hitBoxManager;
            pRefs.RemoteWeapons = remoteWeapons;
            pRefs.playerIK = playerIK;
            pRefs.PlayerAnimator = animator;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(bl_PlayerModelBase), true)]
public class bl_PlayerModelBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Fetch Info"))
        {
            (target as bl_PlayerModelBase).FetchInfo();
        }
    }
}
#endif
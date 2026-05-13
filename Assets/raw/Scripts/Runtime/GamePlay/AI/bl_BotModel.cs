using UnityEngine;
using UnityEngine.Serialization;

public class bl_BotModel : MonoBehaviour
{
    [Tooltip("EXPERIMENTAL, re-use the same character model trougth the whole match, instead of instance on every dead.")]
    [LovattoToogle, SerializeField] private bool reuseModel = false;
    [FormerlySerializedAs("playerPrefabBiding")]
    [SerializeField] private bl_PlayerReferences playerPrefabBinding = null;
    public int playerSkinId = 0;
    [Tooltip("Offset position to apply to the model")]
    public Vector3 modelOffset = Vector3.zero;
    [Space]
    [SerializeField] private RuntimeAnimatorController botAnimatorController = null;
    [SerializeField] private bl_AIShooterReferences references = null;
    [SerializeField] private Transform modelParent = null;
    [SerializeField] private bl_AIAnimation modelInstance = null;

    public bl_EventHandler.UEvent onModelInit;

    /// <summary>
    /// 
    /// </summary>
    private void Awake()
    {
        SetupModel();
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetupModel()
    {
        if (modelInstance != null)
        {
            onModelInit?.Invoke();
            return;
        }
        if (playerPrefabBinding == null)
        {
            Debug.LogError("Please assign the player prefab biding in the inspector to get the bot character model from.", gameObject);
            return;
        }

        bool isCached = false;
        if (bl_AIMananger.TryGetModel(playerPrefabBinding.name, out GameObject instance))
        {
            if (!reuseModel)
            {
                instance = Instantiate(instance);
            }
            instance.SetActive(true);
            isCached = true;
        }
        else
        {
            instance = playerPrefabBinding.ExtractCharacterModel();

            if (instance.TryGetComponent<bl_PlayerModelSkins>(out var playerSkins))
            {
                playerSkins.SetActiveSkin(playerSkinId);
            }
        }

        // This will work with the default MFPS bl_AIAnimation class only
        // If you're using a custom inherit from bl_AIAnimationBase class, you'll need to modify this code
        bl_AIAnimation aiAnimation;
        Animator animator;

        // if it's the first time we're using this model
        if (!isCached)
        {
            aiAnimation = instance.AddComponent<bl_AIAnimation>();
            aiAnimation.GetRigidBodys();
            aiAnimation.DetachModelOnDeath = reuseModel;
            animator = aiAnimation.GetComponent<Animator>();
            aiAnimation.BotAnimator = animator;
            if (instance.TryGetComponent<bl_PlayerIK>(out var playerIK))
            {
                aiAnimation.HandRotationAdjust = playerIK.HandRotationAdjust;
                aiAnimation.Weight = playerIK.Weight;
                aiAnimation.Body = playerIK.Body;
                aiAnimation.Head = playerIK.Head;
                Destroy(playerIK);
            }
        }
        else
        {
            aiAnimation = instance.GetComponent<bl_AIAnimation>();
            aiAnimation.SetKinecmatic();
            animator = aiAnimation.BotAnimator;
            if (reuseModel)
            {
                animator.enabled = true;
                aiAnimation.enabled = true;
            }
        }

        instance.transform.SetParent(modelParent);
        instance.transform.localPosition = modelOffset;
        instance.transform.localEulerAngles = Vector3.zero;
        if (reuseModel) aiAnimation.SetKinecmatic();

        animator.runtimeAnimatorController = botAnimatorController;
        references.aiAnimation = aiAnimation;
        modelInstance = aiAnimation;
        references.PlayerAnimator = animator;
        references.hitBoxManager.FetchAllChildHitBoxes();
        references.remoteWeapons = instance.transform.GetComponentInChildren<bl_RemoteWeapons>(true);
        references.weaponRoot = references.remoteWeapons.transform;

        onModelInit?.Invoke();

        if (!isCached)
        {
            var cacheCopy = instance;
            if (!reuseModel) cacheCopy = Instantiate(instance);
            cacheCopy.name = instance.name;

            string keyName = reuseModel ? references.GetSyncedName() : playerPrefabBinding.name;
            bl_AIMananger.CacheModel(keyName, cacheCopy, reuseModel == false);
            if (!reuseModel)
            {
                cacheCopy.SetActive(false);
            }
        }
    }
}
using UnityEngine;
using UnityEngine.AI;

public class bl_AIShooterReferences : bl_PlayerReferencesCommon
{
    public bl_AIShooter aiShooter;
    public bl_PlayerHealthManagerBase shooterHealth;
    public bl_AIShooterNetwork shooterNetwork;
    public bl_AIShooterAttackBase shooterWeapon;
    public bl_AIAnimationBase aiAnimation;
    public bl_HitboxManagerBase hitBoxManager;
    public bl_NamePlateBase namePlateDrawer;
    public NavMeshAgent Agent;
    public bl_RemoteWeapons remoteWeapons;
    [SerializeField] private Animator m_playerAnimator;
    public Transform weaponRoot;
    public Transform lookReference;
    public Transform lookAtTarget;
    [SerializeField] private Mesh directionMesh;

    public override Animator PlayerAnimator
    {
        get => m_playerAnimator;
        set => m_playerAnimator = value;
    }

    [SerializeField] private bl_AIPlayerTarget m_botAimTarget = null;
    public override bl_AIPlayerTarget BotAimTarget
    {
        get
        {
            if (m_botAimTarget == null)
            {
                m_botAimTarget = gameObject.AddComponent<bl_AIPlayerTarget>();
                m_botAimTarget.playerReferences = this;
            }
            return m_botAimTarget;
        }
        set => m_botAimTarget = value;
    }

    private Collider[] m_allColliders;
    public override Collider[] AllColliders
    {
        get
        {
            if (m_allColliders == null || m_allColliders.Length <= 0)
            {
                m_allColliders = transform.GetComponentsInChildren<Collider>(true);
            }
            return m_allColliders;
        }
    }

    public override string PlayerName => aiShooter.AIName;

    public override Team PlayerTeam => aiShooter.AITeam;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public string GetSyncedName()
    {
        var view = GetComponent<Photon.Pun.PhotonView>();
        if (view == null || view.InstantiationData == null || view.InstantiationData.Length == 0) return string.Empty;
        return (string)view.InstantiationData[0];
    }

    /// <summary>
    /// 
    /// </summary>
    public override void IgnoreColliders(Collider[] list, bool ignore)
    {
        for (int e = 0; e < list.Length; e++)
        {
            for (int i = 0; i < AllColliders.Length; i++)
            {
                if (AllColliders[i] != null)
                {
                    Physics.IgnoreCollision(AllColliders[i], list[e], ignore);
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool IsDeath()
    {
        return shooterHealth.IsDeath();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetHealth()
    {
        return shooterHealth.GetHealth();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override int GetMaxHealth()
    {
        return shooterHealth.GetMaxHealth();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override bool IsRealPlayer()
    {
        return false;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void OnAttacked(bl_PlayerReferencesCommon attacker)
    {
        aiShooter.OnAttacked(attacker);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public override Transform GetThirdPersonModelParent()
    {
        return hitBoxManager.transform;
    }

    public override bl_HitBoxBase GetBodyHitbox(HumanBodyBones bone)
    {
        if (hitBoxManager == null) return null;
        return hitBoxManager.GetHitbox(bone);
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (directionMesh == null) return;

        Gizmos.color = Color.yellow;
        Vector3 pos = transform.position + new Vector3(0, -1.25f, 0);
        Gizmos.DrawMesh(directionMesh, pos, transform.rotation, Vector3.one * 0.4f);
    }
}
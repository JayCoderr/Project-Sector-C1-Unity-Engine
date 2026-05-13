using Photon.Pun;
using UnityEngine;

/// <summary>
/// This class is used to define a target for the AI to aim at.
/// You can create your own target by inheriting from this class and override the methods.
/// </summary>
public abstract class bl_AITarget : MonoBehaviour
{
    private Transform m_Transform;
    private bool isDeath = false;

    /// <summary>
    /// 
    /// </summary>
    public virtual void Awake()
    {
        m_Transform = transform;
    }

    /// <summary>
    /// 
    /// </summary>
    public void MarkAsDeath()
    {
        isDeath = true;
    }

    /// <summary>
    /// This can be called when this target is attacked (hit or almost hit)
    /// By default this is only called when a bot shoot at this target.
    /// </summary>
    public virtual void OnAttacked(bl_PlayerReferencesCommon attacker)
    {
    }

    /// <summary>
    /// Get the team of this target
    /// </summary>
    /// <returns></returns>
    public abstract Team GetTeam();

    /// <summary>
    /// Get the NetworkView of this target
    /// </summary>
    /// <returns></returns>
    public abstract PhotonView GetNetView();

    /// <summary>
    /// The position of this target
    /// </summary>
    public virtual Vector3 position
    {
        get
        {
            return m_Transform.position;
        }
    }

    /// <summary>
    /// The name of this target
    /// </summary>
    public abstract string Name { get; set; }

    public Transform Transform
    {
        get
        {
            return m_Transform;
        }
    }

    public virtual bool IsDeath
    {
        get
        {
            return isDeath;
        }
    }
}
using System.Collections.Generic;
using UnityEngine;

public class bl_DamageArea : MonoBehaviour
{
    public AreaType m_Type = AreaType.OneTime;
    [Range(1, 100)] public int Damage = 5;

    private bool isPlayerCaused = false;
    private DamageData cacheInformation;
    private readonly List<bl_PlayerHealthManagerBase> AllHitted = new();
    private bool isNetwork = false;
    private int originGunID = -1;

    /// <summary>
    /// 
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        if (other.isLocalPlayerCollider())
        {
            if (other.transform.TryGetComponent<bl_PlayerHealthManagerBase>(out var pdm))
            {
                if (m_Type == AreaType.Repeting)
                {
                    if (isPlayerCaused)
                    {
                        cacheInformation.OriginPosition = transform.position;
                        int gi = originGunID == -1 ? bl_GameData.Instance.GetWeaponID("Molotov") : originGunID;
                        if (gi > 0)
                        {
                            cacheInformation.SetGunID(gi);
                        }

                        pdm.DoRepetingDamage(new bl_PlayerHealthManagerBase.RepetingDamageInfo()
                        {
                            Damage = this.Damage,
                            Rate = 1,
                            DamageData = cacheInformation
                        });
                        AllHitted.Add(pdm);
                    }
                    else
                    {
                        pdm.DoRepetingDamage(new bl_PlayerHealthManagerBase.RepetingDamageInfo()
                        {
                            Damage = this.Damage,
                            Rate = 1,
                        });
                    }
                }
                else if (m_Type == AreaType.OneTime)
                {
                    DamageData info = new()
                    {
                        Damage = Damage,
                        OriginPosition = transform.position,
                        Cause = DamageCause.Fire
                    };
                    pdm.DoDamage(info);
                }
            }
        }
        else if ((other.transform.CompareTag("AI") || other.transform.CompareTag("BodyPart")) && !isNetwork)
        {
            if (isPlayerCaused)
            {
                if (other.transform.root.TryGetComponent<bl_PlayerHealthManagerBase>(out var ash))
                {
                    if (!AllHitted.Contains(ash))
                    {
                        cacheInformation.OriginPosition = transform.position;
                        int gi = originGunID == -1 ? bl_GameData.Instance.GetWeaponID("Molotov") : originGunID;
                        if (gi > 0)
                        {
                            cacheInformation.SetGunID(gi);
                        }
                        ash.DoRepetingDamage(new bl_PlayerHealthManagerBase.RepetingDamageInfo()
                        {
                            Damage = this.Damage,
                            Rate = 1,
                            DamageData = cacheInformation
                        });
                        AllHitted.Add(ash);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnTriggerExit(Collider other)
    {
        if (other.isLocalPlayerCollider())
        {
            if (other.transform.TryGetComponent<bl_PlayerHealthManagerBase>(out var pdm))
            {
                if (m_Type == AreaType.Repeting)
                {
                    pdm.CancelRepetingDamage();
                    AllHitted.Remove(pdm);
                }
            }
        }
        else if (other.transform.CompareTag("AI") && !isNetwork)
        {
            if (other.transform.root.TryGetComponent<bl_PlayerHealthManagerBase>(out var ash))
            {
                if (AllHitted.Contains(ash)) { AllHitted.Remove(ash); }
                if (m_Type == AreaType.Repeting)
                {
                    ash.CancelRepetingDamage();
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        cacheInformation = null;
        foreach (var p in AllHitted)
        {
            if (p == null) continue;
            p.CancelRepetingDamage();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetInfo(BulletData projectileData, bool isNetwork)
    {
        isPlayerCaused = true;
        this.isNetwork = isNetwork;
        cacheInformation = new DamageData
        {
            Cause = DamageCause.Player,
            From = projectileData.MFPSActor.Name,
            Damage = (int)projectileData.Damage,
            MFPSActor = projectileData.MFPSActor,
            ActorViewID = projectileData.ActorViewID,
        };
        Damage = (int)projectileData.Damage;
        originGunID = projectileData.WeaponID;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (!TryGetComponent<SphereCollider>(out var c)) return;
        Gizmos.color = new Color(1, 0, 0, 0.5f);
        Gizmos.DrawSphere(transform.TransformPoint(c.center), c.radius);
        Gizmos.DrawWireSphere(transform.TransformPoint(c.center), c.radius);
    }

    [System.Serializable]
    public enum AreaType
    {
        Repeting,
        OneTime,
    }
}
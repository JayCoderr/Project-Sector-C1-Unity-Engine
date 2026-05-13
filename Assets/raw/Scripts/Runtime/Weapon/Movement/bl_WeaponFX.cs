using UnityEngine;

public class bl_WeaponFX : bl_WeaponFXBase
{
    public ParticleSystem[] highFxParticles;
    public ParticleSystem[] lowFxParticles;

    private ParticleSystem[] targetParticles;

    [Tooltip("In case the vfx are not instanced by default and have to be instanced in runtime.")]
    [SerializeField] private bl_WeaponFX prefab = null;

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        if (targetParticles == null) ActiveDependOfTarget();

        if ((targetParticles == null || targetParticles.Length == 0) && prefab != null)
        {
            var instance = Instantiate(prefab.gameObject, transform.position, Quaternion.identity) as GameObject;
            instance.transform.SetParent(transform);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;

            var instanceScript = instance.GetComponent<bl_WeaponFX>();
            highFxParticles = instanceScript.highFxParticles;
            lowFxParticles = instanceScript.lowFxParticles;

            foreach (var item in highFxParticles)
            {
                if (item == null) continue;
                item.gameObject.SetActive(true);
            }

            foreach (var item in lowFxParticles)
            {
                if (item == null) continue;
                item.gameObject.SetActive(true);
            }

            // delete the instance root and leave only the children
            var root = instance.transform;
            for (int i = 0; i < root.childCount; i++)
            {
                root.GetChild(i).SetParent(transform);
            }

            ActiveDependOfTarget();

            Destroy(instance);
        }
    }

    /// <summary>
    /// Play the weapon fire FX effects
    /// </summary>
    public override void PlayFireFX()
    {
        if (targetParticles == null) ActiveDependOfTarget();

        foreach (var item in targetParticles)
        {
            if (item == null) continue;
            item.Play();
        }
    }

    /// <summary>
    /// Use the particles based on the platform target
    /// high for PC and console platforms
    /// low for mobile platforms
    /// </summary>
    public void ActiveDependOfTarget()
    {
        if (bl_UtilityHelper.isMobile)
        {
            SetActiveList(highFxParticles, false);
            SetActiveList(lowFxParticles, true);
            targetParticles = lowFxParticles;
        }
        else
        {
            SetActiveList(lowFxParticles, false);
            SetActiveList(highFxParticles, true);
            targetParticles = highFxParticles;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public void SetActiveList(ParticleSystem[] list, bool active)
    {
        foreach (var item in list)
        {
            if (item == null) continue;
            item.gameObject.SetActive(active);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        bl_GizmosUtility.DrawWireArc(transform.position, 0.03f, 360, 10, Quaternion.LookRotation(transform.up));
    }

    [ContextMenu("Try FP Auto Place")]
    void AutoPlaceInFPWeapon()
    {
        var weapon = transform.GetComponentInParent<bl_Gun>();
        if (weapon == null) return;

        var firePoint = weapon.muzzlePoint;
        if (firePoint == null) return;

        Vector3 p = firePoint.position + (weapon.transform.forward * 0.05f);
        transform.position = p;
    }
}
using UnityEngine;

public abstract class bl_BulletDecalManagerBase : MonoBehaviour
{
    /// <summary>
    /// Indicates whether decals should be displayed. Defaults to true.
    /// Use this property to enable or disable the display of decals in specific situations.
    /// </summary>
    public bool ShowDecals { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    public static void InstantiateDecal(RaycastHit raycastHit)
    {
        if (Instance == null) return;

        Instance.InstanceDecal(raycastHit);
    }

    /// <summary>
    /// 
    /// </summary>
    public abstract void InstanceDecal(RaycastHit raycastHit);

    /// <summary>
    /// Should clear/hide all the instanced decals.
    /// </summary>
    public virtual void ClearDecals() { }

    /// <summary>
    /// 
    /// </summary>
    private static bl_BulletDecalManagerBase _instance;
    public static bl_BulletDecalManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindAnyObjectByType<bl_BulletDecalManagerBase>(); }
            return _instance;
        }
    }
}
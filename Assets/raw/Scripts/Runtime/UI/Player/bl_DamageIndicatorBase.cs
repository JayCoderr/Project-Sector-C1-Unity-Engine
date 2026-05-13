using UnityEngine;

public abstract class bl_DamageIndicatorBase : bl_MonoBehaviour
{
    public struct HitInfo
    {
        public string Actor;
        public Vector3 Direction;
    }

    /// <summary>
    ///
    /// </summary>
    public abstract void SetHit(HitInfo hitInfo);

    private static bl_DamageIndicatorBase _instance;

    public static bl_DamageIndicatorBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<bl_DamageIndicatorBase>();
            }
            return _instance;
        }
    }
}
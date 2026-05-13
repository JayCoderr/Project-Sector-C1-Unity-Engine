using UnityEngine;
/// <summary>
/// Base class for the player weapon recoil movement
/// Inherited from this your custom recoil script.
/// </summary>
public abstract class bl_RecoilBase : bl_MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    public struct RecoilData
    {
        public WeaponRecoilType RecoilType;
        public float Amount;
        public float Speed;
        public float MaxValue;
        public bl_WeaponRecoilSettings RecoilSettings;

        public RecoilData(float amount)
        {
            Amount = amount;
            Speed = 2;
            MaxValue = 5;
            RecoilSettings = null;
            RecoilType = WeaponRecoilType.SimpleVertical;
        }

        public RecoilData(bl_WeaponRecoilSettings recoilSettings, float extra = 0)
        {
            if (recoilSettings == null)
            {
                Debug.LogWarning("The recoil settings was not provided.");
            }

            Amount = Mathf.Max(recoilSettings.recoilVerticalIntensity + extra, 0);
            MaxValue = recoilSettings.maxRecoilValue;
            Speed = recoilSettings.recoilSmoothness;
            RecoilSettings = recoilSettings;
            RecoilType = recoilSettings.recoilType;
        }
    };

    /// <summary>
    /// 
    /// </summary>
    /// <param name="amount"></param>
    public abstract void SetRecoil(RecoilData data);

}
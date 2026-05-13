using TMPro;
using UnityEngine;

public class bl_EquippedWeaponUI : bl_EquippedWeaponUIBase
{
    [SerializeField] private GameObject content = null;
    [SerializeField] private TextMeshProUGUI AmmoText;
    [SerializeField] private TextMeshProUGUI ClipText;
    [SerializeField] private TextMeshProUGUI FireTypeText;
    public Gradient AmmoTextColorGradient;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.onUIMaskChanged += OnUIMaskChanged;
        bl_EventHandler.onChangeWeapon += OnLocalChangeWeapon;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.onUIMaskChanged -= OnUIMaskChanged;
        bl_EventHandler.onChangeWeapon -= OnLocalChangeWeapon;
    }

    /// <summary>
    /// 
    /// </summary>
    public override void SetAmmoOf(bl_Gun gun)
    {
        int bullets = gun.bulletsLeft;
        int clips = gun.RemainingClips;
        float per = (float)bullets / (float)gun.bulletsPerClip;
        Color c = AmmoTextColorGradient.Evaluate(per);

        if (gun.useAmmo)
        {
            AmmoText.text = bullets.ToString();
            ClipText.text = gun.HaveInfinityAmmo ? "∞" : (ClipText.text = clips.ToString("F0"));
            AmmoText.color = c;
            ClipText.color = c;
        }
        else
        {
            AmmoText.text = "--";
            ClipText.text = ClipText.text = "--";
            AmmoText.color = Color.white;
            ClipText.color = Color.white;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weapon"></param>
    public override void SetFireType(bl_WeaponBase.FireType fireType)
    {
        if (FireTypeText == null) return;

        string fireName = "--";
        switch (fireType)
        {
            case bl_WeaponBase.FireType.Auto: fireName = bl_GameTexts.FireTypeAuto.Localized(45); break;
            case bl_WeaponBase.FireType.Semi: fireName = bl_GameTexts.FireTypeSemi.Localized(47); break;
            case bl_WeaponBase.FireType.Single: fireName = bl_GameTexts.FireTypeSingle.Localized(46); break;
        }
        FireTypeText.text = fireName;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="layers"></param>
    void OnUIMaskChanged(RoomUILayers layers)
    {
        content.SetActive(layers.IsEnumFlagPresent(RoomUILayers.WeaponData));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gunID"></param>
    void OnLocalChangeWeapon(int gunID)
    {
        if (gunID == -1) content.SetActive(false);
        else
        {
            content.SetActive(true);
        }
    }
}
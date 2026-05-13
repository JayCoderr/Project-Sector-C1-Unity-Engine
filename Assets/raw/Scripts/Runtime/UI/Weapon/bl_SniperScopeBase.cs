using UnityEngine;
using UnityEngine.Serialization;

public abstract class bl_SniperScopeBase : bl_MonoBehaviour
{
    /// <summary>
    /// If false, the scope will not be shown, this allow use different scopes in the same weapon.
    /// </summary>
    [LovattoToogle] public bool IsScopeEnabled = true;
    [FormerlySerializedAs("Scope"), SerializeField]
    private Sprite scopeTexture;
    public Sprite ScopeTexture
    {
        get => scopeTexture;
        set => scopeTexture = value;
    }
}
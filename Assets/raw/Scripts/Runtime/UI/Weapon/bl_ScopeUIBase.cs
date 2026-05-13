using System;
using UnityEngine;

public abstract class bl_ScopeUIBase : MonoBehaviour
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="active"></param>
    public virtual void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="weapon"></param>
    public abstract void SetupWeapon(bl_SniperScopeBase weapon);

    /// <summary>
    /// Show/Hide the scope UI with a fade transition
    /// </summary>
    /// <param name="fadeIn"></param>
    /// <param name="speed"></param>
    /// <param name="delay"></param>
    /// <param name="onFinish"></param>
    /// <param name="onStart"></param>
    public abstract void Crossfade(bool fadeIn, float speed, float delay = 0, Action onFinish = null, Action onStart = null);

    /// <summary>
    /// Show a text indication
    /// </summary>
    /// <param name="text"></param>
    public abstract void ShowIndication(string text);

    /// <summary>
    /// 
    /// </summary>
    private static bl_ScopeUIBase _instance;
    public static bl_ScopeUIBase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<bl_ScopeUIBase>();
            }
            return _instance;
        }
    }
}
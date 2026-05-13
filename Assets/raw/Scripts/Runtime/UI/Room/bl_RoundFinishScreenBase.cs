using System;
using UnityEngine;

public abstract class bl_RoundFinishScreenBase : MonoBehaviour
{
    private int _countdown = 0;
    private Action _countCallback;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="winner"></param>
    public abstract void Show(bl_GameModeBase.MatchOverInformation matchOverInformation);

    /// <summary>
    /// 
    /// </summary>
    public abstract void Hide();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="count"></param>
    public abstract void SetCountdown(int count);

    /// <summary>
    /// Start the countdown timer
    /// </summary>
    /// <param name="count"></param>
    public void StartCountdown(int count, Action callback = null)
    {
        _countdown = count;
        _countCallback = callback;
        InvokeRepeating(nameof(Countdown), 1, 1);
    }

    private void Countdown()
    {
        _countdown--;
        if (_countdown <= 0)
        {
            _countCallback?.Invoke();
            _countCallback = null;
            CancelInvoke(nameof(Countdown));
        }
    }

    private static bl_RoundFinishScreenBase _instance;
    public static bl_RoundFinishScreenBase Instance
    {
        get
        {
            if (_instance == null) _instance = FindAnyObjectByType<bl_RoundFinishScreenBase>();
            return _instance;
        }
    }
}
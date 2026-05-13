using System.Collections.Generic;

public class bl_MatchEventWatcher : bl_MonoBehaviour
{
    private List<string> hitEnemies = new();
    private Dictionary<string, List<string>> botsHits = new();

    /// <summary>
    /// 
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        bl_EventHandler.onLocalKill += OnLocalKill;
        bl_EventHandler.onLocalPlayerDeath += OnLocalDeath;
        bl_EventHandler.onLocalPlayerHitEnemy += OnLocalHitEnemy;
        bl_EventHandler.onRemotePlayerDeath += OnRemoteDeath;
        bl_EventHandler.Bots.onBotHitPlayer += OnBotHitPlayer;
        bl_EventHandler.onPlayerDeath += OnPlayerDeath;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnDisable()
    {
        base.OnDisable();
        bl_EventHandler.onLocalKill -= OnLocalKill;
        bl_EventHandler.onLocalPlayerDeath -= OnLocalDeath;
        bl_EventHandler.onLocalPlayerHitEnemy -= OnLocalHitEnemy;
        bl_EventHandler.onRemotePlayerDeath -= OnRemoteDeath;
        bl_EventHandler.Bots.onBotHitPlayer -= OnBotHitPlayer;
        bl_EventHandler.onPlayerDeath -= OnPlayerDeath;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="info"></param>
    void OnLocalKill(KillInfo info)
    {

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hitData"></param>
    void OnLocalHitEnemy(MFPSHitData hitData)
    {
        if (GetGameMode.GetGameModeInfo().AllowKillAssist && !hitEnemies.Contains(hitData.HitName) && hitData.HitName != bl_PhotonNetwork.NickName)
        {
            hitEnemies.Add(hitData.HitName);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hitData"></param>
    void OnBotHitPlayer(MFPSHitData hitData)
    {
        if (bl_PhotonNetwork.IsMasterClient)
        {
            // handle the assistences registration of the bots
            if (GetGameMode.GetGameModeInfo().AllowKillAssist)
            {
                if (!string.IsNullOrEmpty(hitData.PlayerAutorName))
                {
                    if (!botsHits.ContainsKey(hitData.PlayerAutorName))
                    {
                        botsHits.Add(hitData.PlayerAutorName, new List<string>());
                    }

                    if (!botsHits[hitData.PlayerAutorName].Contains(hitData.HitName) && !hitData.SelfHit())
                    {
                        botsHits[hitData.PlayerAutorName].Add(hitData.HitName);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalDeath()
    {
        // hitEnemies = new List<string>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="remotePlayer"></param>
    void OnRemoteDeath(bl_EventHandler.PlayerDeathData data)
    {
        if (hitEnemies.Contains(data.Player.Name))
        {
            if (data.KillerName != bl_PhotonNetwork.NickName)
            {
                bl_EventHandler.DispatchLocalKillAssist(new bl_EventHandler.KillAssistData() { KilledPlayer = data.Player.Name });
                bl_PhotonNetwork.LocalPlayer.PostAssist(1);
                bl_PhotonNetwork.LocalPlayer.PostScore(bl_GameData.ScoreSettings.ScorePerKillAssist);
            }
            hitEnemies.Remove(data.Player.Name);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void OnPlayerDeath(bl_EventHandler.PlayerDeathData data)
    {
        CheckBotAssist(data.Player.Name, data.KillerName);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="deathPlayer"></param>
    private void CheckBotAssist(string deathPlayer, string killer)
    {
        if (!bl_PhotonNetwork.IsMasterClient) return;

        // handle the assistences points for the bots
        foreach (var bot in botsHits)
        {
            if (bot.Key == killer) continue;
            if (bot.Value.Contains(deathPlayer))
            {
                bl_AIMananger.SetBotAssist(bot.Key);
                botsHits[bot.Key].Remove(deathPlayer);
            }
        }
    }
}
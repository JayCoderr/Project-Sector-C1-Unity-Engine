using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public static class bl_Extensions
{
    /// <summary>
    /// Get the score of the given player
    /// </summary>
    public static int GetPlayerScore(this Player p)
    {
        return GetIntProp(p, PropertiesKeys.ScoreKey);
    }

    /// <summary>
    /// Get player kills
    /// </summary>
    public static int GetKills(this Player p)
    {
        return GetIntProp(p, PropertiesKeys.KillsKey);
    }

    /// <summary>
    /// Get player deaths
    /// </summary>
    public static int GetDeaths(this Player p)
    {
        return GetIntProp(p, PropertiesKeys.DeathsKey);
    }

    /// <summary>
    /// Get player assists
    /// </summary>
    public static int GetAssists(this Player p)
    {
        return GetIntProp(p, PropertiesKeys.AssistsKey);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="p"></param>
    /// <param name="propKey"></param>
    /// <returns></returns>
    public static int GetIntProp(this Player p, string propKey)
    {
        return p != null && p.CustomProperties.ContainsKey(propKey) ? (int)p.CustomProperties[propKey] : 0;
    }

    /// <summary>
    /// Try get a custom property from the player
    /// </summary>
    /// <returns></returns>
    public static bool TryGetProp(this Player p, string propKey, out object value)
    {
        if (p == null) { value = null; return false; }

        return p.CustomProperties.TryGetValue(propKey, out value);
    }

    /// <summary>
    /// Post score to the given player and sync over network
    /// </summary>
    public static void PostScore(this Player p, int ScoreToAdd = 0)
    {
        ScoreToAdd = bl_Hook.ApplyFilters("Filter_LocalScoreAdd", ScoreToAdd);
        PostIntProp(p, ScoreToAdd, PropertiesKeys.ScoreKey);

        if (ScoreToAdd > 0 && p.IsLocal) bl_EventHandler.Player.onAddedScore?.Invoke(ScoreToAdd);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void PostKill(this Player p, int kills)
    {
        PostIntProp(p, kills, PropertiesKeys.KillsKey);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void PostDeaths(this Player p, int deaths)
    {
        PostIntProp(p, deaths, PropertiesKeys.DeathsKey);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void PostAssist(this Player p, int assist)
    {
        PostIntProp(p, assist, PropertiesKeys.AssistsKey);
    }

    /// <summary>
    /// 
    /// </summary>
    public static void PostIntProp(this Player p, int valueToAdd, string propKey)
    {
        if (p == null) return;

        int current = GetIntProp(p, propKey);
        current += valueToAdd;

        var table = new Hashtable()
        {
            {propKey, current},
        };

        p.SetCustomProperties(table);  // this locally sets the score and will sync it in-game asap.
    }

    /// <summary>
    /// Get the score of the given team
    /// </summary>
    /// <returns></returns>
    public static int GetRoomScore(this Room room, Team team)
    {
        object teamId;
        if (team == Team.Team1)
        {
            if (room.CustomProperties.TryGetValue(PropertiesKeys.Team1Score, out teamId))
            {
                return (int)teamId;
            }
        }
        else if (team == Team.Team2)
        {
            if (room.CustomProperties.TryGetValue(PropertiesKeys.Team2Score, out teamId))
            {
                return (int)teamId;
            }
        }

        return 0;
    }

    /// <summary>
    /// Add the given score to the given team and sync over network
    /// </summary>
    public static void SetTeamScore(this Room r, Team t, int scoreToAdd = 1)
    {
        if (t == Team.None) return;
        int score = r.GetRoomScore(t);
        score += scoreToAdd;
        string key = (t == Team.Team1) ? PropertiesKeys.Team1Score : PropertiesKeys.Team2Score;
        var h = new Hashtable
        {
            { key, score }
        };
        r.SetCustomProperties(h);
    }

    /// <summary>
    /// Get the team in which the given player is affiliated to
    /// </summary>
    /// <returns></returns>
    public static Team GetPlayerTeam(this Player p)
    {
        return p == null ? Team.None : p.CustomProperties.TryGetValue(PropertiesKeys.TeamKey, out object teamId) ? (Team)teamId : Team.None;
    }

    /// <summary>
    /// Sync the player team in which the given player is affiliated
    /// </summary>
    public static void SetPlayerTeam(this Player player, Team team)
    {
        var PlayerTeam = new Hashtable
        {
            { PropertiesKeys.TeamKey, team }
        };
        player.SetCustomProperties(PlayerTeam);
    }

    /// <summary>
    /// Get the team name by their identifier
    /// </summary>
    /// <returns></returns>
    public static string GetTeamName(this Team t)
    {
        switch (t)
        {
            case Team.Team1:
            case Team.Team2:
                return MFPSTeam.Get(t).Name;
            default:
                return "Solo";
        }
    }

    /// <summary>
    /// Get the team color by their identifier
    /// </summary>
    /// <returns></returns>
    public static Color GetTeamColor(this Team t, float alpha = 0)
    {
        Color c = Color.white;//default color
        switch (t)
        {
            case Team.Team1:
            case Team.Team2:
                c = MFPSTeam.Get(t).TeamColor;
                break;
            case Team.All:
                c = Color.white;
                break;
        }
        if (alpha > 0) { c.a = alpha; }

        return c;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static GameModeSettings GetModeInfo(this GameMode mode)
    {
        for (int i = 0; i < bl_GameData.Instance.gameModes.Count; i++)
        {
            if (bl_GameData.Instance.gameModes[i].gameMode == mode) { return bl_GameData.Instance.gameModes[i]; }
        }
        return null;
    }

    /// <summary>
    /// Save (locally) the given player class as the default
    /// </summary>
    private const string PLAYER_CLASS_KEY = "{0}.playerclass";
    public static void SavePlayerClass(this PlayerClass pc)
    {
        string key = string.Format(PLAYER_CLASS_KEY, Application.productName);
        PlayerPrefs.SetInt(key, (int)pc);
    }

    /// <summary>
    /// Get the locally saved player class
    /// </summary>
    /// <returns></returns>
    public static PlayerClass GetSavePlayerClass(this PlayerClass pc)
    {
        string key = string.Format(PLAYER_CLASS_KEY, Application.productName);
        int id = PlayerPrefs.GetInt(key, 0);
        PlayerClass pclass = (PlayerClass)id;

        return pclass;
    }

    /// <summary>
    /// Get the player name along with their user role (if there's any)
    /// </summary>
    /// <returns></returns>
    public static string NickNameAndRole(this Player p)
    {
        return p.CustomProperties.TryGetValue(PropertiesKeys.UserRole, out object role)
            ? string.Format("<b>{1}</b> {0}", p.NickName, (string)role)
            : p.NickName;
    }

    /// <summary>
    /// Get the complete game mode name by their identifier
    /// </summary>
    /// <returns></returns>
    public static string GetName(this GameMode mode)
    {
        GameModeSettings info = mode.GetModeInfo();
        return info != null ? info.ModeName : string.Format("Not define: " + mode.ToString());
    }

    /// <summary>
    /// Get the game mode info by their identifier
    /// </summary>
    /// <returns></returns>
    public static GameModeSettings GetGameModeInfo(this GameMode mode)
    {
        return bl_GameData.GetGameModeInfo(mode);
    }

    /// <summary>
    /// is this player in the same team that local player?
    /// </summary>
    /// <returns></returns>
    public static bool IsTeamMate(this Player p)
    {
        bool b = false;
        if (p.GetPlayerTeam() == bl_PhotonNetwork.LocalPlayer.GetPlayerTeam()) { b = true; }
        return b;
    }

    /// <summary>
    /// Get the player list of an specific team
    /// </summary>
    /// <returns></returns>
    public static Player[] GetPlayersInTeam(this Player[] player, Team team)
    {
        var list = new List<Player>();
        for (int i = 0; i < player.Length; i++)
        {
            if (player[i].GetPlayerTeam() == team) { list.Add(player[i]); }
        }
        return list.ToArray();
    }

    /// <summary>
    /// Get the player with the highest score from the given player list.
    /// </summary>
    /// <param name="playerList">The array of players to search.</param>
    /// <returns>The player with the highest score.</returns>
    public static Player GetPlayerWithHighestScore(Player[] playerList)
    {
        if (playerList == null) return null;
        if (playerList.Length <= 1) return playerList[0];

        int high = 0, index = 0;
        for (int i = 0; i < playerList.Length; i++)
        {
            int h = playerList[i].GetPlayerScore();
            if (h > high)
            {
                high = h;
                index = i;
            }
        }

        return playerList[index];
    }

    /// <summary>
    /// Get current gamemode
    /// </summary>
    public static GameMode GetGameMode(this RoomInfo room)
    {
        if (room == null) { return GameMode.FFA; }

        return (GameMode)room.CustomProperties[PropertiesKeys.GameModeKey];
    }

    /// <summary>
    /// Get the current room info parsed in the custom MFPS class
    /// </summary>
    /// <returns></returns>
    public static MFPSRoomInfo GetRoomInfo(this Room room)
    {
        return new MFPSRoomInfo(room);
    }

    /// <summary>
    /// Check if the given team is the local player enemy team
    /// </summary>
    /// <returns></returns>
    public static Team OppsositeTeam(this Team team)
    {
        if (team == Team.Team1) { return Team.Team2; }
        else if (team == Team.Team2) { return Team.Team1; }
        else
        {
            return Team.All;
        }
    }

    /// <summary>
    /// Better solution for invoke methods after a certain time
    /// </summary>
    public static void InvokeAfter(this MonoBehaviour mono, float time, Action callback)
    {
        mono.StartCoroutine(WaitToExecute(time, callback));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="time"></param>
    /// <param name="callback"></param>
    /// <returns></returns>
    static IEnumerator WaitToExecute(float time, Action callback)
    {
        yield return new WaitForSeconds(time);
        callback?.Invoke();
    }

    /// <summary>
    /// Re-size a rawImage to fit with the parent RectTransform size
    /// </summary>
    /// <returns></returns>
    public static Vector2 SizeToParent(this RawImage image, float padding = 0)
    {
        float w = 0, h = 0;
        var parent = image.transform.parent.GetComponent<RectTransform>();
        var imageTransform = image.GetComponent<RectTransform>();

        // check if there is something to do
        if (image.texture != null)
        {
            if (!parent) { return imageTransform.sizeDelta; } //if we don't have a parent, just return our current width;
            padding = 1 - padding;
            float ratio = image.texture.width / (float)image.texture.height;
            var bounds = new Rect(0, 0, parent.rect.width, parent.rect.height);
            if (Mathf.RoundToInt(imageTransform.eulerAngles.z) % 180 == 90)
            {
                //Invert the bounds if the image is rotated
                bounds.size = new Vector2(bounds.height, bounds.width);
            }
            //Size by height first
            h = bounds.height * padding;
            w = h * ratio;
            if (w > bounds.width * padding)
            { //If it doesn't fit, fallback to width;
                w = bounds.width * padding;
                h = w / ratio;
            }
        }
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, w);
        imageTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, h);
        return imageTransform.sizeDelta;
    }

    /// <summary>
    /// Randomize some Audio Source properties to get a different audio output result
    /// </summary>
    public static void RandomizeAudioOutput(this AudioSource source)
    {
        source.pitch = UnityEngine.Random.Range(0.92f, 1.1f);
        source.spread = UnityEngine.Random.Range(0.98f, 1.25f);
    }

    /// <summary>
    /// Check if the collider is from the local player
    /// </summary>
    /// <returns></returns>
    public static bool isLocalPlayerCollider(this Collider collider)
    {
        return collider.CompareTag(bl_MFPS.LOCAL_PLAYER_TAG);
    }

    /// <summary>
    /// Is the collider part of the MFPS player? (Local, AI, Remote)
    /// </summary>
    /// <param name="collider"></param>
    /// <returns></returns>
    public static bool IsMFPSPlayer(this Collider collider)
    {
        return collider.CompareTag(bl_MFPS.HITBOX_TAG) || collider.CompareTag(bl_MFPS.LOCAL_PLAYER_TAG) || collider.CompareTag(bl_MFPS.AI_TAG) || collider.CompareTag(bl_MFPS.REMOTE_PLAYER_TAG);
    }

    /// <summary>
    /// Localization addon helper
    /// Get the localized text of the given key
    /// </summary>
    /// <returns></returns>
    public static string Localized(this string str, string key, bool plural = false, string defaultText = "")
    {
#if LOCALIZATION
        return plural ? bl_Localization.Instance.GetTextPlural(key, defaultText) : bl_Localization.Instance.GetText(key, defaultText);
#else
        return str;
#endif
    }

    /// <summary>
    /// Localization addon helper
    /// Get the localized text of the given key
    /// </summary>
    /// <returns></returns>
    public static string Localized(this string str, int id, bool plural = false)
    {
#if LOCALIZATION
        return plural ? bl_Localization.Instance.GetTextPlural(id) : bl_Localization.Instance.GetText(id);
#else
        return str;
#endif
    }

    /// <summary>
    /// Get an int array as a string array separated by the give char
    /// </summary>
    /// <returns></returns>
    public static string[] AsStringArray(this int[] array, string endoint = "") => array.Select(x => (x.ToString() + endoint)).ToArray();

    /// <summary>
    /// Check if a flag is selected in the give flag enum property
    /// </summary>
    /// <returns></returns>
    public static bool IsEnumFlagPresent<T>(this T value, T lookingForFlag) where T : Enum
    {
        int intValue = (int)(object)value;
        int intLookingForFlag = (int)(object)lookingForFlag;
        return ((intValue & intLookingForFlag) == intLookingForFlag);
    }

    /// <summary>
    /// Check if a flag is selected in the give flag enum property
    /// </summary>
    /// <returns></returns>
    public static bool IsByteEnumFlagPresent<T>(this T value, T lookingForFlag) where T : Enum
    {
        int intValue = (byte)(object)value;
        int intLookingForFlag = (byte)(object)lookingForFlag;
        return ((intValue & intLookingForFlag) == intLookingForFlag);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    public static MFPSPlayer[] ToMFPSPlayerList(this Player[] list)
    {
        var mfpsList = new MFPSPlayer[list.Length];
        for (int i = 0; i < list.Length; i++)
        {
            mfpsList[i] = bl_GameManager.Instance.GetMFPSPlayer(list[i].NickName);
        }
        return mfpsList;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="playerClass"></param>
    /// <returns></returns>
    public static string DisplayName(this PlayerClass playerClass)
    {
        return bl_GameData.GetPlayerClassData(playerClass).DisplayName;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="playerClass"></param>
    /// <returns></returns>
    public static Sprite Icon(this PlayerClass playerClass)
    {
        return bl_GameData.GetPlayerClassData(playerClass).ClassIcon;
    }

    /// <summary>
    /// Is this game mode of one team mode (one vs all) or a multiple team mode?
    /// </summary>
    /// <param name="mode"></param>
    /// <returns></returns>
    public static bool IsOneTeamMode(this GameMode mode)
    {
        return !bl_GameData.IsTeamMode(mode);
    }

    /// <summary>
    /// Creates a new parent GameObject for the given Transform.
    /// </summary>
    /// <param name="t">The Transform to create a parent for.</param>
    /// <param name="name">The name of the new parent GameObject.</param>
    /// <param name="moveToNewParent">Whether to move the Transform to the new parent.</param>
    /// <param name="keepPosition">Whether to keep the Transform's position and rotation when moving to the new parent.</param>
    /// <returns>The Transform of the new parent GameObject.</returns>
    public static Transform CreateParent(this Transform t, string name, bool moveToNewParent = true, bool keepPosition = true)
    {
        GameObject g = new(name);
        g.transform.SetParent(t.parent);
        if (keepPosition)
        {
            g.transform.localPosition = t.localPosition;
            g.transform.localEulerAngles = t.localEulerAngles;
        }
        else
        {
            g.transform.localPosition = Vector3.zero;
            g.transform.localEulerAngles = Vector3.zero;
        }
        g.transform.localScale = Vector3.one;
        if (moveToNewParent)
        {
            t.SetParent(g.transform);
            if (keepPosition)
            {
                t.localPosition = Vector3.zero;
                t.localEulerAngles = Vector3.zero;
            }
        }
        return g.transform;
    }

    /// <summary>
    /// Assign a mixer group to an audio source
    /// </summary>
    /// <param name="source"></param>
    /// <param name="mixerName"></param>
    public static void AssignMixerGroup(this AudioSource source, string mixerName)
    {
        if (bl_GlobalReferences.I.MFPSAudioMixer == null) return;

        var mixers = bl_GlobalReferences.I.MFPSAudioMixer.FindMatchingGroups(mixerName);
        if (mixers == null || mixers.Length == 0)
        {
            // log warning
            Debug.LogWarning($"The mixer '{mixerName}' was not found in the Audio Mixer");
            return;
        }

        source.outputAudioMixerGroup = mixers[0];
    }

    /// <summary>
    /// Get the GunInfo of the given gun id
    /// </summary>
    /// <param name="gunId"></param>
    /// <returns></returns>
    public static bl_GunInfo GetGunInfo(this int gunId)
    {
        return bl_GameData.Instance.GetWeapon(gunId);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gunId"></param>
    /// <returns></returns>
    public static string GetWeaponName(this int gunId)
    {
        var info = GetGunInfo(gunId);
        if (info == null) return "Gun ID: " + gunId;
        return info.Name;
    }

    /// <summary>
    /// Is the value withing the Vector2 range? (X = min, Y = max)
    /// </summary>
    /// <param name="value"></param>
    /// <param name="range"></param>
    /// <returns></returns>
    public static bool IsWithinRange(this Vector2 range, float value)
    {
        return value >= range.x && value <= range.y;
    }

    /// <summary>
    /// Is the animator playing or in transition
    /// </summary>
    /// <param name="animator"></param>
    /// <returns></returns>
    public static bool IsPlaying(this Animator animator)
    {
        return !(animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0));
    }

    /// <summary>
    /// Invert the given color
    /// </summary>
    /// <param name="color"></param>
    /// <returns></returns>
    public static Color InvertColor(this Color color)
    {
        return new Color(1 - color.r, 1 - color.g, 1 - color.b, color.a);
    }
}
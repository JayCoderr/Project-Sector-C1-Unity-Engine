using System.IO;
using UnityEngine;

/// <summary>
/// MFPS class with all the Damage information
/// </summary>
public class DamageData
{
    /// <summary>
    /// The amount of damage to apply to the IDamageable object
    /// </summary>
    public int Damage = 10;

    /// <summary>
    /// The cached name of the actor of this damage
    /// Since the damage can't always comes from a <see cref="MFPSActor"/> (player or bot)
    /// This the alternative way to get the name of the actor.
    /// </summary>
    public string From;

    /// <summary>
    /// Cause of the damage
    /// </summary>
    public DamageCause Cause = DamageCause.Player;

    /// <summary>
    /// The position from where this damage origins (bullet origin, explosion origin, etc...)
    /// </summary>
    public Vector3 OriginPosition
    {
        get;
        set;
    } = Vector3.zero;

    /// <summary>
    /// The point of the hit collision
    /// </summary>
    public Vector3 HitPoint
    {
        get;
        set;
    }

    /// <summary>
    /// In case of a bullet, the direction of the bullet
    /// </summary>
    public Vector3 TravelDirection
    {
        get;
        set;
    }

    /// <summary>
    /// Is this damage comes from a head shot?
    /// </summary>
    public bool IsHeadShot
    {
        get;
        set;
    } = false;

    /// <summary>
    /// The GunID of the weapon that cause this damage in case it was origin from a weapon
    /// if the damage was not from a weapon you can skip it.
    /// </summary>
    public int GunID
    {
        get;
        private set; // to set the GunID use the SetGunID method
    } = 0;

    /// <summary>
    /// The MFPS Actor of this damage
    /// Can be a real player or bot
    /// </summary>
    public MFPSPlayer MFPSActor
    {
        get;
        set;
    }

    /// <summary>
    /// The cached network view id of the actor of this damage
    /// </summary>
    public int ActorViewID
    {
        get;
        set;
    }

    /// <summary>
    /// 
    /// </summary>
    public string SpecialWeaponName
    {
        get;
        set;
    }

    /// <summary>
    /// Create a hashtable with the this damage data
    /// </summary>
    /// <returns></returns>
    public ExitGames.Client.Photon.Hashtable GetAsHashtable()
    {
        var data = bl_UtilityHelper.CreatePhotonHashTable();
        data.Add((byte)0, Damage);
        data.Add((byte)1, GunID);
        data.Add((byte)2, ActorViewID);
        data.Add((byte)3, Cause);
        data.Add((byte)4, From);
        if (OriginPosition != Vector3.zero)
            data.Add((byte)5, OriginPosition);

        if (MFPSActor != null)
        {
            data.Add("ma", MFPSActor.Name);
        }

        return data;
    }

    /// <summary>
    /// 
    /// </summary>
    public DamageData(ExitGames.Client.Photon.Hashtable data)
    {
        Damage = (int)data[(byte)0];
        GunID = (int)data[(byte)1];
        ActorViewID = (int)data[(byte)2];
        Cause = (DamageCause)data[(byte)3];
        From = (string)data[(byte)4];
        if (data.ContainsKey((byte)5)) OriginPosition = (Vector3)data[(byte)5];

        if (data.ContainsKey("ma"))
        {
            string actorName = (string)data["ma"];
            MFPSActor = bl_GameManager.Instance.GetMFPSPlayer(actorName);
        }

        if (MFPSActor == null) MFPSActor = bl_GameManager.Instance.GetMFPSActor(ActorViewID);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gunId"></param>
    public void SetGunID(int gunId)
    {
        GunID = gunId;
        Cause |= DamageCause.Weapon;
    }

    /// <summary>
    /// Set the GunID as a special weapon
    /// </summary>
    /// <param name="id"></param>
    public void SetSpecialWeaponID(int id)
    {
        // special weapons starts from 300
        GunID = 300 + id;
    }

    /// <summary>
    /// Set the local player as the actor of this damage
    /// </summary>
    public void SetLocalAsActor()
    {
        MFPSActor = bl_MFPS.LocalPlayer.MFPSActor;
        ActorViewID = bl_MFPS.LocalPlayer.ViewID;
    }

    /// <summary>
    /// Get the distance between the origin position and the hit point
    /// </summary>
    /// <returns></returns>
    public float GetDamageDistance()
    {
        return Vector3.Distance(OriginPosition, HitPoint);
    }

    /// <summary>
    /// Is this damage cause by the local player?
    /// </summary>
    /// <returns></returns>
    public bool FromLocalPlayer()
    {
        if (ActorViewID != -1)
        {
            return bl_MFPS.Utility.IsLocalPlayer(ActorViewID);
        }

        if (MFPSActor != null)
        {
            return MFPSActor.isRealPlayer && bl_MFPS.Utility.IsLocalPlayer(MFPSActor.ActorViewID);
        }

        if (!string.IsNullOrEmpty(From))
        {
            return From == bl_PhotonNetwork.LocalPlayer.NickName;
        }

        return false;
    }

    /// <summary>
    /// Is this damage cause by a player or bot?
    /// </summary>
    /// <returns></returns>
    public bool CauseByAPlayer()
    {
        return Cause.IsEnumFlagPresent(DamageCause.Player) || Cause.IsEnumFlagPresent(DamageCause.Bot);
    }

    /// <summary>
    /// Is this damage cause by a weapon?
    /// </summary>
    /// <returns></returns>
    public bool CauseByAWeapon()
    {
        return Cause.IsEnumFlagPresent(DamageCause.Weapon);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool CauseBySpecialWeapon(out int specialId)
    {
        specialId = GunID - 300;
        return GunID >= 300;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public bool IsActorARealPlayer()
    {
        if (MFPSActor != null)
        {
            return MFPSActor.isRealPlayer;
        }

        return false;
    }

    /// <summary>
    /// Get the direction of the damage (origin position to hit point)
    /// </summary>
    /// <returns></returns>
    public Vector3 GetRHSDirection()
    {
        return HitPoint - OriginPosition;
    }

    /// <summary>
    /// Get the team of the actor of this damage
    /// </summary>
    /// <returns></returns>
    public Team GetActorTeam()
    {
        return MFPSActor != null ? MFPSActor.Team : Team.None;
    }

    /// <summary>
    /// 
    /// </summary>
    public DamageData() { }

    public static byte[] SerializeDamageData(object customObject)
    {
        DamageData data = (DamageData)customObject;
        // Use a MemoryStream with a BinaryWriter to serialize data
        // @TODO: Compress data before sending it
        using MemoryStream ms = new();
        using (BinaryWriter writer = new(ms))
        {
            writer.Write((short)data.Damage);
            writer.Write(string.IsNullOrEmpty(data.From) ? "" : data.From); // still needed?
            writer.Write((byte)data.Cause);
            writer.WriteVector3(data.OriginPosition);
            writer.WriteVector3(data.HitPoint);
            writer.Write(data.IsHeadShot);
            writer.Write((byte)data.GunID);
            writer.Write((short)data.ActorViewID);
            writer.Write(data.MFPSActor != null);
            if (data.GunID >= 300)
            {
                if (string.IsNullOrEmpty(data.SpecialWeaponName))
                {
                    data.SpecialWeaponName = "";
                }
                writer.Write(data.SpecialWeaponName);
            }

            // current byte size: 2 + 32 + 1 + 12 + 12 + 1 + 4 + 1 + 1 + 4 + 2 + 1 + 1 = 65
            // 65 bytes is the current size of the serialized data, we trade some CPU for less bandwidth by casting some types to smaller ones
            // ideally those types should be using those smaller types by default.

            return ms.ToArray();
        }
    }

    public static object DeserializeDamageData(byte[] data)
    {
        DamageData result = new();
        using (MemoryStream ms = new(data))
        {
            using (BinaryReader reader = new(ms))
            {
                result.Damage = reader.ReadInt16();
                result.From = reader.ReadString();
                result.Cause = (DamageCause)reader.ReadByte();
                result.OriginPosition = reader.ReadVector3();
                result.HitPoint = reader.ReadVector3();
                result.IsHeadShot = reader.ReadBoolean();
                result.GunID = reader.ReadByte();
                result.ActorViewID = reader.ReadInt16();
                bool hasActor = reader.ReadBoolean();
                if (hasActor)
                {
                    result.MFPSActor = bl_GameManager.Instance.GetMFPSActor(result.ActorViewID);
                }
                if (result.GunID >= 300)
                {
                    result.SpecialWeaponName = reader.ReadString();
                }
            }
        }
        return result;
    }
}

/// <summary>
/// 
/// </summary>
public struct MFPSHitData
{
    /// <summary>
    /// The name of the player who cause the damage
    /// </summary>
    public string PlayerAutorName;

    /// <summary>
    /// The name of the object who was hit
    /// </summary>
    public string HitName;

    /// <summary>
    /// The amount of damage applied to the hit object
    /// </summary>
    public int Damage;

    /// <summary>
    /// The point of the hit collision
    /// </summary>
    public Vector3 HitPosition;

    /// <summary>
    /// The origin position of the damage
    /// </summary>
    public Vector3 OriginPosition;

    /// <summary>
    /// The transform reference of the hit object
    /// A null check is required before access this since is can be destroyed
    /// </summary>
    public Transform HitTransform;

    /// <summary>
    /// Was this hit lethal? (kill the hit object)
    /// </summary>
    public bool WasLethal;

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public readonly bool SelfHit() => PlayerAutorName == HitName;

    /// <summary>
    /// The distance between the origin position and the hit position
    /// </summary>
    /// <returns></returns>
    public float DamageDistance() => Vector3.Distance(OriginPosition, HitPosition);
}
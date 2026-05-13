using System.IO;
using UnityEngine;

/// <summary>
/// Contains all the information about an instanced bullet
/// </summary>
public class BulletData
{

    /// <summary>
    /// The name of the weapon from which this bullet was fired.
    /// </summary>
    public string WeaponName;

    /// <summary>
    /// The base damage that this bullet will cause
    /// </summary>
    public float Damage;

    /// <summary>
    /// The Position from where this bullet was fired.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// The amount of force to applied to the hit object in case this have a RigidBody
    /// </summary>
    public float ImpactForce;

    /// <summary>
    /// The bullet inaccuracy to apply to the projected direction
    /// </summary>
    public Vector3 Inaccuracity = Vector3.zero;

    /// <summary>
    /// Bullet Speed
    /// </summary>
    public float Speed;

    /// <summary>
    /// The max distance that this bullet can travel without hit anything.
    /// </summary>
    public float Range;

    /// <summary>
    /// The amount of bullet drop
    /// </summary>
    public float DropFactor;

    /// <summary>
    /// Maximum distance at which a weapon can be expected to be accurate and achieve the intended base damage.
    /// </summary>
    public float EffectiveFiringRange;

    /// <summary>
    /// Dtermine the drop off of the bullet damage over distance after the <see cref="EffectiveFiringRange"/> is reached.
    /// </summary>
    public AnimationCurve EffectivenessDropOff;

    /// <summary>
    /// Projectile decelerating due to air resistance (drag)
    /// control how quickly the bullet slows down, lower values (0-0.2) experiences less air resistance as it travels, resulting in slower deceleration
    /// The bullet maintains its speed longer and travels farther before losing significant velocity.
    /// </summary>
    public float DragCoefficient;

    /// <summary>
    /// GunID of the weapon from which this bullet was fire
    /// </summary>
    public int WeaponID;

    /// <summary>
    /// Was this bullet created by a remote player?
    /// </summary>
    public bool isNetwork;

    /// <summary>
    /// Was this bullet created by the actual local player
    /// The difference between this and <see cref="isNetwork"/>
    /// Is that IsNetwork can be False (meaning is not a remote bullet) when a Bot created the bullet
    /// But this ensure that the bullet was created by the real local player.
    /// </summary>
    public bool IsLocalPlayer;

    /// <summary>
    /// The Cached Network View
    /// </summary>
    public int ActorViewID
    {
        get;
        set;
    } = -1;

    /// <summary>
    /// The MFPS Actor who create this bullet
    /// </summary>
    public MFPSPlayer MFPSActor
    {
        get;
        set;
    }

    /// <summary>
    /// Penetration budget for this bullet
    /// By default doesn't affect anything, this property is to add support to a custom mod.
    /// </summary>
    public int PenetrationBudget;

    /// <summary>
    /// Create the bullet data and fetch info from the <see cref="DamageData"/>
    /// </summary>
    /// <param name="data"></param>
    public BulletData(DamageData data)
    {
        MFPSActor = data.MFPSActor;
        Damage = data.Damage;
        ActorViewID = data.ActorViewID;
        WeaponID = data.GunID;
        Position = data.OriginPosition;
    }

    /// <summary>
    /// Calculate and assign the projectile inaccuracy vector
    /// </summary>
    /// <param name="spreadBase"></param>
    /// <param name="maxSpread"></param>
    /// <returns></returns>
    public BulletData SetInaccuracity(float spreadBase, float maxSpread)
    {
        Inaccuracity = new Vector3(Random.Range(-maxSpread, maxSpread) * spreadBase, Random.Range(-maxSpread, maxSpread) * spreadBase, 1);
        return this;
    }

    /// <summary>
    /// 
    /// </summary>
    public BulletData()
    {
    }

    /// <summary>
    /// Get the bullet damage based on the distance
    /// </summary>
    /// <param name="distance">Set -1 to get the base damage</param>
    /// <returns></returns>
    public float GetDamage(float distance)
    {
        if (distance < 0 || distance <= EffectiveFiringRange || EffectivenessDropOff == null)
        {
            return Damage;
        }
        else
        {
            float effectiveness = EffectivenessDropOff.Evaluate(distance - EffectiveFiringRange);
            return Damage * effectiveness;
        }
    }

    /// <summary>
    /// Set the GunID as a special weapon
    /// </summary>
    /// <param name="id"></param>
    public void SetSpecialWeaponID(int id)
    {
        WeaponID = id + 300;
    }

    /// <summary>
    /// Setup the bullet data as a network bullet
    /// </summary>
    /// <returns></returns>
    public BulletData AsNetworkBullet()
    {
        isNetwork = true;
        Damage = 0;
        ImpactForce = 0;
        return this;
    }

    /// <summary>
    /// Setup the bullet data as a local created bullet.
    /// </summary>
    /// <returns></returns>
    public BulletData AsLocalBullet()
    {
        IsLocalPlayer = true;
        ActorViewID = bl_MFPS.LocalPlayer.ViewID;
        MFPSActor = bl_MFPS.LocalPlayer.MFPSActor;
        return this;
    }

    /// <summary>
    /// Associates the bullet with an actor identified by the specified view ID.
    /// </summary>
    /// <remarks>If <paramref name="viewID"/> is not -1, the method attempts to retrieve and assign the
    /// corresponding actor from the game manager. Otherwise, the bullet is disassociated from any actor.</remarks>
    /// <param name="viewID">The unique identifier of the actor to associate with this bullet. Specify -1 to disassociate the bullet from any
    /// actor.</param>
    /// <returns>The current <see cref="BulletData"/> instance with the updated actor association.</returns>
    public BulletData SetActor(int viewID)
    {
        ActorViewID = viewID;
        if (viewID != -1 && bl_GameManager.Instance != null)
        {
            MFPSActor = bl_GameManager.Instance.GetMFPSActor(viewID);
        }
        return this;
    }

    public static byte[] SerializeDamageData(object customObject)
    {
        BulletData data = (BulletData)customObject;
        // Use a MemoryStream with a BinaryWriter to serialize data
        using MemoryStream ms = new();
        using (BinaryWriter writer = new(ms))
        {
            writer.Write(data.Damage);
            writer.Write(data.Position.x);
            writer.Write(data.Position.y);
            writer.Write(data.Position.z);
            writer.Write(data.ImpactForce);
            writer.Write(data.Inaccuracity.x);
            writer.Write(data.Inaccuracity.y);
            writer.Write(data.Inaccuracity.z);
            writer.Write(data.Speed);
            writer.Write(data.Range);
            writer.Write(data.DropFactor);
            writer.Write(data.EffectiveFiringRange);
            writer.Write(data.WeaponID);
            writer.Write(data.IsLocalPlayer);
            writer.Write(data.ActorViewID);

            return ms.ToArray();
        }
    }

    public static object DeserializeDamageData(byte[] data)
    {
        BulletData result = new();
        using (MemoryStream ms = new(data))
        {
            using (BinaryReader reader = new(ms))
            {
                result.Damage = reader.ReadInt32();
                result.Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                result.ImpactForce = reader.ReadSingle();
                result.Inaccuracity = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                result.Speed = reader.ReadSingle();
                result.Range = reader.ReadSingle();
                result.DropFactor = reader.ReadSingle();
                result.EffectiveFiringRange = reader.ReadSingle();
                result.WeaponID = reader.ReadInt32();
                result.IsLocalPlayer = reader.ReadBoolean();
                result.ActorViewID = reader.ReadInt32();
                result.isNetwork = !bl_MFPS.Utility.IsLocalPlayer(result.ActorViewID);

                if (result.ActorViewID != -1)
                {
                    result.MFPSActor = bl_GameManager.Instance.GetMFPSActor(result.ActorViewID);
                }
            }
        }
        return result;
    }
}

/// <summary>
/// 
/// </summary>
public class CustomProjectileData
{
    public Vector3 Origin;
    public Quaternion Rotation;
}

public struct BulletInstanceData
{
    public Vector3 Position;
    public Quaternion Rotation;
    public Vector3 ProjectedHitPoint;
    public BulletInstanceFlags Flags;
    public bool CanShoot;
}
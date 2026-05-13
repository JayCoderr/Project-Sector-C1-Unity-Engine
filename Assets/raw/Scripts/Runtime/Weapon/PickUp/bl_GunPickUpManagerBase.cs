using UnityEngine;

public abstract class bl_GunPickUpManagerBase : bl_PhotonHelper
{
    /// <summary>
    /// 
    /// </summary>
    public struct ThrowData
    {
        public int GunID;
        public Vector3 Origin;
        public Vector3 Direction;
        public int[] Data;
        public bool AutoDestroy;
        public GunType WeaponType;

        public bool EmptyAmmo => Data == null || Data.Length == 0 || (Data[0] == 0 && Data[1] == 0);
    }

    /// <summary>
    /// 
    /// </summary>
    public struct PickUpData
    {
        public string Identifier;
        public int GunID;
        public bl_GunPickUpBase.AmmoInformation Ammunition;
    }

    /// <summary>
    /// Block the pick up of weapons
    /// </summary>
    public bool BlockPickUp { get; set; }

    public bl_GunPickUpBase LastTrigger { get; set; }

    /// <summary>
    /// Throw a weapon on all clients
    /// </summary>
    public abstract void ThrowGun(ThrowData throwData);

    /// <summary>
    /// Send a call to all the room clients to let them know that player will pick up a weapon
    /// </summary>
    public abstract void SendPickUp(PickUpData pickUpData);

    /// <summary>
    /// 
    /// </summary>
    private static bl_GunPickUpManagerBase _instance;
    public static bl_GunPickUpManagerBase Instance
    {
        get
        {
            if (_instance == null) { _instance = FindAnyObjectByType<bl_GunPickUpManagerBase>(); }
            return _instance;
        }
    }
}
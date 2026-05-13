using UnityEngine;

namespace MFPS.Runtime.Misc
{
    /// <summary>
    /// Use this script to call a event when this object is hit by a bullet.
    /// Simple attach this script to a object with an collider and assign the event in the inspector to call when hit.
    /// </summary>
    public class bl_WeaponButton : MonoBehaviour, IMFPSDamageable
    {
        [LovattoToogle] public bool fromLocalOnly = false;
        public bl_EventHandler.UEvent onHit;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="damageData"></param>
        public void ReceiveDamage(DamageData damageData)
        {
            if (fromLocalOnly && !damageData.FromLocalPlayer()) return;

            onHit?.Invoke();
        }
    }
}
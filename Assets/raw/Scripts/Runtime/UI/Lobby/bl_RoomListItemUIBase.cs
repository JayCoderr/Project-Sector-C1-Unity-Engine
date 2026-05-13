using Photon.Realtime;
using UnityEngine;

namespace MFPS.Runtime.UI
{
    public abstract class bl_RoomListItemUIBase : MonoBehaviour
    {
        public enum RoomFlag
        {
            None = 0,
            Persisten = 1,
        }

        public RoomFlag roomFlag;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="info"></param>
        public abstract void SetInfo(RoomInfo info);

        /// <summary>
        /// Set the info of the room (not from the server)
        /// </summary>
        /// <param name="info"></param>
        /// <param name="playerCount"></param>
        public virtual void SetMFPSInfo(MFPSRoomInfo info, int playerCount = 0) { }
    }
}
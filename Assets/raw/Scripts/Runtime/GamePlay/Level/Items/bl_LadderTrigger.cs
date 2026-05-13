using UnityEngine;

namespace MFPS.Runtime.Level
{
    public class bl_LadderTrigger : MonoBehaviour
    {
        [SerializeField] private bl_Ladder ladder = null;

        private void OnTriggerEnter(Collider other)
        {
            if (ladder == null) return;

            if (other.isLocalPlayerCollider())
            {
                var playerReferences = other.GetComponent<bl_PlayerReferences>();
                if (playerReferences != null)
                {
                    ladder.OnPlayerTrigger(playerReferences, true);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (ladder == null) return;

            if (other.isLocalPlayerCollider())
            {
                var playerReferences = other.GetComponent<bl_PlayerReferences>();
                if (playerReferences != null)
                {
                    ladder.OnPlayerTrigger(playerReferences, false);
                }
            }
        }
    }
}
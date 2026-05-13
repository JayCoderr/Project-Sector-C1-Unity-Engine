using UnityEngine;

namespace MFPS.Runtime.AI
{
    [CreateAssetMenu(fileName = "AI Behavior Settings", menuName = "MFPS/AI/Behavior Settings")]
    public class bl_AIBehaviorSettings : ScriptableObject
    {
        [Header("Settings")]
        public AIAgentBehave agentBehave = AIAgentBehave.Aggressive;
        public AIWeaponAccuracy weaponAccuracy = AIWeaponAccuracy.Casual;
        [LovattoToogle] public bool GetRandomTargetOnStart = true;
        [Tooltip("When the bot has receive damage and it's health is less than half, it will go all in to the target.")]
        [LovattoToogle] public bool forceFollowAtHalfHealth = true;
        [Tooltip("When the bot have a target it still check for enemy threats (closer than the target and on sight)")]
        [LovattoToogle] public bool checkEnemysWhenHaveATarget = true;
        [Tooltip("Detect nearby enemies out of the view scope of the bot?")]
        [LovattoToogle] public bool detectNearbyEnemies = true;
        [LovattoToogle] public bool breakHoldPositionOnDamage = true;
        public AITargetOutNavmeshBehave targetOutNavmeshBehave = AITargetOutNavmeshBehave.RandomPatrol;
        public AITargetOutRangeBehave targetOutRangeBehave = AITargetOutRangeBehave.KeepFollowingBasedOnState;
        public int nearThreatCheckRate = 60;

        [Header("Probabilities")]
        [Range(0, 1)] public float toSelectACoverNearTarget = 0.65f;
        [Tooltip("Probability of the bot to engage the target in hot areas (areas with multiple enemies) or hold position when the target is in an area dominated by the enemy team.")]
        [Range(0, 1)] public float engagementBias = 0.5f;
        [Tooltip("Probability of the bot to hold the position and fire when has a clear shot to the target at long distance.")]
        [Range(0, 1)] public float longShotsProbility = 0.02f;
        [Tooltip("Probability of get a cover point as random destination")]
        [Range(0, 1)] public float randomCoverProbability = 0.1f;

        [Header("Cover")]
        public float maxCoverTime = 10;
        public float coverColdDown = 15;
    }
}
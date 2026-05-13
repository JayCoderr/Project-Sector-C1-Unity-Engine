using MFPS.Internal.Scriptables;
using MFPSEditor;
using UnityEngine;

namespace MFPS.Runtime.Level
{
    public class bl_BulletDecalManager : bl_BulletDecalManagerBase
    {
        public int maxDecalInstances = 100;
        public bl_BulletDecalBase decalPrefab;
        [ScriptableDrawer] public bl_BulletDecalList decalList;

        private bl_BulletDecalBase[] decalPool;
        private int currentPool = -1;
        private Vector3 decalBaseScale = Vector3.one;
        private bool showDecals = true;

        /// <summary>
        /// 
        /// </summary>
        private void Start()
        {
            Init();
        }

        /// <summary>
        /// Instantiate a decal in the raycastHit point
        /// </summary>
        public override void InstanceDecal(RaycastHit raycastHit)
        {
            if (raycastHit.transform == null || !showDecals) return;

            Init();

            if (!showDecals || !ShowDecals) return;

            var decalInstance = GetPool();

            // since the decals are attached to the colliders, if a collider is destroyed, the decal get destroyed as well
            // so we have to make sure it is not null, otherwise, replace the null decal.
            if (decalInstance == null)
            {
                decalPool[currentPool] = Instantiate(decalPrefab.gameObject).GetComponent<bl_BulletDecalBase>();
                decalPool[currentPool].Init(transform);
                decalInstance = decalPool[currentPool];
            }

            var decalData = decalList.GetDecalForTag(raycastHit.collider.transform);

            if (decalData.HasDecals())
            {
                decalInstance
                    .SetDecalMaterial(decalData.GetMaterial())
                    .SetToHit(raycastHit, true)
                    .SetScaleVariation(decalBaseScale, decalData.SizeRange);
            }

            if (decalData.HasHitParticle())
            {
                GameObject go = bl_ObjectPoolingBase.Instance.Instantiate(decalData.HitParticleName, raycastHit.point, Quaternion.LookRotation(raycastHit.normal));
                if (go != null) go.transform.SetParent(raycastHit.transform);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bl_BulletDecalBase GetPool()
        {
            currentPool = (currentPool + 1) % maxDecalInstances;
            return decalPool[currentPool];
        }

        /// <summary>
        /// Initialize the decal pool list
        /// </summary>
        private void Init()
        {
            if (decalPool != null) return;

            showDecals = bl_GameData.CoreSettings.bulletDecals;

            if (!showDecals) return;

            decalBaseScale = decalPrefab.transform.localScale;

            decalPool = new bl_BulletDecalBase[maxDecalInstances];
            for (int i = 0; i < maxDecalInstances; i++)
            {
                decalPool[i] = Instantiate(decalPrefab.gameObject).GetComponent<bl_BulletDecalBase>();
                decalPool[i].Init(transform);
            }
        }

        public override void ClearDecals()
        {
            foreach (var decal in decalPool)
            {
                decal.SetActive(false);
            }
        }
    }
}
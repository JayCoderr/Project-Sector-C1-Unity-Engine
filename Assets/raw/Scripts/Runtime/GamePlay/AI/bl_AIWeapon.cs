using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using MFPSEditor;

namespace MFPS.Runtime.AI
{
    public class bl_AIWeapon : MonoBehaviour
    {
        [Header("Info")]
        [GunID] public int GunID;
        [Range(1, 60)] public int Bullets = 30;
        [Range(1, 6)] public int bulletsPerShot = 1;
        public int maxFollowingShots = 5;
        public string BulletName = "bullet";
        [Header("References")]
        public Transform FirePoint;
        public ParticleSystem MuzzleFlash;
        public Transform GripPosition;
        public AudioClip fireSound;
        public AudioClip[] reloadSounds;

        /// <summary>
        /// 
        /// </summary>
        public void Initialize(bl_AIShooterAttackBase shooterWeapon)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 GetTipPosition()
        {
            if (MuzzleFlash != null) return MuzzleFlash.transform.position;
            return transform.position;
        }

        private bl_GunInfo m_info;
        public bl_GunInfo Info
        {
            get
            {
                if (m_info == null)
                {
                    m_info = bl_GameData.Instance.GetWeapon(GunID); ;
                }
                return m_info;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (FirePoint != null)
            {
                Vector3 origin = FirePoint.position;
                Vector3 dir = transform.root.position + (transform.root.forward * 25);
#if UNITY_EDITOR
                UnityEditor.Handles.color = Color.yellow;
                UnityEditor.Handles.DrawDottedLine(origin, dir, 3);
                UnityEditor.Handles.color = Color.white;
#endif
            }

            if (GripPosition != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(GripPosition.position, 0.02f);
                Gizmos.DrawWireSphere(GripPosition.position, 0.05f);
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(bl_AIWeapon))]
    public class bl_AIWeaponEditor : Editor
    {
        bl_AIWeapon script;

        private void OnEnable()
        {
            script = (bl_AIWeapon)target;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            GUILayout.Space(5);
            if (script.GripPosition == null)
            {
                if (GUILayout.Button("Create Grip Position"))
                {
                    GameObject go = new GameObject("Grip Position");
                    go.transform.SetParent(script.transform);
                    go.transform.localPosition = Vector3.zero;
                    go.transform.localEulerAngles = Vector3.zero;
                    script.GripPosition = go.transform;
                    EditorUtility.SetDirty(target);
                    EditorUtility.SetDirty(go);
                }
            }
            else
            {
                if (GUILayout.Button("Edit Grip Position"))
                {
                    OpenIKWindow();
                }
            }
        }

        void OpenIKWindow()
        {
            script.gameObject.SetActive(true);
            AnimatorRunner window = (AnimatorRunner)EditorWindow.GetWindow(typeof(AnimatorRunner));
            window.Show();
            var pa = script.transform.GetComponentInParent<bl_AIShooterReferences>();
            if (pa == null)
            {
                // log warning
                Debug.LogWarning("Bot references script could not be found.");
                return;
            }
            if (pa.aiAnimation == null)
            {
                // log warning
                Debug.LogWarning("Bot references script does not have an bl_AIAnimation component.");
                return;
            }
            Animator anim = pa.aiAnimation.BotAnimator;
            bl_AIAnimation aia = (pa.aiAnimation as bl_AIAnimation);
            // aia.currentWeapon = script;
            aia.defaultWeaponRootPosition = pa.weaponRoot.localEulerAngles;

            Selection.activeTransform = script.GripPosition;

            window.SetAnim(anim, () =>
            {
                pa.weaponRoot.localEulerAngles = aia.defaultWeaponRootPosition;
                aia.currentWeapon = null;
            });
            Selection.activeObject = script.GripPosition;
        }
    }
#endif
}
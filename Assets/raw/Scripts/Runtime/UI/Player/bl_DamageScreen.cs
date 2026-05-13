using UnityEngine;

namespace MFPS.Runtime.UI
{
    public class bl_DamageScreen : bl_MonoBehaviour
    {
        [Range(1, 10)] public float StartRegenerateIn = 4f;
        [SerializeField] private CanvasGroup alphaCanvas = null;

        private float damageAlphaValue, uiFadeDelay = 0;
        private int lastHealth = 0;

        /// <summary>
        /// 
        /// </summary>
        protected override void OnEnable()
        {
            base.OnEnable();
            bl_EventHandler.Player.onLocalHealthChanged += OnLocalHealthChanged;
            bl_EventHandler.onLocalPlayerSpawn += OnLocalSpawn;
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            bl_EventHandler.Player.onLocalHealthChanged -= OnLocalHealthChanged;
            bl_EventHandler.onLocalPlayerSpawn -= OnLocalSpawn;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="health"></param>
        /// <param name="maxHealth"></param>
        void OnLocalHealthChanged(int health, int maxHealth)
        {
            // if the health has increased
            if (lastHealth < health)
            {
                uiFadeDelay = 0;
            }
            else if (lastHealth > health) // if the health has decreased
            {
                damageAlphaValue = Mathf.Max(1f - ((float)health / (float)maxHealth), 0.25f);
                uiFadeDelay = StartRegenerateIn;
            }

            lastHealth = health;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnLocalSpawn()
        {
            damageAlphaValue = 0;
            uiFadeDelay = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        public override void OnUpdate()
        {
            DamageUI();
        }

        /// <summary>
        /// 
        /// </summary>
        void DamageUI()
        {
            if (damageAlphaValue <= 0)
            {
                alphaCanvas.alpha = 0;
                return;
            }

            alphaCanvas.alpha = Mathf.Lerp(alphaCanvas.alpha, damageAlphaValue, Time.deltaTime * 6);

            if (uiFadeDelay <= 0)
            {
                damageAlphaValue -= Time.deltaTime;
            }
            else
            {
                uiFadeDelay -= Time.deltaTime;
            }
        }
    }
}
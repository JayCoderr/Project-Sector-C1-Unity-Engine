using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Runtime.UI
{
    public class bl_PlayerHealthUI : MonoBehaviour
    {
        public Gradient HealthColorGradient;
        [Header("References")]
        [SerializeField] private GameObject content = null;
        [SerializeField] private TextMeshProUGUI healthText = null;
        [SerializeField] private Image healthBar = null;
        [SerializeField] private Image playerStateImg = null;
        public Sprite StandIcon;
        public Sprite CrouchIcon;

        private Color healthColor;

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            bl_EventHandler.Player.onLocalHealthChanged += OnLocalHealthChanged;
            bl_EventHandler.onUIMaskChanged += OnUIMaskChanged;
            bl_EventHandler.onLocalPlayerStateChanged += OnLocalPlayerStateChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            bl_EventHandler.Player.onLocalHealthChanged -= OnLocalHealthChanged;
            bl_EventHandler.onUIMaskChanged -= OnUIMaskChanged;
            bl_EventHandler.onLocalPlayerStateChanged -= OnLocalPlayerStateChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        void OnLocalHealthChanged(int currentHealth, int maxHealth)
        {
            float h = Mathf.Max(currentHealth, 0);
            float deci = h * 0.01f;
            healthColor = HealthColorGradient.Evaluate(deci);
            if (healthText != null)
            {
                healthText.text = Mathf.FloorToInt(currentHealth).ToString();
                healthText.color = healthColor;
            }
            if (healthBar != null)
            {
                healthBar.fillAmount = deci;
                healthBar.color = healthColor;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="layers"></param>
        void OnUIMaskChanged(RoomUILayers layers)
        {
            content.SetActive(layers.IsEnumFlagPresent(RoomUILayers.PlayerStats));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        void OnLocalPlayerStateChanged(PlayerState from, PlayerState to)
        {
            if (playerStateImg == null) return;

            playerStateImg.sprite = to == PlayerState.Crouching ? CrouchIcon : StandIcon;
        }
    }
}
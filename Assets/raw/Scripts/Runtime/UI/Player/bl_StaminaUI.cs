using UnityEngine;
using UnityEngine.UI;

namespace MFPS.Runtime.UI
{
    public class bl_StaminaUI : MonoBehaviour
    {
        public Gradient StaminaGradient = new Gradient();
        [SerializeField] private GameObject content = null;
        [SerializeField] private Image fillBar = null;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            bl_EventHandler.Player.onStaminaChange += OnStaminaChange;
            bl_EventHandler.onLocalPlayerDeath += OnLocalDeath;
            bl_EventHandler.onLocalPlayerStateChanged += OnLocalStateChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            bl_EventHandler.Player.onStaminaChange -= OnStaminaChange;
            bl_EventHandler.onLocalPlayerDeath -= OnLocalDeath;
            bl_EventHandler.onLocalPlayerStateChanged -= OnLocalStateChanged;
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnStaminaChange(float current, float max)
        {
            SetActive(current < max);
            if (fillBar != null)
            {
                fillBar.fillAmount = current / max;
                fillBar.color = StaminaGradient.Evaluate(current / max);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public void SetActive(bool active)
        {
            if (content != null)
            {
                content.SetActive(active);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void OnLocalDeath()
        {
            SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        void OnLocalStateChanged(PlayerState from, PlayerState to)
        {
            if (to == PlayerState.InVehicle)
            {
                SetActive(false);
            }
        }
    }
}
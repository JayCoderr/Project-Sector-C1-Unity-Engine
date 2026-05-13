using UnityEngine;

namespace MFPS.InputManager
{
    public class bl_GamePadPointer : MonoBehaviour
    {
        public float Resposiveness = 2;
        public float smoothing = 2;
        public GameObject UIObject;

        private RectTransform rectTransform;
        private Canvas _parentCanvas;
        private bool isActive = true;
        private Vector2 currentVelocity;
        private float delay;

        public Canvas parentCanvas
        {
            get
            {
                if (_parentCanvas == null)
                {
                    _parentCanvas = GetComponentInParent<Canvas>();
                }
                return _parentCanvas;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            rectTransform.SetAsLastSibling();
            isActive = gameObject.activeInHierarchy;

            // position the pointer in the center of the screen
            rectTransform.position = new Vector2(Screen.width / 2, Screen.height / 2);
            delay = Time.time + 1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="active"></param>
        public static void SetActivePointer(bool active)
        {
            if (!bl_Input.IsGamePad) return;

            if (Instance != null)
            {
                Instance.SetActive(active);
            }
        }

        public void SetActive(bool active)
        {
            isActive = active;
            UIObject.SetActive(active);
        }

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            if (!isActive) return;

            PadControlled();
        }

        /// <summary>
        /// 
        /// </summary>
        void PadControlled()
        {
            if (Time.time < delay || parentCanvas == null) { return; }

            float inputX = bl_Input.MouseX * Resposiveness;
            float inputY = bl_Input.MouseY * Resposiveness;

            // Calculate the target position
            Vector2 targetPos = rectTransform.anchoredPosition + new Vector2(inputX, inputY);

            // Clamp the target position to the canvas bounds
            targetPos.x = Mathf.Clamp(targetPos.x, (-parentCanvas.pixelRect.width / 2) - 13, (parentCanvas.pixelRect.width / 2) + 13);
            targetPos.y = Mathf.Clamp(targetPos.y, (-parentCanvas.pixelRect.height / 2) - 13, (parentCanvas.pixelRect.height / 2) + 13);

            rectTransform.anchoredPosition = Vector2.SmoothDamp(rectTransform.anchoredPosition, targetPos, ref currentVelocity, smoothing);
        }

        public Vector3 Position => rectTransform.position;
        public Vector3 WorldPosition => rectTransform.position;

        private static bl_GamePadPointer m_instance;
        public static bl_GamePadPointer Instance
        {
            get
            {
                if (m_instance == null) { m_instance = FindAnyObjectByType<bl_GamePadPointer>(); }
                return m_instance;
            }
        }
    }
}
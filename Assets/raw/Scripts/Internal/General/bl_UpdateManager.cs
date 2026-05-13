using System;
using UnityEngine;

namespace MFPS.Internal
{
    public class bl_UpdateManager : MonoBehaviour
    {
        public float SlowUpdateTime = 0.5f;

        private int regularUpdateCount = 0;
        private int fixedUpdateCount = 0;
        private int lateUpdateCount = 0;
        private int slowUpdateCount = 0;

        private bl_MonoBehaviour[] regularArray = new bl_MonoBehaviour[16];
        private bl_MonoBehaviour[] fixedArray = new bl_MonoBehaviour[16];
        private bl_MonoBehaviour[] lateArray = new bl_MonoBehaviour[16];
        private bl_MonoBehaviour[] slowArray = new bl_MonoBehaviour[16];

        private bool initialized = false;
        private float lastSlowCall = 0;

        /// <summary>
        /// Called when the update manager is ready
        /// </summary>
        public static Action onReady;

        private static bl_UpdateManager _instance;
        public static bl_UpdateManager Instance
        {
            get
            {
                if (_instance == null) { _instance = FindAnyObjectByType<bl_UpdateManager>(); }
                return _instance;
            }
        }

        public static bool AddItem(bl_MonoBehaviour behaviour)
        {
            if (Instance == null) return false;

            Instance.AddItemToArray(behaviour);
            return true;
        }

        public static void RemoveSpecificItem(bl_MonoBehaviour behaviour)
        {
            Instance?.RemoveItemFromArray(behaviour);
        }

        public static void RemoveSpecificItemAndDestroyIt(bl_MonoBehaviour behaviour)
        {
            Instance?.RemoveItemFromArray(behaviour);
            Destroy(behaviour.gameObject);
        }

        private void Awake()
        {
            onReady?.Invoke();
        }

        private void Start()
        {
            initialized = true;
        }

        private void OnDestroy()
        {
            ClearArray(ref regularArray, ref regularUpdateCount);
            ClearArray(ref fixedArray, ref fixedUpdateCount);
            ClearArray(ref lateArray, ref lateUpdateCount);
            ClearArray(ref slowArray, ref slowUpdateCount);
        }

        private void AddItemToArray(bl_MonoBehaviour behaviour)
        {
            var type = behaviour.GetType();
            if (type.GetMethod("OnUpdate").DeclaringType != typeof(bl_MonoBehaviour))
            {
                AddToArray(ref regularArray, ref regularUpdateCount, behaviour);
            }
            if (type.GetMethod("OnFixedUpdate").DeclaringType != typeof(bl_MonoBehaviour))
            {
                AddToArray(ref fixedArray, ref fixedUpdateCount, behaviour);
            }
            if (type.GetMethod("OnSlowUpdate").DeclaringType != typeof(bl_MonoBehaviour))
            {
                AddToArray(ref slowArray, ref slowUpdateCount, behaviour);
            }
            if (type.GetMethod("OnLateUpdate").DeclaringType != typeof(bl_MonoBehaviour))
            {
                AddToArray(ref lateArray, ref lateUpdateCount, behaviour);
            }
        }

        private void AddToArray(ref bl_MonoBehaviour[] array, ref int count, bl_MonoBehaviour item)
        {
            if (count == array.Length)
            {
                System.Array.Resize(ref array, array.Length * 2);
            }
            array[count] = item;
            count++;
        }

        private void RemoveItemFromArray(bl_MonoBehaviour behaviour)
        {
            RemoveFromArray(ref regularArray, ref regularUpdateCount, behaviour);
            RemoveFromArray(ref fixedArray, ref fixedUpdateCount, behaviour);
            RemoveFromArray(ref slowArray, ref slowUpdateCount, behaviour);
            RemoveFromArray(ref lateArray, ref lateUpdateCount, behaviour);
        }

        private void RemoveFromArray(ref bl_MonoBehaviour[] array, ref int count, bl_MonoBehaviour item)
        {
            for (int i = 0; i < count; i++)
            {
                if (array[i] == item)
                {
                    array[i] = array[count - 1];
                    array[count - 1] = null;
                    count--;
                    return;
                }
            }
        }

        private void ClearArray(ref bl_MonoBehaviour[] array, ref int count)
        {
            for (int i = 0; i < count; i++)
            {
                array[i] = null;
            }
            count = 0;
        }

        private void Update()
        {
            if (!initialized) return;

            for (int i = 0; i < regularUpdateCount; i++)
            {
                var behaviour = regularArray[i];
                if (behaviour != null && behaviour.enabled)
                {
                    behaviour.OnUpdate();
                }
            }

            SlowUpdate();
        }

        private void SlowUpdate()
        {
            if ((Time.time - lastSlowCall) < SlowUpdateTime) return;

            lastSlowCall = Time.time;
            for (int i = 0; i < slowUpdateCount; i++)
            {
                var behaviour = slowArray[i];
                if (behaviour != null && behaviour.enabled)
                {
                    behaviour.OnSlowUpdate();
                }
            }
        }

        private void FixedUpdate()
        {
            if (!initialized) return;

            for (int i = 0; i < fixedUpdateCount; i++)
            {
                var behaviour = fixedArray[i];
                if (behaviour != null && behaviour.enabled)
                {
                    behaviour.OnFixedUpdate();
                }
            }
        }

        private void LateUpdate()
        {
            if (!initialized) return;

            for (int i = 0; i < lateUpdateCount; i++)
            {
                var behaviour = lateArray[i];
                if (behaviour != null && behaviour.enabled)
                {
                    behaviour.OnLateUpdate();
                }
            }
        }
    }
}

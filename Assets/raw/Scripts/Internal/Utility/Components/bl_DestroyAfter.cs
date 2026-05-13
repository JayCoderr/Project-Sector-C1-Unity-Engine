using System;
using UnityEngine;
using UnityEngine.Events;

public class bl_DestroyAfter : MonoBehaviour
{
    public DestroyType destroyType = DestroyType.Destroy;
    public float destroyAfter = 15.0f;
    public UEvent onDestroyEvent;

    [Serializable]
    public class UEvent : UnityEvent { }

    void OnEnable()
    {
        if (destroyAfter > 0)
        {
            if (destroyType == DestroyType.Destroy)
                Destroy(gameObject, destroyAfter);
            else
                Invoke(nameof(Desactive), destroyAfter);
        }
    }

    public void Desactive()
    {
        if (destroyType == DestroyType.Event)
            onDestroyEvent?.Invoke();
        else
            gameObject.SetActive(false);
    }

    public enum DestroyType
    {
        Destroy,
        Disable,
        Event
    }
}
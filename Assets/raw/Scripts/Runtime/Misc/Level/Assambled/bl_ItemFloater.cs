using UnityEngine;

public class bl_ItemFloater : MonoBehaviour
{
    [SerializeField] private float floatDistance = 0.5f;  // Max distance to float up from start position
    [SerializeField] private float floatSpeed = 1.5f;     // Speed of the float movement
    [SerializeField] private float rotationSpeed = 50f;   // Speed of the rotation

    private Vector3 startPosition;  // Cached bottom-most position
    private float timeOffset;       // Cached time offset for unique floating per object
    private Transform cachedTransform;  // Cached transform for efficient access

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        // Cache transform to avoid repeated native calls
        cachedTransform = transform;

        // Cache the bottom-most position (start at the lowest point of the movement)
        startPosition = cachedTransform.position;

        // Add random offset to make multiple items float asynchronously
        timeOffset = Random.Range(0f, Mathf.PI * 2);
    }

    /// <summary>
    /// 
    /// </summary>
    void Update()
    {
        // Calculate the Y position using the sine wave, with start position as the lowest point
        float newY = startPosition.y + (Mathf.Sin((Time.time + timeOffset) * floatSpeed) + 1) * 0.5f * floatDistance;

        // Only update Y value (avoid creating new Vector3 objects)
        Vector3 currentPosition = cachedTransform.position;
        cachedTransform.position = new Vector3(currentPosition.x, newY, currentPosition.z);

        // Apply rotation directly to avoid Vector3 and native calls
        cachedTransform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }
}
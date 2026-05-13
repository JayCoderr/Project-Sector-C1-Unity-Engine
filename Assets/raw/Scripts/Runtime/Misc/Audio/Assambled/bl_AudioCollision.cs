using UnityEngine;

public class bl_AudioCollision : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioClip[] collisionSounds;   // List of audio clips to play
    public float soundCooldown = 0.5f;    // Minimum time between sounds in seconds

    private float lastSoundTime = 0;
    private AudioSource audioSource;

    /// <summary>
    /// 
    /// </summary>
    void Start()
    {
        // Get the AudioSource component or add one if it doesn't exist
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        TryPlaySound();
    }

    /// <summary>
    /// 
    /// </summary>
    void TryPlaySound()
    {
        // Check if enough time has passed since the last sound was played
        if (Time.time - lastSoundTime >= soundCooldown)
        {
            if (collisionSounds.Length > 0)
            {
                // Choose a random audio clip from the list
                int randomIndex = Random.Range(0, collisionSounds.Length);
                AudioClip clip = collisionSounds[randomIndex];

                // Play the selected audio clip
                audioSource.PlayOneShot(clip);

                // Update the time when the last sound was played
                lastSoundTime = Time.time;
            }
        }
    }
}

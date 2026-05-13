using UnityEngine;

public class bl_FlashExplosion : MonoBehaviour
{
    public float radius = 10;
    [Tooltip("The required angle of the player camera with respect to the flashbang origin to not be affected.")]
    public float safeViewAngle = 90;
    public float maxBlindDuration = 7;
    public float maxFadeDuration = 2;
    [LovattoToogle] public bool distanceAffectDuration = false;

    public static RenderTexture RenderTextureCached = null;

    /// <summary>
    /// 
    /// </summary>
    private void Start()
    {
        DoExplosion();
        this.InvokeAfter(1, () =>
        {
            Destroy(gameObject);
        });
    }

    /// <summary>
    /// 
    /// </summary>
    void DoExplosion()
    {
        var localPlayerRef = bl_MFPS.LocalPlayerReferences;

        // if the local player reference is null, it means that the player is not instanced yet or is dead
        if (localPlayerRef == null) return;

        float distance = Vector3.Distance(transform.position, localPlayerRef.transform.position);

        // if the player is not in the radius of the flashbang, return
        if (distance > radius)
        {
            return;
        }

        float effect = 1;
        if (distanceAffectDuration)
        {
            // calculate the percentage of the flashbang effect
            effect = 1 - (distance / radius);
        }

        Vector3 flashDir = transform.position - localPlayerRef.transform.position;
        // check the player camera angle with respect to the flashbang
        float angle = Vector3.Angle(flashDir, localPlayerRef.PlayerCameraTransform.forward);

        // if the player is not looking at the flashbang
        if (angle > safeViewAngle)
        {
            return;
        }
        else
        {
            // calculate the percentage of the flashbang effect
            effect *= 1 - (angle / safeViewAngle);
        }

        // check if there is an obstacle between the player and the flashbang
        if (Physics.Linecast(localPlayerRef.PlayerCameraTransform.position, transform.position, out RaycastHit hit, bl_GameData.TagsAndLayerSettings.EnvironmentAndVehicles, QueryTriggerInteraction.Ignore))
        {
            if (!hit.transform.CompareTag("Projectile"))
            {
                return;
            }
        }

#if MFPS_VEHICLE
        if (localPlayerRef.IsInVehicle() && localPlayerRef.PlayerVehicle != null && localPlayerRef.PlayerVehicle.CurrentSeat != null)
        {
            if (!localPlayerRef.PlayerVehicle.CurrentSeat.CanBeFlashed)
            {
                return;
            }
        }
#endif

        // apply the flashbang effect to the player
        // take a screenshot of the player camera
        var frameTexture = RenderFrame();
        // apply the flashbang effect to the player
        bl_FlashbangUI.Instance?.DoFlash(effect, frameTexture, this);
        localPlayerRef.playerAnimations.CustomCommand(PlayerAnimationCommands.OnFlashbang);

        var flashSnapshot = bl_GlobalReferences.I.MFPSAudioMixer.FindSnapshot("Flashed");
        if (flashSnapshot != null) { flashSnapshot.TransitionTo(0.5f); }

        // the local effects are only applied to the local player
        // they will be reverted in the bl_FlashbangUI class
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    private Texture2D RenderFrame()
    {
        int screenshotWidth = Screen.width;
        int screenshotHeight = Screen.height;
        Camera playerCamera = bl_MFPS.LocalPlayerReferences.playerCamera;
        if (playerCamera == null || !playerCamera.gameObject.activeInHierarchy)
        {
            playerCamera = bl_CameraIdentity.CurrentCamera;
        }

        if (RenderTextureCached == null)
        {
            RenderTextureCached = new RenderTexture(screenshotWidth, screenshotHeight, 24);
        }

        playerCamera.targetTexture = RenderTextureCached;
        Texture2D screenShot = new(screenshotWidth, screenshotHeight, TextureFormat.RGB24, false);
        playerCamera.Render();
        RenderTexture.active = RenderTextureCached;
        screenShot.ReadPixels(new Rect(0, 0, screenshotWidth, screenshotHeight), 0, 0);
        playerCamera.targetTexture = null;
        RenderTexture.active = null;
        screenShot.Apply();

        return screenShot;
    }
}
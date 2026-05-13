using System.Collections;
using UnityEngine;

/// <summary>
/// Camera Identity is used to identify which camera is rendering currently
/// instead of use the Unity Camera.current which is slow and can't be used in loop functions
/// You must attach this script in any camera that you want to use in game.
/// If you want to get the current camera that is rendered, use: bl_GameManager.Instance.CameraRendered.
/// </summary>
public class bl_CameraIdentity : MonoBehaviour
{
    private Camera m_Camera;
    private static Camera m_currentRenderCamera = null;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        StartCoroutine(Set());
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    IEnumerator Set()
    {
        yield return new WaitForSeconds(0.5f);
        if (m_Camera == null)
        {
            m_Camera = GetComponent<Camera>();
        }
        if (m_Camera != null) CurrentCamera = m_Camera;
    }

    /// <summary>
    /// 
    /// </summary>  
    public static Camera CurrentCamera
    {
        get
        {
            return m_currentRenderCamera == null ? Camera.current : m_currentRenderCamera;
        }
        set
        {
            if (m_currentRenderCamera != null && m_currentRenderCamera.isActiveAndEnabled)
            {
                //if the current render over the set camera, keep it as renderer camera
                if (m_currentRenderCamera.depth >= value.depth) return;
            }
            m_currentRenderCamera = value;
        }
    }
}
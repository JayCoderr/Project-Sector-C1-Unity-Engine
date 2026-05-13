using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// This script is used to store the positions of the room camera.
/// With this you can set the camera to a specific position on certain events, e.g when the player enters a room, the spectator camera start position, etc.
/// All you have to do is to attach this script in a empty gameobject in your map scene and set the positions in the inspector <see cref="positions"/> list.
/// The default positions Key are: "spectator-start", "match-start", "match-finish" 
/// </summary>
public class bl_RoomCameraPositions : MonoBehaviour
{
    [Serializable]
    public class PositionData
    {
        public string Key;
        public Transform Position;
        [LovattoToogle] public bool Moving = false;
        public float MoveSpeed = 4;
        public Vector3 MoveDirection;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Vector3 GetMoveDirection()
        {
            return Position.position - MoveDirection;
        }
    }

    public List<PositionData> positions;

    /// <summary>
    /// 
    /// </summary>
    private void OnEnable()
    {
        bl_EventHandler.onRoundEnd += OnMatchFinish;
        bl_EventHandler.onLocalPlayerSpawn += OnLocalSpawn;
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDisable()
    {
        bl_EventHandler.onRoundEnd -= OnMatchFinish;
        bl_EventHandler.onLocalPlayerSpawn -= OnLocalSpawn;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public PositionData GetPosition(string key)
    {
        return positions.Find(x => x.Key == key);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="positionKey"></param>
    public static void SetRoomCameraToPosition(string positionKey)
    {
        if (Instance == null) return;

        Instance.StopAllCoroutines();

        PositionData data = Instance.GetPosition(positionKey);
        if (data != null)
        {
            Transform t = bl_RoomCameraBase.Instance.transform;
            t.SetPositionAndRotation(data.Position.position, data.Position.rotation);
            if (data.Moving)
            {
                bl_RoomCameraBase.Instance.BlockSelfMovement = true;
                Instance.StartCoroutine(Instance.Move(data));
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    private IEnumerator Move(PositionData data)
    {
        if (bl_RoomCameraBase.Instance == null || !data.Moving) yield break;

        Transform t = bl_RoomCameraBase.Instance.transform;
        Vector3 moveDir = data.GetMoveDirection();
        while (true)
        {
            t.position += data.MoveSpeed * Time.deltaTime * moveDir;
            yield return null;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    void OnMatchFinish()
    {
        // give 3 seconds to the player keep seeing his perspective before change to the match finish camera
        this.InvokeAfter(3, () =>
        {
            bl_RoomCameraBase.Instance.SetCameraMode(bl_RoomCameraBase.CameraMode.MatchFinish);
        });
    }

    /// <summary>
    /// 
    /// </summary>
    void OnLocalSpawn()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// 
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // draw a frustum gizmo for each position
        foreach (PositionData data in positions)
        {
            if (data.Position == null) continue;

            Matrix4x4 temp = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(data.Position.position, data.Position.rotation, Vector3.one);
            Gizmos.DrawFrustum(Vector3.zero, 60, 3f, 0.5f, 1.78f);
            Gizmos.matrix = temp;

#if UNITY_EDITOR
            Handles.color = Color.yellow;
            Handles.Label(data.Position.position + data.Position.right, data.Key, EditorStyles.miniBoldLabel);
#endif

            if (data.Moving)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(data.Position.position, data.GetMoveDirection());
            }
        }
    }

    private static bl_RoomCameraPositions _instance = null;
    public static bl_RoomCameraPositions Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<bl_RoomCameraPositions>();
            }
            return _instance;
        }
    }
}
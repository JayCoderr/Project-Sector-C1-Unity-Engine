using MFPS.Internal.BaseClass;
using MFPS.Internal.Scriptables;
using MFPS.Runtime.Motion;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "GlobalReferences", menuName = "MFPS/Settings/Global References")]
public class bl_GlobalReferences : ScriptableObject
{
    public bl_DatabaseBase databaseHandler;
    public bl_SceneLoaderBase sceneLoader;
    public AudioMixer MFPSAudioMixer;
    public TextAsset BadWordsDatabase;
    public GameObject PlayerPlaceholder;
    public bl_SpringTransform aimSpring;
    public MFPSTeam globalTeam;
    public List<WeaponGenericAnimation> GenericWeaponAnimators;
    [HideInInspector] public List<MethodInfoCache> gameItemCollectors;
#if UNITY_EDITOR
    public Mesh SpawnPointPlayerGizmo;
#endif

    /// <summary>
    /// Get the generic weapon animation based in the guntype
    /// </summary>
    /// <param name="gunType"></param>
    /// <returns></returns>
    public static RuntimeAnimatorController GetGenericWeaponAnimationFor(GunType gunType)
    {
        int index = I.GenericWeaponAnimators.FindIndex(x => x.GunType == gunType);
        if (index == -1) return I.GenericWeaponAnimators[0].Animator;

        return I.GenericWeaponAnimators[index].Animator;
    }

    /// <summary>
    /// 
    /// </summary>
    public static bl_DatabaseBase DatabaseBase
    {
        get
        {
            if (I.databaseHandler == null)
            {
                Debug.LogError("Missing Database Handler in Global References");
                return null;
            }
            return I.databaseHandler;
        }
    }

    [Serializable]
    public class WeaponGenericAnimation
    {
        public GunType GunType;
        public RuntimeAnimatorController Animator;
    }

    [Serializable]
    public class MethodInfoCache
    {
        public string typeName;
        public string methodName;
    }

    private static bl_GlobalReferences m_Data;
    public static bl_GlobalReferences I
    {
        get
        {
            if (m_Data == null)
            {
                m_Data = Resources.Load("GlobalReferences", typeof(bl_GlobalReferences)) as bl_GlobalReferences;
            }
            return m_Data;
        }
    }
}
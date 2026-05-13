using MFPSEditor;
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MFPS.Internal.Structures
{
    [Serializable]
    public class MapInfo
    {
        public string ShowName;
        [SerializeField]
        public Object m_Scene;
        [Tooltip("Name of the Unity Scene Asset.")]
        [HideInInspector] public string RealSceneName;
        [Tooltip("Path of the scene in the build settings.")]
        [HideInInspector] public string Path;
        [SpritePreview] public Sprite Preview;
        public List<GameMode> NoAllowedGameModes = new();

        /// <summary>
        /// Return only the allowed game modes in this map from the given modes list.
        /// </summary>
        /// <returns></returns>
        public GameModeSettings[] GetAllowedGameModes(GameModeSettings[] allAvailables)
        {
            if (allAvailables == null || allAvailables.Length == 0) return allAvailables;

            var list = new List<GameModeSettings>();
            for (int i = 0; i < allAvailables.Length; i++)
            {
                if (!NoAllowedGameModes.Contains(allAvailables[i].gameMode))
                {
                    list.Add(allAvailables[i]);
                }
            }
            return list.ToArray();
        }

        public void Validate()
        {
            if (m_Scene != null)
            {
                RealSceneName = m_Scene.name;
#if UNITY_EDITOR
                Path = AssetDatabase.GetAssetPath(m_Scene);
#endif
            }
            else
            {
                RealSceneName = string.Empty;
                Path = string.Empty;
            }
        }
    }

    [Serializable]
    public class MFPSSceneAsset
    {
#if UNITY_EDITOR
        public SceneAsset Scene;
#endif
        [HideInInspector] public string Name;
        [HideInInspector] public string Path;

        public void Validate()
        {
#if UNITY_EDITOR
            if (Scene == null)
            {
                Name = string.Empty;
                Path = string.Empty;
                return;
            }
            Name = Scene.name;
            Path = AssetDatabase.GetAssetPath(Scene);
#endif
        }

        public bool IsEmpty()
        {
            return string.IsNullOrEmpty(Name);
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MFPS.Internal.BaseClass
{
    public abstract class bl_SceneLoaderBase : ScriptableObject
    {
        /// <summary>
        /// 
        /// </summary>
        public abstract void LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Single);

        /// <summary>
        /// 
        /// </summary>
        public abstract void LoadScene(int sceneID, LoadSceneMode loadSceneMode = LoadSceneMode.Single);

        /// <summary>
        /// Special method to load a map scenes
        /// Since the map scene is loaded additive, the UI scene is loaded first
        /// </summary>
        /// <param name="mapSceneName"></param>
        public virtual void LoadMapScene(string mapSceneName)
        {
            LoadScene(bl_GameData.Instance.UIScene.Name);
            LoadScene(mapSceneName, LoadSceneMode.Additive);
        }

        /// <summary>
        /// 
        /// </summary>
        private static bl_SceneLoaderBase _instance;
        public static bl_SceneLoaderBase Instance
        {
            get
            {
                if (_instance == null) { _instance = FindAnyObjectByType<bl_SceneLoaderBase>(); }
                return _instance;
            }
        }
    }
}
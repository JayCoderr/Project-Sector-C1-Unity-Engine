using MFPS.Tween;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace MFPS.Runtime.UI
{
    public class bl_MapSceneLoader : bl_MapSceneLoaderBase
    {
        public Image progressBar;
        [SerializeField] private GameObject content = null;
        [SerializeField] private bl_TweenCurveAlpha rootAlphaTween = null;
        [SerializeField] private TextMeshProUGUI loadingText = null;
        [SerializeField] private TextMeshProUGUI gameInfoText = null;
        [SerializeField] private RawImage background = null;

        private float currentProgress = 0.0f;

        /// <summary>
        /// 
        /// </summary>
        private void Awake()
        {
            // Ensure this object persists between scene loads in order to maintain loading progress when transitioning between scenes
            DontDestroyOnLoad(gameObject);
            content.SetActive(false);
        }

        /// <summary>
        /// 
        /// </summary>
        private void ResetAll()
        {
            currentProgress = 0.0f;
            progressBar.fillAmount = 0.0f;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapSceneName"></param>
        public override void LoadMap(string mapSceneName)
        {
            StartCoroutine(LoadScenesAsync(mapSceneName));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="mapSceneName"></param>
        /// <returns></returns>
        private IEnumerator LoadScenesAsync(string mapSceneName)
        {
            content.SetActive(true);
            ResetAll();
            SetLoadingText($"{bl_GameTexts.LoadingUI.Localized(276)}...");

            var mapInfo = bl_MFPS.GameData.GetMapInfo(mapSceneName);

            if (background != null && mapInfo.Preview != null)
            {
                background.texture = mapInfo.Preview.texture;
            }

            var gameModeInfo = bl_MFPS.RoomGameMode.CurrentGameModeData;
            if (gameInfoText != null)
            {
                gameInfoText.text = $"{mapInfo.ShowName} | {gameModeInfo.ModeName}";
            }

            yield return new WaitForSeconds(rootAlphaTween.Duration);

            // Load UI Scene synchronously
            SceneManager.LoadScene(bl_GameData.Instance.UIScene.Name);

            UpdateProgressBar(0.1f);

            SetLoadingText($"{bl_GameTexts.LoadingMap.Localized(277)}...");

            // Load Map Scene
            AsyncOperation mapLoadOperation = SceneManager.LoadSceneAsync(mapInfo.RealSceneName, LoadSceneMode.Additive);
            mapLoadOperation.allowSceneActivation = false;

            while (currentProgress < 1f)
            {
                // Calculate weighted progress (90% for Map Scene)
                float totalProgress = 0.25f + (mapLoadOperation.progress / 0.9f * 0.9f);
                UpdateProgressBar(totalProgress);
                yield return null;
            }
            SetLoadingText($"{bl_GameTexts.LoadingCompleteJoining.Localized(278)}...");

            // Set progress bar to 100% to indicate loading is complete but not activated
            currentProgress = 1f;
            UpdateProgressBar(1.0f);
            yield return new WaitForSeconds(0.2f);

            // Now, activate the Map Scene
            mapLoadOperation.allowSceneActivation = true;

            // Wait until the scene has fully activated
            while (!mapLoadOperation.isDone)
            {
                yield return null;
            }
            SetLoadingText($"{bl_GameTexts.Done.Localized(273)}...");

            if (gameObject.activeInHierarchy) rootAlphaTween.StartReverseTween(true);
            yield return new WaitForSeconds(rootAlphaTween.Duration);
            Destroy(gameObject);
        }

        private void UpdateProgressBar(float progress)
        {
            currentProgress = Mathf.Lerp(currentProgress, progress, Time.smoothDeltaTime);
            if (progressBar != null)
            {
                progressBar.fillAmount = progress; // Assuming the Slider's min value is 0 and max value is 1
            }
        }

        private void SetLoadingText(string text)
        {
            if (loadingText != null)
            {
                loadingText.text = text;
            }
        }
    }
}
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;
#if UNITY_EDITOR
using UnityEditor;

namespace MFPSEditor
{
    public class bl_MFPSPlayerPrefabsList : EditorWindow
    {
        public List<PlayerData> players;
        public EditorSpinnerGUI loadingSpinner;
        public Action<bl_PlayerReferences, bl_MFPSPlayerPrefabsList> onClickPlayer;

        public static Texture2D _soldierIcon = null;
        readonly float itemWidth = 70; // Width of each item
        readonly float itemHeight = 70; // Height of each item
        readonly float padding = 20; // Space between items
        private Vector2 scroll;
        private string titleText;

        public class PlayerData
        {
            public string Name;
            public bl_PlayerReferences prefab;
            public Texture2D Icon;

            private bool loadingIcon = false;

            public PlayerData(GameObject pref, Texture2D icon = null)
            {
                if (pref == null) return;

                prefab = pref.GetComponent<bl_PlayerReferences>();
                Name = prefab.name;
                Icon = icon;
            }

            public void LoadIcon(EditorWindow window)
            {
                if (loadingIcon) return;

                loadingIcon = true;

                Icon = AssetPreview.GetMiniThumbnail(prefab.gameObject);
                window.Repaint();
            }

            public void CleanUp()
            {
                Icon = null;
                loadingIcon = false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnEnable()
        {
            titleContent = new GUIContent("Player List");
            loadingSpinner = new EditorSpinnerGUI();
            loadingSpinner.Initializated(this);
            minSize = new Vector2(300, 300);

            LoadPlayers();
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnDisable()
        {
            if (players != null)
            {
                foreach (var item in players)
                {
                    item.CleanUp();
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void OnGUI()
        {

            GUILayout.Space(20);
            PlayerList();

            if (loadingSpinner.IsActive)
            {
                loadingSpinner.DrawSpinnerOnMiddle();
                Repaint();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void PlayerList()
        {
            if (players == null || players.Count == 0)
            {
                GUILayout.Label("No player prefabs found");
                return;
            }

            if (!string.IsNullOrEmpty(titleText))
            {
                GUILayout.Label(titleText, TutorialWizard.Style.TextStyle);
                GUILayout.Space(20);
            }

            float windowWidth = this.position.width; // Get the width of the window
            int itemsPerRow = Mathf.Max(1, Mathf.FloorToInt((windowWidth - padding) / (itemWidth + padding))); // Calculate number of items per row

            scroll = GUILayout.BeginScrollView(scroll);
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical();

            int itemIndex = 0;
            int itemCount = players.Count;

            while (itemIndex < itemCount)
            {
                GUILayout.BeginHorizontal();
                {
                    for (int i = 0; i < itemsPerRow && itemIndex < itemCount; i++)
                    {
                        var player = players[itemIndex];
                        if (player == null) continue;

                        if (GUILayout.Button(GUIContent.none, GUIStyle.none, GUILayout.Width(itemWidth), GUILayout.Height(itemHeight)))
                        {
                            onClickPlayer?.Invoke(player.prefab, this);
                        }

                        var r = GUILayoutUtility.GetLastRect();
                        EditorGUI.DrawRect(r, new Color(0, 0, 0, 0.3f));
                        if (player.Icon == null)
                        {
                            GUI.DrawTexture(r, GetSoldierIcon(), ScaleMode.ScaleToFit);
                            player.LoadIcon(this);
                        }
                        else
                        {
                            GUI.DrawTexture(r, player.Icon, ScaleMode.ScaleToFit);
                        }

                        var rr = r;
                        rr.y += itemHeight;
                        rr.height = 40;
                        EditorStyles.miniBoldLabel.wordWrap = true;
                        GUI.Label(rr, player.prefab.name, EditorStyles.miniBoldLabel);

                        GUILayout.Space(padding);

                        itemIndex++;
                    }
                }
                GUILayout.EndHorizontal();
                GUILayout.Space(padding + 20);
            }

            EditorGUILayout.EndVertical();
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
            GUILayout.EndScrollView();
        }

        /// <summary>
        /// 
        /// </summary>
        async void LoadPlayers()
        {
            players = new List<PlayerData>();

            loadingSpinner.SetActive(true);
            var gameDataOp = Resources.LoadAsync("GameData");
            while (!gameDataOp.isDone)
            {
                await Task.Delay(100);
            }

            if (bl_GameData.Instance.Player1 != null) AddPlayer(bl_GameData.Instance.Player1.gameObject);
            if (bl_GameData.Instance.Player2 != null) AddPlayer(bl_GameData.Instance.Player2.gameObject);

#if PSELECTOR
            var pseop = Resources.LoadAsync("PlayerSelector");
            while (!pseop.isDone)
            {
                await Task.Delay(100);
            }

            foreach (var item in bl_PlayerSelector.Data.AllPlayers)
            {
                if (item.Prefab == null) continue;

                var icon = item.Icon != null ? item.Icon.texture : null;
                AddPlayer(item.Prefab, icon);
            }
#endif

            loadingSpinner.SetActive(false);
        }

        private void AddPlayer(GameObject prefab, Texture2D icon = null)
        {
            if (prefab == null) return;
            if (players.Exists(x => x.Name == prefab.name)) return;

            players.Add(new PlayerData(prefab, icon));
        }

        public void SetTitle(string title) => titleText = title;

        // [MenuItem("Lovatto/Player List")]
        public static void Open()
        {
            GetWindow<bl_MFPSPlayerPrefabsList>();
        }

        public static Texture2D GetSoldierIcon()
        {
            if (_soldierIcon == null)
            {
                _soldierIcon = AssetDatabase.LoadAssetAtPath("Assets/MFPS/Content/Art/UI/Icons/mfps-soldier.png", typeof(Texture2D)) as Texture2D;
            }
            return _soldierIcon;
        }
    }

}
#endif
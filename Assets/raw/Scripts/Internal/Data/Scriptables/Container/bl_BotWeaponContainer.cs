using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "BotWeaponContainer", menuName = "MFPS/Loadout/Bot Weapon Container")]
public class bl_BotWeaponContainer : ScriptableObject
{
    [System.Serializable]
    public class BotWeaponData
    {
        [HideInInspector] public string WeaponName;
        [GunID] public int GunID;
        public int MaxFollowingShots = 5;
        public AudioClip[] ReloadSounds;
    }

    public List<BotWeaponData> weapons;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public BotWeaponData GetWeapon(int id)
    {
        return weapons.Find(x => x.GunID == id);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(bl_BotWeaponContainer))]
public class bl_BotWeaponContainerEditor : Editor
{
    public bl_BotWeaponContainer script;

    private void OnEnable()
    {
        script = (bl_BotWeaponContainer)target;
    }

    public override void OnInspectorGUI()
    {
        EditorGUI.BeginChangeCheck();
        base.OnInspectorGUI();

        if (EditorGUI.EndChangeCheck())
        {
            UpdateNames();
            EditorUtility.SetDirty(script);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    private void UpdateNames()
    {
        var names = bl_GameData.Instance.AllWeaponStringList(true, true);
        for (int i = 0; i < script.weapons.Count; i++)
        {
            if (script.weapons[i].WeaponName == names[script.weapons[i].GunID]) continue;

            script.weapons[i].WeaponName = names[script.weapons[i].GunID];
        }
    }
}
#endif
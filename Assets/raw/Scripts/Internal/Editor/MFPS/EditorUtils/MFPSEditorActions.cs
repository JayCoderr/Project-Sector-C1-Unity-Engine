using UnityEditor;
using UnityEngine;

public static class MFPSEditorActions
{

    [MenuItem("MFPS/Actions/Reset default server")]
    static void ResetDefaultServer()
    {
        PlayerPrefs.DeleteKey(GetUniqueKey("preferredregion"));
    }

    [MenuItem("MFPS/Actions/Delete Player Prefs")]
    static void DeleteAllPlayerPrefs()
    {
        if (EditorUtility.DisplayDialog("Delete Prefs", "Are you sure to delete all the PlayerPrefs?", "Yes", "Cancel"))
            PlayerPrefs.DeleteAll();
    }

    public static string GetUniqueKey(string key)
    {
        return string.Format("{0}.{1}.{2}", Application.companyName, Application.productName, key);
    }
}
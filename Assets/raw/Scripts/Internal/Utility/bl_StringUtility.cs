using MFPS.Internal.Utility;
using System.Text.RegularExpressions;
using UnityEngine;

public static class bl_StringUtility
{
    static bl_Trie trie;

    [System.Serializable]
    public class BadWordModel
    {
        public string[] words;
    }

    /// <summary>
    /// Get string in time format
    /// </summary>
    public static string GetTimeFormat(float m, float s)
    {
        return string.Format("{0:00}:{1:00}", m, s);
    }

    /// <summary>
    /// 
    /// </summary>
    public static string GetTimeFormat(int seconds)
    {
        int minutes = seconds > 60 ? seconds / 60 : 0;
        int realSeconds = seconds % 60;
        return string.Format("{0:00}:{1:00}", minutes, realSeconds);
    }

    /// <summary>
    /// Make a displayable name for a variable like name.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string NicifyVariableName(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        string result = "";
        for (int i = 0; i < str.Length; i++)
        {
            if (i == 0)
            {
                result += char.ToUpper(str[i]);
                continue;
            }
            if (char.IsUpper(str[i]) == true && i != 0)
            {
                result += " ";
            }

            result += str[i];
        }
        return result;
    }

    /// <summary>
    /// Generate a random string with the given length
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public static string GenerateKey(int length = 7)
    {
        string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789abcdefghlmnopqrustowuvwxyz";
        string key = "";
        for (int i = 0; i < length; i++)
        {
            key += chars[Random.Range(0, chars.Length)];
        }
        return key;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="text"></param>
    /// <param name="trie"></param>
    /// <returns></returns>
    public static bool ContainsProfanity(string text, out string filteredText, bool lazySearch = false)
    {
        if (trie == null)
        {
            var wordsDatabase = JsonUtility.FromJson<BadWordModel>(bl_GlobalReferences.I.BadWordsDatabase.text);
            trie = new bl_Trie();

            foreach (string word in wordsDatabase.words)
            {
                trie.Insert(word.ToLowerInvariant());
            }
        }

        filteredText = text;
        string cleanedText = Regex.Replace(text, @"\W+", " ");
        string[] words = cleanedText.Split(' ', System.StringSplitOptions.RemoveEmptyEntries);

        bool found = false;
        foreach (string word in words)
        {
            if (trie.Contains(word.ToLowerInvariant()))
            {
                string replaced = new string('*', word.Length);
                filteredText = filteredText.Replace(word, replaced);
                found = true;
                if (lazySearch) return true;
            }
        }

        return found;
    }
}
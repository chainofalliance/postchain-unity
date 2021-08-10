using System.Runtime.InteropServices;

namespace Chromia.Postchain.Ft3
{
    public class FileIOWrapper
    {
#if UNITY_WEBGL
        [DllImport("__Internal")]
        private static extern void SaveToLocalStorage(string key, string value);

        [DllImport("__Internal")]
        private static extern string LoadFromLocalStorage(string key);

        [DllImport("__Internal")]
        private static extern void RemoveFromLocalStorage(string key);

        [DllImport("__Internal")]
        private static extern int HasKeyInLocalStorage(string key);
#endif

        public static void DeleteKey(string key)
        {
#if UNITY_WEBGL
            RemoveFromLocalStorage(key: key);
#else
        UnityEngine.Debug.Log("DELETE BEFORE");
        UnityEngine.PlayerPrefs.DeleteKey(key: key);
        UnityEngine.Debug.Log("DELETE AFTER");
#endif
        }

        public static bool HasKey(string key)
        {
#if UNITY_WEBGL
            return (HasKeyInLocalStorage(key) == 1);
#else
        return (UnityEngine.PlayerPrefs.HasKey(key: key));
#endif
        }

        public static string GetString(string key)
        {
#if UNITY_WEBGL
            return LoadFromLocalStorage(key: key);
#else
        return (UnityEngine.PlayerPrefs.GetString(key: key));
#endif
        }

        public static void SetString(string key, string value)
        {
#if UNITY_WEBGL
            SaveToLocalStorage(key: key, value: value);
#else
        UnityEngine.PlayerPrefs.SetString(key: key, value: value);
#endif
        }

        public static void Save()
        {
#if !UNITY_WEBGL
        UnityEngine.PlayerPrefs.Save();
#endif
        }
    }
}
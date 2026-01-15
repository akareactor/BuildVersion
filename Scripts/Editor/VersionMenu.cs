using UnityEditor;
using UnityEngine;

namespace KulibinSpace.BuildVersion {

    public class VersionMenu {
        [MenuItem("Build/Version Manager")]
        public static void ShowVersionManager () {
            VersionData versionData = GetOrCreateVersionData();
            VersionWindow.ShowWindow(versionData, null);
        }

        [MenuItem("Build/Quick Version Info")]
        public static void ShowQuickVersionInfo () {
            VersionData versionData = GetOrCreateVersionData();
            EditorUtility.DisplayDialog("Version Info", versionData.GetFullVersionString(), "OK");
        }

        private static VersionData GetOrCreateVersionData () {
            string[] guids = AssetDatabase.FindAssets("t:VersionData");
            if (guids.Length > 0) {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<VersionData>(path);
            } else {
                VersionData newVersionData = ScriptableObject.CreateInstance<VersionData>();
                AssetDatabase.CreateAsset(newVersionData, "Assets/VersionData.asset");
                AssetDatabase.SaveAssets();
                return newVersionData;
            }
        }
    }

}

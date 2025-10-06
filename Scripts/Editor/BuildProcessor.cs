using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildProcessor : IPreprocessBuildWithReport {
    public int callbackOrder => 0;

    private static bool buildContinued = false;

    public void OnPreprocessBuild (BuildReport report) {
        if (buildContinued) {
            buildContinued = false;
            return;
        }

        // Находим или создаем VersionData
        VersionData versionData = GetOrCreateVersionData();

        // Останавливаем сборку и показываем окно
        throw new BuildFailedException("Build paused for version confirmation");
    }

    private VersionData GetOrCreateVersionData () {
        string[] guids = AssetDatabase.FindAssets("t:VersionData");

        if (guids.Length > 0) {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<VersionData>(path);
        } else {
            // Создаем новый VersionData
            VersionData newVersionData = ScriptableObject.CreateInstance<VersionData>();
            AssetDatabase.CreateAsset(newVersionData, "Assets/VersionData.asset");
            AssetDatabase.SaveAssets();
            return newVersionData;
        }
    }

    [InitializeOnLoadMethod]
    private static void Initialize () {
        BuildPlayerWindow.RegisterBuildPlayerHandler(OnBuildPlayer);
    }

    private static void OnBuildPlayer (BuildPlayerOptions options) {
        VersionData versionData = GetVersionDataStatic();

        VersionWindow.ShowWindow(versionData, () => {
            buildContinued = true;
            BuildPlayerWindow.DefaultBuildMethods.BuildPlayer(options);
        });
    }

    private static VersionData GetVersionDataStatic () {
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

using UnityEngine;
using UnityEditor;
using System;

namespace KulibinSpace.BuildVersion {

    public class VersionWindow : EditorWindow {
        private VersionData versionData;
        private VersionSnapshot originalSnapshot;
        private Vector2 scrollPosition;
        private bool showHistory = false;
        private System.Action onContinue;

        // Стили для подсказок
        private GUIStyle helpBoxStyle;
        private bool stylesInitialized = false;

        public static void ShowWindow (VersionData data, System.Action continueCallback) {
            VersionWindow window = GetWindow<VersionWindow>("Build Version Manager", true);
            window.versionData = data;
            window.originalSnapshot = data.CreateSnapshot(); // Сохраняем исходное состояние
            window.onContinue = continueCallback;
            window.minSize = new Vector2(500, 400);
            window.Show();
        }

        private void InitializeStyles () {
            if (!stylesInitialized) {
                helpBoxStyle = new GUIStyle(EditorStyles.helpBox);
                helpBoxStyle.fontSize = 10;
                helpBoxStyle.wordWrap = true;
                stylesInitialized = true;
            }
        }

        void OnGUI () {
            InitializeStyles();

            if (versionData == null) {
                EditorGUILayout.HelpBox("Version data not found!", MessageType.Error);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // Текущая версия
            EditorGUILayout.LabelField("Current Version", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Version: " + versionData.GetVersionString());
            EditorGUILayout.LabelField("Type: " + versionData.GetReleaseTypeString());
            EditorGUILayout.LabelField("Build: " + versionData.buildNumber.ToString());
            EditorGUILayout.LabelField("Last Build: " + versionData.buildDate);

            EditorGUILayout.Space();

            // Настройки версии с подсказками
            EditorGUILayout.LabelField("Version Settings", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();

            // Major версия
            EditorGUILayout.BeginVertical("box");
            int newMajor = EditorGUILayout.IntField("Major Version", versionData.major);
            EditorGUILayout.LabelField("Критические изменения, несовместимые с предыдущими версиями. При изменении сбрасывает Minor и Maintenance в 0.", helpBoxStyle);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Minor версия
            EditorGUILayout.BeginVertical("box");
            int newMinor = EditorGUILayout.IntField("Minor Version", versionData.minor);
            EditorGUILayout.LabelField("Новые функции, совместимые с предыдущими версиями. При изменении сбрасывает Maintenance в 0.", helpBoxStyle);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Maintenance версия
            EditorGUILayout.BeginVertical("box");
            int newMaintenance = EditorGUILayout.IntField("Maintenance Version", versionData.maintenance);
            EditorGUILayout.LabelField("Исправления ошибок, мелкие улучшения, не влияющие на функциональность.", helpBoxStyle);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Release Type
            EditorGUILayout.BeginVertical("box");
            string[] releaseTypes = { "Alpha (0)", "Beta (1)", "Release Candidate (2)", "Release (3)" };
            int newReleaseType = EditorGUILayout.Popup("Release Type", versionData.releaseType, releaseTypes);
            EditorGUILayout.LabelField("0 = Alpha (ранняя разработка), 1 = Beta (тестирование), 2 = Release Candidate (предрелиз), 3 = Release (публичный выпуск).", helpBoxStyle);
            EditorGUILayout.EndVertical();

            if (EditorGUI.EndChangeCheck()) {
                // Проверяем, изменилась ли мажорная версия
                if (newMajor != versionData.major) {
                    if (EditorUtility.DisplayDialog("Major Version Change",
                        "Изменение мажорной версии сбросит Minor и Maintenance в 0. Продолжить?",
                        "Да", "Нет")) {
                        versionData.major = newMajor;
                        versionData.minor = 0;
                        versionData.maintenance = 0;
                    }
                } else if (newMinor != versionData.minor) {
                    // Проверяем, изменилась ли минорная версия
                    if (newMinor > versionData.minor) {
                        if (EditorUtility.DisplayDialog("Minor Version Change",
                            "Увеличение Minor версии сбросит Maintenance в 0. Продолжить?",
                            "Да", "Нет")) {
                            versionData.minor = newMinor;
                            versionData.maintenance = 0;
                        }
                    } else {
                        versionData.minor = newMinor;
                        versionData.maintenance = newMaintenance;
                    }
                } else {
                    versionData.minor = newMinor;
                    versionData.maintenance = newMaintenance;
                }

                versionData.releaseType = newReleaseType;
                EditorUtility.SetDirty(versionData);
            }

            EditorGUILayout.Space();

            // Кнопки быстрого управления версией
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("+ Maintenance")) {
                versionData.maintenance++;
                EditorUtility.SetDirty(versionData);
            }
            if (GUILayout.Button("+ Minor")) {
                versionData.minor++;
                versionData.maintenance = 0;
                EditorUtility.SetDirty(versionData);
            }
            if (GUILayout.Button("+ Major")) {
                if (EditorUtility.DisplayDialog("Major Version Increment",
                    "Увеличение Major версии сбросит Minor и Maintenance в 0. Продолжить?",
                    "Да", "Нет")) {
                    versionData.major++;
                    versionData.minor = 0;
                    versionData.maintenance = 0;
                    EditorUtility.SetDirty(versionData);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Next Release Type")) {
                versionData.releaseType = Mathf.Min(3, versionData.releaseType + 1);
                EditorUtility.SetDirty(versionData);
            }
            if (GUILayout.Button("Reset to Alpha")) {
                versionData.releaseType = 0;
                EditorUtility.SetDirty(versionData);
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Кнопка сброса к исходным значениям
            GUI.backgroundColor = Color.yellow;
            if (GUILayout.Button("Reset to Original Values", GUILayout.Height(25))) {
                if (EditorUtility.DisplayDialog("Reset Version",
                    "Сбросить все изменения к исходным значениям?",
                    "Да", "Нет")) {
                    versionData.RestoreFromSnapshot(originalSnapshot);
                    EditorUtility.SetDirty(versionData);
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space();

            // История версий
            showHistory = EditorGUILayout.Foldout(showHistory, "Version History");
            if (showHistory && versionData.history != null && versionData.history.Length > 0) {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical("box");
                for (int i = versionData.history.Length - 1; i >= 0; i--) {
                    var entry = versionData.history[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField("v" + entry.version, GUILayout.Width(80));
                    EditorGUILayout.LabelField("Build " + entry.buildNumber.ToString(), GUILayout.Width(80));
                    EditorGUILayout.LabelField(entry.buildDate, GUILayout.Width(120));
                    if (GUILayout.Button("Restore", GUILayout.Width(60))) {
                        RestoreVersion(entry);
                    }
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space();

            // Информационная панель
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Build Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Next Version: " + versionData.GetVersionString());
            EditorGUILayout.LabelField("Next Build Number: " + (versionData.buildNumber + 1).ToString());
            EditorGUILayout.LabelField("Player Settings Version: " + PlayerSettings.bundleVersion);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            // Кнопки действий
            EditorGUILayout.BeginHorizontal();
            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Cancel Build", GUILayout.Height(35))) {
                Close();
            }

            GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Continue Build", GUILayout.Height(35))) {
                UpdateVersionForBuild();
                onContinue?.Invoke();
                Close();
            }
            GUI.backgroundColor = Color.white;
            EditorGUILayout.EndHorizontal();
        }

        private void RestoreVersion (VersionHistoryEntry entry) {
            versionData.major = int.Parse(entry.version.Split('.')[0]);
            versionData.minor = int.Parse(entry.version.Split('.')[1]);
            versionData.maintenance = int.Parse(entry.version.Split('.')[2]);
            versionData.releaseType = 0; // Reset to default release type if needed
            versionData.buildNumber = entry.buildNumber;
            versionData.buildDate = entry.buildDate;
            EditorUtility.SetDirty(versionData);
        }

        private void UpdateVersionForBuild () {
            versionData.buildNumber++;
            versionData.buildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            // Формируем строку версии
            string versionString = versionData.GetVersionString();
#if UNITY_EDITOR
            // Обновляем PlayerSettings.bundleVersion
            UnityEditor.PlayerSettings.bundleVersion = versionString;
            // Сохраняем изменения в asset
            UnityEditor.EditorUtility.SetDirty(versionData);
            UnityEditor.AssetDatabase.SaveAssets();
#endif
        }

    }

}

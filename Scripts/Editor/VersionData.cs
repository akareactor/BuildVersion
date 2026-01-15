using System;
using UnityEngine;

namespace KulibinSpace.BuildVersion {

    [CreateAssetMenu(fileName = "VersionData", menuName = "Build/Version Data")]
    [System.Serializable]
    public class VersionData : ScriptableObject {
        [Header("Version Info")]
        public int major = 0;
        public int minor = 0;
        public int maintenance = 0;
        public int releaseType = 0; // 0-alpha, 1-beta, 2-rc, 3-release

        [Header("Build Info")]
        public int buildNumber = 0;
        public string buildDate = "";

        [Header("Version History")]
        public VersionHistoryEntry[] history = new VersionHistoryEntry[0];

        [Header("System Info")]
        public bool isInitialized = false;

        //public string GetVersionString () { return string.Format("{0}.{1}.{2}.{3}", major, minor, maintenance, releaseType); }
        public string GetVersionString () {
            return string.Format(
                "{0}.{1}.{2}.{3}+{4}",
                major, minor, maintenance, releaseType, buildNumber
            );
        }

        public string GetReleaseTypeString () {
            switch (releaseType) {
                case 0: return "Alpha";
                case 1: return "Beta";
                case 2: return "Release Candidate";
                case 3: return "Release";
                default: return "Unknown";
            }
        }

        public string GetFullVersionString () {
            return string.Format("{0} ({1}) Build {2} - {3}",
                GetVersionString(), GetReleaseTypeString(), buildNumber, buildDate);
        }

        public void InitializeFromPlayerSettings () {
            if (!isInitialized) {
#if UNITY_EDITOR
                ParseVersionFromString(UnityEditor.PlayerSettings.bundleVersion);
                buildDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
                isInitialized = true;
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        public void ParseVersionFromString (string versionString) {
            if (string.IsNullOrEmpty(versionString)) {
                major = 0;
                minor = 1;
                maintenance = 0;
                releaseType = 0;
                buildNumber = 0;
                return;
            }
            // 1. Отделяем buildNumber
            string mainPart = versionString;
            string buildPart = null;
            int plusIndex = versionString.IndexOf('+');
            if (plusIndex >= 0) {
                mainPart = versionString.Substring(0, plusIndex);
                buildPart = versionString.Substring(plusIndex + 1);
            }
            // 2. Парсим основную версию
            string[] parts = mainPart.Split('.');
            int parsedValue;
            if (parts.Length > 0 && int.TryParse(parts[0], out parsedValue)) major = parsedValue;
            if (parts.Length > 1 && int.TryParse(parts[1], out parsedValue)) minor = parsedValue;
            if (parts.Length > 2 && int.TryParse(parts[2], out parsedValue)) maintenance = parsedValue;
            if (parts.Length > 3 && int.TryParse(parts[3], out parsedValue)) releaseType = Mathf.Clamp(parsedValue, 0, 3);
            // 3. Парсим buildNumber (если есть)
            if (!string.IsNullOrEmpty(buildPart) && int.TryParse(buildPart, out parsedValue)) buildNumber = parsedValue;
        }

        public VersionSnapshot CreateSnapshot () {
            return new VersionSnapshot {
                major = this.major,
                minor = this.minor,
                maintenance = this.maintenance,
                releaseType = this.releaseType,
                buildNumber = this.buildNumber,
                buildDate = this.buildDate
            };
        }

        public void RestoreFromSnapshot (VersionSnapshot snapshot) {
            this.major = snapshot.major;
            this.minor = snapshot.minor;
            this.maintenance = snapshot.maintenance;
            this.releaseType = snapshot.releaseType;
            this.buildNumber = snapshot.buildNumber;
            this.buildDate = snapshot.buildDate;
        }
    }

    [System.Serializable]
    public class VersionHistoryEntry {
        public string version;
        public int buildNumber;
        public string buildDate;
        public string timestamp;

        public VersionHistoryEntry (string ver, int build, string date) {
            version = ver;
            buildNumber = build;
            buildDate = date;
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }

    [System.Serializable]
    public class VersionSnapshot {
        public int major;
        public int minor;
        public int maintenance;
        public int releaseType;
        public int buildNumber;
        public string buildDate;
    }

}

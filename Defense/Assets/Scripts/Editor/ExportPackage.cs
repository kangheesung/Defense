using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ExportPackage : MonoBehaviour {
    [MenuItem("Export/Export Packages")]

    public static void export() {
        string[] projectContent = new string[] {
            "Assets",
            "ProjectSettings/DynamicsManager.asset",
            "ProjectSettings/TagManager.asset",
            "ProjectSettings/InputManager.asset",
            "ProjectSettings/ProjectSettings.asset"
        };

        AssetDatabase.ExportPackage(
            projectContent,
            "Defense.unitypackage",
            ExportPackageOptions.Interactive |
            ExportPackageOptions.Recurse |
            ExportPackageOptions.IncludeDependencies
        );

        Debug.Log("Project Exported");
    }
}

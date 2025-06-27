using UnityEditor;
using UnityEngine;
using System.IO;

namespace Burmuruk.Tesis.Editor
{
    [InitializeOnLoad]
    public static class RPGStarterSetup
    {
        static readonly string sourcePath = "Packages/com.Burmuruk.RPG-Starter-Template/GameArchitecture";
        static readonly string targetPath = "Assets/GameArchitecture";

        static RPGStarterSetup()
        {
            if (!Directory.Exists(targetPath))
            {
                if (EditorUtility.DisplayDialog("RPG Starter Template",
                    "¿Deseas copiar los archivos base a tu carpeta Assets/GameArchitecture?",
                    "Sí, copiar", "No"))
                {
                    FileUtil.CopyFileOrDirectoryFollowSymlinks(sourcePath, targetPath);
                    AssetDatabase.Refresh();
                    Debug.Log("RPG Starter Template: GameArchitecture copiado a Assets/");
                }
            }
        }
    }
}

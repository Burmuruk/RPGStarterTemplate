using Burmuruk.RPGStarterTemplate.Control;
using Burmuruk.RPGStarterTemplate.Movement.PathFindig;
using System.IO;
using UnityEngine;

namespace Burmuruk.RPGStarterTemplate.Saving.Samples
{
    public class JsonSavingWrapperSample : JsonSavingWrapper
    {
        protected override void LoadNavigationMap()
        {
#if UNITY_EDITOR
            NavSaver.Restart();
            string assetsSamplePath = Path.Combine(
                Directory.GetParent(Application.dataPath).FullName,
                "Assets/com.burmuruk.rpg-starter-template/Samples/RPGStarterDemo/NavigationMaps"
            );

            if (!Directory.Exists(assetsSamplePath))
            {
                assetsSamplePath = Path.Combine(
                  Directory.GetParent(Application.dataPath).FullName,
                  "Packages/com.burmuruk.rpg-starter-template/Samples~/RPGStarterDemo/NavigationMaps"
                );
            }

            if (!Directory.Exists(assetsSamplePath))
            {
                assetsSamplePath = Path.Combine(
                    Application.dataPath,
                    "Samples/RPGStarterDemo/NavigationMaps"
                );
            }

            if (!Directory.Exists(assetsSamplePath)) return;

            NavSaver.LoadNavMesh(assetsSamplePath);
            FindAnyObjectByType<LevelManager>()?.SetPaths(); 
#endif
        }
    }
}

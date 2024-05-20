using System.Collections;
using UnityEngine;

namespace Burmuruk.Tesis.Saving
{
    public class JsonSavingWrapper : MonoBehaviour
    {
        const string defaultSaveFile = "save";

        private IEnumerator Start()
        {
            yield return GetComponent<JsonSavingSystem>().LoadLastScene(defaultSaveFile);
        }

        public void Save()
        {
            GetComponent<JsonSavingSystem>().Save(defaultSaveFile);
        }

        public void Load()
        {
            GetComponent<JsonSavingSystem>().Load(defaultSaveFile);
        }
    }
}
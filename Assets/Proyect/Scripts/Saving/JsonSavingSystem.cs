using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

namespace Burmuruk.Tesis.Saving
{
    public class JsonSavingSystem : MonoBehaviour
    {
        private const string extension = ".json";

        public event Action onSceneLoaded;

        public IEnumerator LoadLastScene(JObject state, int slot, Action<JObject> callback)
        {
            int curScene = SceneManager.GetActiveScene().buildIndex;
            int buildIndex = 2;
            JObject slotData = null;

            if (state.ContainsKey("SlotData"))
            {
                slotData = (JObject)state["SlotData"];
                if ((int)state["SlotData"]["BuildIdx"] != SceneManager.GetActiveScene().buildIndex)
                {
                    buildIndex = (int)state["SlotData"]["BuildIdx"];
                }
            }
            else
            {
                slotData = new JObject();
                slotData["Slot"] = slot;
                slotData["BuildIdx"] = slot;
                slotData["TimePlayed"] = slot;
            }

            
            yield return SceneManager.LoadSceneAsync(buildIndex);
            
            onSceneLoaded?.Invoke();
            
            RestoreFromToken(state);
            
            callback?.Invoke(slotData);
            //yield return SceneManager.UnloadSceneAsync(curScene);
            //Debug.Log("Scene Unloaded");
        }

        public void Save(string saveFile, JObject slotData = null)
        {
            JObject state = LoadJsonFromFile(saveFile);
            CaptureAsToken(state, slotData);
            SaveFileAsJson(saveFile, state);
        }

        public void Delete (string saveFile)
        {
            File.Delete(GetPathFromSaveFile(saveFile));
        }

        public void Load(string saveFile, int slot, Action<JObject> callback)
        {
            JObject state = LoadJsonFromFile(saveFile);

            StartCoroutine(LoadLastScene(state, slot, callback));

            //RestoreFromToken(LoadJsonFromFile(saveFile));
        }

        private JObject LoadJsonFromFile(string saveFile)
        {
            string path = GetPathFromSaveFile(saveFile);

            if (!File.Exists(path))
            {
                return new JObject();
            }

            using (var textReader = File.OpenText(path))
            {
                using (var reader = new JsonTextReader(textReader))
                {
                    reader.FloatParseHandling = FloatParseHandling.Double;

                    return JObject.Load(reader);
                }
            }
        }

        private void SaveFileAsJson(string saveFile, JObject state)
        {
            string path = GetPathFromSaveFile(saveFile);
            
            using (var textWriter = File.CreateText(path))
            {
                using (var writer = new JsonTextWriter(textWriter))
                {
                    writer.Formatting = Formatting.Indented;
                    state.WriteTo(writer);
                } 
            }
        }

        private void CaptureAsToken(JObject state, JObject slotData)
        {
            IDictionary<string, JToken> stateDict = state;

            if (slotData != null)
            {
                stateDict["SlotData"] = slotData;
            }

            foreach (var saveable in FindObjectsOfType<JsonSaveableEntity>())
            {
                stateDict[saveable.GetUniqueIdentifier()] = saveable.CaptureAsJtoken();
            }
        }

        private void RestoreFromToken(JObject state)
        {
            if (state.Count <= 0) return;

            IDictionary<string, JToken> stateDict = state;

            foreach (var saveable in FindObjectsOfType<JsonSaveableEntity>())
            {
                string id = saveable.GetUniqueIdentifier();

                if (stateDict.ContainsKey(id))
                {
                    saveable.RestoreFromJToken(stateDict[id]);
                }
            }


        }

        private string GetPathFromSaveFile(string saveFile)
        {
            //return Path.Combine( Application.persistentDataPath, saveFile + ".sav");
            return Path.Combine(Application.persistentDataPath, saveFile + extension);
        }

        public void GetFileInfo(int id)
        {

        }

        public bool LookForSlots(string saveFile)
        {
            string path = GetPathFromSaveFile(saveFile);

            return File.Exists(path);
        }
    }
}

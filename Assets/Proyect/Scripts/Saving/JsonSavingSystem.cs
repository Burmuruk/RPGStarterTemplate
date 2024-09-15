using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Text;
using System.Linq;

namespace Burmuruk.Tesis.Saving
{
    public class JsonSavingSystem : MonoBehaviour
    {
        private const string extension = ".json";

        public event Action onSceneLoaded;
        public event Action<int> OnLoadingStateFinished;

        public IEnumerator LoadLastScene(JObject state, int slot, Action<JObject> callback)
        {
            JObject slotState = new JObject();
            int curScene = SceneManager.GetActiveScene().buildIndex;
            int nextScene = 2;
            JObject slotData = null;

            if (state.ContainsKey(slot.ToString()) && (state[slot.ToString()] as JObject).ContainsKey("SlotData"))
            {
                slotState = (JObject)state[slot.ToString()];
                slotData = (JObject)slotState["SlotData"];
                nextScene = (int)slotState["SlotData"]["BuildIdx"];
            }
            else
            {
                slotData = new JObject();
                slotData["Slot"] = slot;
                slotData["BuildIdx"] = nextScene;
                slotData["TimePlayed"] = 0;
                slotData["MembersCount"] = 1;

                slotState["SlotData"] = slotData;
            }

            yield return SceneManager.LoadSceneAsync(nextScene);
            
            onSceneLoaded?.Invoke();

            RestoreFromToken(slotState);
            
            callback?.Invoke(slotData);
            //yield return SceneManager.UnloadSceneAsync(curScene);
            //Debug.Log("Scene Unloaded");
        }

        public void Save(string saveFile, int slot, JObject slotData = null)
        {
            JObject state = LoadJsonFromFile(saveFile);
            CaptureAsToken(ref state, slotData, slot);
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

            //using (Stream stream = new FileStream(path, FileMode.Open))
            //{
            //    //List<byte[]> text = new();
            //    //int result = 0;
            //    //do
            //    //{
            //    //    byte[] buffer = new byte[64];
            //    //    result = stream.Read(buffer, 0, 64); 

            //    //} while (result != 0);

            //    //var total = text.ToArray();

            //    //JObject hi = new JObject(Encrypter.Decrypt());

            //    //return hi;
            //}
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
            
            //using (Stream textWriter = new FileStream (path, FileMode.Create))
            //{
            //    //var buffer = Encoding.UTF8.GetBytes(Encrypter.EncryptString(state));
            //    //textWriter.Write(buffer, 0, buffer.Length);

            //}

            using (var textWriter = File.CreateText(path))
            {
                using (var writer = new JsonTextWriter(textWriter))
                {
                    writer.Formatting = Formatting.Indented;
                    state.WriteTo(writer);
                }
            }
        }

        private void CaptureAsToken(ref JObject state, JObject slotData, int slot)
        {
            IDictionary<string, JToken> stateDict = state;

            //if (!state.ContainsKey(slot.ToString())) return;

            JObject slotState = new();

            if (state.ContainsKey(slot.ToString()))
            {
                slotState = (JObject)stateDict[slot.ToString()];
            }

            slotState["SlotData"] = slotData;

            foreach (var saveable in FindObjectsOfType<JsonSaveableEntity>())
            {
                var idComponents = saveable.CaptureAsJtoken(out JObject UniqueItemns);

                if (idComponents != null)
                    slotState[saveable.GetUniqueIdentifier()] = idComponents;

                if (UniqueItemns == null) continue;

                foreach (var item in UniqueItemns)
                {
                    if (slotState.ContainsKey(item.Key))
                    {
                        foreach (var component in (JObject)item.Value)
                        {
                            slotState[item.Key][component.Key] = component.Value;
                        }
                    }
                    else
                    {
                        JObject newComponents = new JObject();
                        foreach (var component in (JObject)item.Value)
                        {
                            newComponents[component.Key] = component.Value;
                        }

                        slotState[item.Key] = newComponents;
                    }
                }
            }

            stateDict[slot.ToString()] = slotState;
        }

        private void RestoreFromToken(JObject state)
        {
            if (state.Count <= 0) return;

            IDictionary<string, JToken> stateDict = state;

            var saveables = FindObjectsOfType<JsonSaveableEntity>().ToList();

            for (int i = 0; i < Enum.GetValues(typeof(SavingExecution)).Length; i++)
            {
                if (!stateDict.ContainsKey(((SavingExecution)i).ToString()))
                    continue;

                for (int x = 0; x < saveables.Count; x++)
                {
                    string id = saveables[x].GetUniqueIdentifier();

                    saveables[x].RestoreFromJToken(state, (SavingExecution)i);
                    //if (!stateDict.ContainsKey(id))
                    //{
                    //    //saveables.RemoveAt(x);
                    //}
                }

                OnLoadingStateFinished?.Invoke(i);
            }

            for (int i = 0; i < saveables.Count; i++)
            {
                string id = saveables[i].GetUniqueIdentifier();

                if (stateDict.ContainsKey(id))
                {
                    saveables[i].RestoreFromJToken(stateDict[id], SavingExecution.General);
                } 
            }

            OnLoadingStateFinished?.Invoke((int)SavingExecution.General);
        }

        private string GetPathFromSaveFile(string saveFile)
        {
            //return Path.Combine( Application.persistentDataPath, saveFile + ".sav");
            return Path.Combine(Application.persistentDataPath, saveFile + extension);
        }

        public void GetFileInfo(int id)
        {

        }

        public List<(int id, JObject slotData)> LookForSlots(string saveFile)
        {
            var data = LoadJsonFromFile(saveFile);

            IDictionary<string, JToken> stateDict = data;
            List<(int id, JObject slotData)> slots = new();
            int i = 0;

            foreach (var slot in stateDict)
            {
                int id = Int32.Parse(slot.Key);
                slots.Add((id, (JObject)slot.Value["SlotData"]));
            }

            return slots;
        }
    }
}

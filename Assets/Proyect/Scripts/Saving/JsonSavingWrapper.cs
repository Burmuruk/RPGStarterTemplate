using Burmuruk.Tesis.Stats;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UnityEngine;

namespace Burmuruk.Tesis.Saving
{
    public class JsonSavingWrapper : MonoBehaviour
    {
        const string defaultSaveFile = "miGuardado-";
        const string defaultAutoSaveFile = "miAutoGuardado-";

        public event Action<float> OnSaving;
        public event Action<float> OnLoading;
        public event Action onSceneLoaded;
        public event Action<JObject> OnLoaded;

        private void Awake()
        {
            GetComponent<JsonSavingSystem>().onSceneLoaded += onSceneLoaded;
        }

        //private IEnumerator Start()
        //{
        //    yield return GetComponent<JsonSavingSystem>().LoadLastScene(defaultSaveFile);
        //}

        public void Save(int slot, JObject slotData = null)
        {
            OnSaving?.Invoke(0);
            if (slot == 0)
            {
                int id = System.DateTime.Now.Second + System.DateTime.Now.Hour + System.DateTime.Now.Year;
                GetComponent<JsonSavingSystem>().Save(defaultAutoSaveFile + id, slotData);
            }
            else
            {
                GetComponent<JsonSavingSystem>().Save(defaultSaveFile + slot, slotData);
            }
            OnSaving?.Invoke(1);
        }

        public void Load(int slot)
        {
            OnLoading?.Invoke(0);
            FindObjectOfType<BuffsManager>()?.RemoveAllBuffs();

            if (slot < 0)
            {
                GetComponent<JsonSavingSystem>().Load(defaultAutoSaveFile + slot, slot, OnLoaded);
            }
            else
            {
                GetComponent<JsonSavingSystem>().Load(defaultSaveFile + slot, slot, OnLoaded);
            }
            OnLoading?.Invoke(1);
        }

        public void LookForSlots(out int slotsCount, out int autosaveCount)
        {
            slotsCount = 0;
            autosaveCount = 0;
            var saver = GetComponent<JsonSavingSystem>();

            for (int i = 0, j = 0; i < 3; i++, j--)
            {
                if (saver.LookForSlots(defaultSaveFile + (i + 1)))
                    slotsCount++;

                if (saver.LookForSlots(defaultAutoSaveFile + (j -1)))
                    autosaveCount++;
            }
        }
    }
}
using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Stats;
using Burmuruk.Tesis.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Burmuruk.Tesis.Saving
{
    public class JsonSavingWrapper : MonoBehaviour
    {
        const string defaultSaveFile = "miGuardado-";
        const string defaultAutoSaveFile = "miAutoGuardado-";

        private int _lastBuildIdx = 0;

        public event Action<float> OnSaving;
        public event Action<float> OnLoading;
        public event Action<JObject> OnLoaded;
        public UnityEvent OnSavingUI;
        public UnityEvent OnLoadingUI;
        public UnityEvent OnLoadedUI;

        public event Action<int> OnLoadingStateFinished
        {
            add => GetComponent<JsonSavingSystem>().OnLoadingStateFinished += value;
            remove => GetComponent<JsonSavingSystem>().OnLoadingStateFinished -= value;
        }

        private void Awake()
        {
            var saver = GetComponent<JsonSavingSystem>();

            saver.onSceneLoaded += () =>
            {
                Movement.PathFindig.Path.Restart();
                Movement.PathFindig.Path.LoadNavMesh();
                TemporalSaver.RemoveAllData();

                if (_lastBuildIdx == 0)
                {
                    GetComponent<PersistentObjSpawner>().TrySpawnObjects();
                }
            };
            
            OnLoaded += RestoreSlotData;
            OnLoadingStateFinished += LoadStage;

            DontDestroyOnLoad(gameObject);
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
                GetComponent<JsonSavingSystem>().Save(defaultAutoSaveFile + id, -1, slotData);
            }
            else
            {
                GetComponent<JsonSavingSystem>().Save(defaultSaveFile, slot, slotData);
            }

            OnSaving?.Invoke(1);
        }

        public void Load(int slot)
        {
            _lastBuildIdx = SceneManager.GetActiveScene().buildIndex;
            OnLoading?.Invoke(0);
            OnLoadingUI?.Invoke();
            FindObjectOfType<BuffsManager>()?.RemoveAllBuffs();

            //Timer timer = new Timer(500);
            //timer.Elapsed += (obj, args) => LoadWithoutFade(slot);
            //timer.Start();
            Task.Delay(50).GetAwaiter().OnCompleted(() => LoadWithoutFade(slot));

        }

        private void LoadWithoutFade(int slot)
        {
            if (slot < 0)
            {
                GetComponent<JsonSavingSystem>().Load(defaultAutoSaveFile, slot, 
                    (args) => { OnLoaded?.Invoke(args); OnLoadedUI?.Invoke(); });
            }
            else
            {
                GetComponent<JsonSavingSystem>().Load(defaultSaveFile, slot,
                    (args) => { OnLoaded?.Invoke(args); OnLoadedUI?.Invoke(); });
            }
            OnLoading?.Invoke(1);
        }

        public List<(int id, JObject slotData)> LookForSlots()
        {
            var saver = GetComponent<JsonSavingSystem>();

            return saver.LookForSlots(defaultSaveFile);
        }
        
        public List<(int id, JObject slotData)> FindAvailableSlots()
        {
            return LookForSlots();
        }

        private void RestoreSlotData(JObject slotData)
        {
            var data = new SlotData(
                slotData["Slot"].ToObject<int>(),
                slotData["BuildIdx"].ToObject<int>(),
                slotData["TimePlayed"].ToObject<float>());

            FindObjectOfType<LevelManager>().SaveSlotData(data);
        }

        private void LoadStage(int stage)
        {
            switch ((SavingExecution)stage)
            {
                case SavingExecution.Admin:
                    break;

                case SavingExecution.System:
                    
                    break;

                case SavingExecution.Organization:
                    FindObjectOfType<LevelManager>().SetPaths();
                    FindObjectOfType<PlayerManager>().UpdateLeaderPosition();

                    break;

                case SavingExecution.General:

                    if (_lastBuildIdx != 0) break;

                    FindObjectOfType<HUDManager>().Init();
                    break;
            }
        }
    }
}
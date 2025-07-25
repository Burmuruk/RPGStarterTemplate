﻿using Burmuruk.RPGStarterTemplate.Control;
using Burmuruk.RPGStarterTemplate.Stats;
using Burmuruk.RPGStarterTemplate.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Burmuruk.RPGStarterTemplate.Saving
{
    public class JsonSavingWrapper : MonoBehaviour
    {
        const string defaultSaveFile = "miGuardado-";
        const string defaultAutoSaveFile = "miAutoGuardado-";
        const string defaultImageName = "Slot";
        const string defaultImageExtention = ".png";

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
            OnLoading += (_) => { 
                FindObjectOfType<GameManager>()?.SetState(GameManager.State.Loading);
                
            };

            saver.onSceneLoaded += () =>
            {
                Movement.PathFindig.NavSaver.Restart();
                Movement.PathFindig.NavSaver.LoadNavMesh();
                TemporalSaver.RemoveAllData();

                if (_lastBuildIdx == 0)
                {
                    GetComponent<PersistentObjSpawner>().TrySpawnObjects();
                    //FindObjectOfType<LevelManager>().pauseMenu = 
                }
            };

            OnLoaded += (args) =>
            {
                RestoreSlotData(args);
                FindObjectOfType<GameManager>()?.SetState(GameManager.State.Playing);
            };
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

            string path = Path.Combine(Application.persistentDataPath, defaultImageName + slot.ToString() + defaultImageExtention);
            ScreenCapture.CaptureScreenshot(path);

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

        public void DeleteSlot(int idx)
        {
            GetComponent<JsonSavingSystem>().DeleteSlot(defaultSaveFile, idx);

            string path = Path.Combine(Application.persistentDataPath, defaultImageName + idx.ToString() + defaultImageExtention);
            File.Delete(path);
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

        public List<(int id, JObject slotData)> FindAvailableSlots(out List<(int id, Sprite sprite)> images)
        {
            images = null;
            var saver = GetComponent<JsonSavingSystem>();

            var slots = saver.LookForSlots(defaultSaveFile);

            if (slots is null) return null;

            foreach (var slot in slots)
            {
                if (TryLoadSlotImage(slot.id, out Sprite newSprite))
                {
                    (images ??= new()).Add((slot.id, newSprite));
                }
            }

            return slots;
        }

        private bool TryLoadSlotImage(int slot, out Sprite sprite)
        {
            sprite = null;
            string path = Path.Combine(Application.persistentDataPath, defaultImageName + slot.ToString() + defaultImageExtention);

            if (!File.Exists(path))
                return false;

            byte[] data = File.ReadAllBytes(path);

            Texture2D tex = new Texture2D(2, 2);
            ImageConversion.LoadImage(tex, data);

            sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(.5f, .5f));
            return true;
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

                    //if (_lastBuildIdx == 0) break;

                    FindObjectOfType<HUDManager>().Init();
                    break;
            }
        }
    }
}
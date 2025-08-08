using Burmuruk.RPGStarterTemplate.Control;
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
        const string DEFAULT_SAVEFILE = "miGuardado-";
        //const string DEFAULT_AUTOSAVE_FILE = "miAutoGuardado-";
        const string DEFAULT_IMAGE_NAME = "Slot";
        const string DEFAULT_IMAGE_EXTENTION = ".png";

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
            OnLoading += (_) =>
            {
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
                GetComponent<JsonSavingSystem>().Save(DEFAULT_SAVEFILE/* + id*/, -1, slotData);
            }
            else
            {
                GetComponent<JsonSavingSystem>().Save(DEFAULT_SAVEFILE, slot, slotData);
            }

            TakeSlotPicture(slot);

            OnSaving?.Invoke(1);
        }

        private void TakeSlotPicture(int slot)
        {
            string path = Path.Combine(Application.persistentDataPath, DEFAULT_IMAGE_NAME + slot.ToString() + DEFAULT_IMAGE_EXTENTION);
            ScreenCapture.CaptureScreenshot(path);
        }

        private void CopySlotPicture(int oldSlot, int newSlot)
        {
            string oldPath = Path.Combine(Application.persistentDataPath, DEFAULT_IMAGE_NAME + oldSlot.ToString() + DEFAULT_IMAGE_EXTENTION);
            string newPath = Path.Combine(Application.persistentDataPath, DEFAULT_IMAGE_NAME + newSlot.ToString() + DEFAULT_IMAGE_EXTENTION);

            Task.Delay(100).GetAwaiter().OnCompleted(() => CreatePictureCopy(oldPath, newPath));
        }

        private void CreatePictureCopy(string oldPath, string newPath)
        {
            if (File.Exists(oldPath))
            {
                File.Copy(oldPath, newPath, true);
            }
        }

        private void DeleteSlotPicture(int slot)
        {
            string path = Path.Combine(Application.persistentDataPath, DEFAULT_IMAGE_NAME + slot.ToString() + DEFAULT_IMAGE_EXTENTION);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        private void RenameSlotPicture(int lastSlot, int newSlot)
        {
            string oldPath = Path.Combine(Application.persistentDataPath, DEFAULT_IMAGE_NAME + lastSlot.ToString() + DEFAULT_IMAGE_EXTENTION);
            string newPath = Path.Combine(Application.persistentDataPath, DEFAULT_IMAGE_NAME + newSlot.ToString() + DEFAULT_IMAGE_EXTENTION);

            if (File.Exists(oldPath))
            {
                File.Move(oldPath, newPath);
            }
        }

        /// <summary>
        /// Loads the game at the specified index. If there's no index saved, a new slot is created.
        /// </summary>
        /// <param name="slot">Use positive numbers for manual saving and negative for auto saving.</param>
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
            GetComponent<JsonSavingSystem>().DeleteSlot(DEFAULT_SAVEFILE, idx);

            string path = Path.Combine(Application.persistentDataPath, DEFAULT_IMAGE_NAME + idx.ToString() + DEFAULT_IMAGE_EXTENTION);
            File.Delete(path);
        }

        private void LoadWithoutFade(int slot)
        {
            //string fileName = slot < 0 ? DEFAULT_SAVEFILE : DEFAULT_AUTOSAVE_FILE;
            //GetComponent<JsonSavingSystem>().Load(fileName, slot,
            //    (args) => { OnLoaded?.Invoke(args); OnLoadedUI?.Invoke(); });

            GetComponent<JsonSavingSystem>().Load(DEFAULT_SAVEFILE, slot,
                (args) => { OnLoaded?.Invoke(args); OnLoadedUI?.Invoke(); });

            OnLoading?.Invoke(1);
        }

        /// <summary>
        /// Creates a new auto-save slot with -1 as it's index.
        /// </summary>
        public void AddNewAutoSaveSlot(JObject slotData, bool overrideManualSave)
        {
            OnSaving?.Invoke(0);
            var slots = FindAvailableSlots(out _);

            var saver = GetComponent<JsonSavingSystem>();
            var data = saver.LoadSave(DEFAULT_SAVEFILE);
            var newSave = new JObject();

            for (int i = -3; i < 4; i++)
            {
                if (!data.ContainsKey(i.ToString())) continue;

                if (i > 0)
                {
                    newSave[i.ToString()] = data[i.ToString()];
                    continue; // Skip if it's not an auto-save slot
                }
                else if (i - 1 < -3)
                {
                    DeleteSlotPicture(i);
                    continue; // Removes old auto-save slots
                }

                newSave[(i - 1).ToString()] = data[i.ToString()];
                RenameSlotPicture(i, i - 1);
            }

            string slotIdx = slotData["Slot"].ToObject<string>();
            var curData = saver.LoadCurrentSlot(DEFAULT_SAVEFILE, slotData);
            newSave[(-1).ToString()] = curData[slotIdx];
            TakeSlotPicture(-1);

            if (overrideManualSave)
            {
                CopySlotPicture(-1, slotData["Slot"].ToObject<int>());
                newSave[slotIdx] = curData[slotIdx];
            }

            saver.OverwriteSave(DEFAULT_SAVEFILE, newSave);
            OnSaving?.Invoke(1);
        }

        public List<(int id, JObject slotData)> FindAvailableSlots(out List<(int id, Sprite sprite)> images)
        {
            images = null;
            var saver = GetComponent<JsonSavingSystem>();

            var slots = saver.LookForSlots(DEFAULT_SAVEFILE);
            //if (includeAutoSaves)
            //    slots.AddRange(saver.LookForSlots(DEFAULT_AUTOSAVE_FILE));

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
            string path = Path.Combine(Application.persistentDataPath, DEFAULT_IMAGE_NAME + slot.ToString() + DEFAULT_IMAGE_EXTENTION);

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
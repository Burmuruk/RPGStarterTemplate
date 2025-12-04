using Burmuruk.RPGStarterTemplate.Control.AI;
using Burmuruk.RPGStarterTemplate.Interaction;
using Burmuruk.RPGStarterTemplate.Movement.PathFindig;
using Burmuruk.RPGStarterTemplate.Saving;
using Burmuruk.RPGStarterTemplate.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Burmuruk.RPGStarterTemplate.Control
{
    public class LevelManager : MonoBehaviour, ISlotDataProvider, IslotDataSaver
    {
        [SerializeField] UnityEvent onUILoaded;
        [SerializeField] UnityEvent onUIUnLoaded;
        [SerializeField] public GameObject pauseMenu;
        protected JsonSavingWrapper savingWrapper;

        protected GameManager gameManager;
        protected UIMenuCharacters menuCharacters;
        protected PlayerManager playerManager;

        List<GameObject> itemsToDestroy = new();

        private int slotIdx = 1;
        private bool initialized = false;

        //public static List<Coroutine> activeCoroutines = new();
        public event Action OnNavmeshLoaded;

        private void Awake()
        {
            savingWrapper = FindObjectOfType<JsonSavingWrapper>();
            //playerManager.OnPlayerAdded += SetPathToCharacter;

            NavSaver.Restart();
            NavSaver.LoadNavMesh();
            FindObjectOfType<PickupSpawner>().RegisterCurrentItems();
        }

        void Start()
        {
            playerManager = FindObjectOfType<PlayerManager>();
            //AddItemToDestroy(playerManager.PlayersParent);
            AddItemToDestroy(FindObjectOfType<SavingUI>().gameObject);

            gameManager = GetComponent<GameManager>();
            gameManager.onStateChange += UpdateGameState;
            OnNavmeshLoaded += () =>
            {
                gameManager.NotifyLevelLoaded();
                Resume();
            };

            StartCoroutine(Autosave());
            DontDestroyOnLoad(gameObject);

            NavSaver.Restart();
            NavSaver.LoadNavMesh();
            FindAnyObjectByType<LevelManager>().SetPaths();
            UpdatePlayerPosition();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += VerifyScene;
            SceneManager.sceneUnloaded += RestoreScene;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= VerifyScene;
            SceneManager.sceneUnloaded -= RestoreScene;
        }

        private void OnLevelWasLoaded(int level)
        {
            NavSaver.Restart();
            NavSaver.LoadNavMesh();
            FindAnyObjectByType<LevelManager>().SetPaths();
            UpdatePlayerPosition();
        }

        public void Update()
        {
            if (Input.GetKeyUp(KeyCode.K))
            {
                var data = CaptureLevelData();

                savingWrapper.Save(data["Slot"].ToObject<int>(), data);
            }

            if (Input.GetKeyUp(KeyCode.L))
            {


                TemporalSaver.RemoveAllData();
                savingWrapper.Load(GetSlotData().Id);
            }
        }

        public void SetPaths()
        {
            if (NavSaver.NodeList == null) return;

            var movers = FindObjectsOfType<Movement.Movement>(true);

            foreach (var mover in movers)
            {
                mover.SetConnections(NavSaver.NodeList);
            }

            OnNavmeshLoaded?.Invoke();
        }

        public void SetPathToPlayer(Character character)
        {
            if (NavSaver.NodeList == null) return;

            character.mover.SetConnections(NavSaver.NodeList);
        }

        public void UpdatePlayerPosition()
        {
            var playerSpawner = FindObjectOfType<PlayerSpawner>();
            var mainPlayer = FindObjectOfType<AIGuildMember>(true).Leader;

            if (playerSpawner && playerSpawner.Enabled)
            {
                mainPlayer.SetPosition(playerSpawner.transform.position);
            }
        }

        public void AddItemToDestroy(GameObject item)
        {
            itemsToDestroy.Add(item);
        }

        public void GoToMainMenu()
        {
            FindObjectOfType<Fader>().FadeIn();
            savingWrapper.AddNewAutoSaveSlot(CaptureLevelData(), true);

            itemsToDestroy.ForEach(obj => Destroy(obj));
            gameManager.GoToMainMenu();
            Destroy(gameObject.transform.parent.gameObject);
        }

        public void ExitGame()
        {
            savingWrapper.AddNewAutoSaveSlot(CaptureLevelData(), true);
            gameManager.ExitGame();
        }

        private void VerifyScene(Scene scene, LoadSceneMode mode)
        {
            if (scene.buildIndex == 1)
            {
                onUILoaded?.Invoke();
                Time.timeScale = 0;
                SceneManager.SetActiveScene(scene);
                var rootItems = SceneManager.GetSceneByBuildIndex(1).GetRootGameObjects();
                FindObjectOfType<LevelManager>().
                GetComponentInChildren<Camera>().gameObject.SetActive(false);
                var uiController = FindObjectOfType<UICharactersController>();

                foreach (var item in rootItems)
                {
                    menuCharacters = item.GetComponentInChildren<UIMenuCharacters>();

                    if (menuCharacters != null)
                    {
                        var pm = FindObjectOfType<PlayerManager>();
                        menuCharacters.SetPlayers(pm.Players);
                        menuCharacters.SetInventory(pm.MainInventory);
                        menuCharacters.SetPlayerManager(pm);

                        menuCharacters.OnMainPlayerChanged += playerManager.SetPlayerControl;
                        uiController.menuCharacters = menuCharacters;
                        uiController.gameManager = gameManager;
                        break;
                    }
                }
            }
        }

        private void RestoreScene(Scene scene)
        {
            if (scene.buildIndex == 1)
            {
                onUIUnLoaded?.Invoke();
                Time.timeScale = 1;
            }
        }

        public void ToggleSavingOptions()
        {
            FindObjectOfType<SavingUI>().ToggleSlots();
        }

        public void HideSavingOptions()
        {
            FindObjectOfType<SavingUI>().ShowSlots(false);
        }

        public void Pause()
        {
            if (gameManager.GameState == GameManager.State.Pause)
            {
                if (gameManager.Continue())
                {
                    pauseMenu.gameObject.SetActive(false);
                    Time.timeScale = 1;
                    HideSavingOptions();
                }
            }
            else if (gameManager.GameState == GameManager.State.Playing)
            {
                if (gameManager.PauseGame())
                {
                    pauseMenu.gameObject.SetActive(true);
                    Time.timeScale = 0;
                }
            }
        }

        public void Die()
        {
            Task.Delay(1000).GetAwaiter().OnCompleted(LoadLastPoint);
        }

        private void LoadLastPoint()
        {
            var slots = savingWrapper.FindAvailableSlots(out _);
            (int idx, float time) max = (0, float.MinValue);

            foreach (var slot in slots)
            {
                if (slot.id == -1 || slot.id == 1)
                {
                    if (slot.slotData["TimePlayed"].ToObject<float>() is var t && t > max.time)
                        max = (slot.id, t);
                }
            }

            savingWrapper.Load(max.idx == 0 ? slotIdx : max.idx);
        }

        public void Resume()
        {
            if (gameManager.Continue())
            {
                pauseMenu.gameObject.SetActive(false);
                Time.timeScale = 1;

                HideSavingOptions();
            }
            else if (pauseMenu.gameObject.activeSelf)
            {
                pauseMenu.gameObject.SetActive(false);
                Time.timeScale = 1;

                HideSavingOptions();
            }

            if (gameManager.GameState == GameManager.State.Pause)
            {
            }
        }

        public void ExitUI()
        {
            if (menuCharacters.curState != UIMenuCharacters.State.None) return;

            menuCharacters.UnloadMenu();
            GetComponentInChildren<Camera>(true).gameObject.SetActive(true);

            gameManager.ExitUI();
            Task.Delay(200).GetAwaiter().OnCompleted(() => savingWrapper.AddNewAutoSaveSlot(CaptureLevelData(), false));
        }

        public void RestoreFromJToken(JToken state)
        {
            //SceneManager.LoadScene(state.)
            slotIdx = state["Slot"].ToObject<int>();
        }

        public SlotData GetSlotData()
        {
            SlotData slotData = new SlotData(
                slotIdx,
                SceneManager.GetActiveScene().buildIndex,
                Time.realtimeSinceStartup);

            return slotData;
        }

        public void SaveSlotData(SlotData slotData)
        {
            slotIdx = slotData.Id;
        }

        private void UpdateGameState(GameManager.State state)
        {
            switch (state)
            {
                case GameManager.State.Playing:
                    FindObjectOfType<HUDManager>(true).gameObject.SetActive(true);
                    break;
                case GameManager.State.Pause:
                    break;
                case GameManager.State.UI:
                    FindObjectOfType<HUDManager>().gameObject.SetActive(false);
                    break;
                case GameManager.State.Loading:
                    break;
                case GameManager.State.Cinematic:
                    break;
                default:
                    break;
            }
        }

        public JObject CaptureLevelData()
        {
            var slotData = GetSlotData();

            JObject data = new JObject();
            data["Slot"] = slotData.Id;
            data["BuildIdx"] = slotData.BuildIdx;
            data["TimePlayed"] = slotData.PlayedTime;
            data["MembersCount"] = FindObjectOfType<PlayerManager>().Players.Count;

            return data;
        }

        IEnumerator Autosave()
        {
            yield break;
            //while (true)
            //{
            //    yield return new WaitForSeconds(600);

            //    while (gameManager.GameState != GameManager.State.Playing)
            //    {
            //        yield return new WaitForSeconds(60);
            //    }

            //    var data = CaptureLevelData();
            //    savingWrapper.Save(data["Slot"].ToObject<int>(), data);
            //    FindObjectOfType<JsonSavingWrapper>().Save(0, data);
            //}
        }
    }
}

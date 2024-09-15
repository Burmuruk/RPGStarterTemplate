using Burmuruk.Tesis.Movement.PathFindig;
using Burmuruk.Tesis.Saving;
using Burmuruk.Tesis.UI;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using UnityEditor.Media;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Burmuruk.Tesis.Control
{
    public class LevelManager : MonoBehaviour, ISlotDataProvider, IslotDataSaver
    {
        [SerializeField] Path path;
        [SerializeField] UnityEvent onUILoaded;
        [SerializeField] UnityEvent onUIUnLoaded;
        [SerializeField] GameObject pauseMenu;
        JsonSavingWrapper savingWrapper;

        GameManager gameManager;
        UIMenuCharacters menuCharacters;
        PlayerManager playerManager;

        private int slotIdx = 1;
        private bool initialized = false;

        public event Action OnNavmeshLoaded;

        private void Awake()
        {
            savingWrapper = FindObjectOfType<JsonSavingWrapper>();
            savingWrapper.onSceneLoaded += LoadPaths;
            savingWrapper.OnLoadingStateFinished += LoadStage;
            playerManager = FindObjectOfType<PlayerManager>();
            //playerManager.OnPlayerAdded += SetPathToCharacter;
        }

        void Start()
        {
            gameManager = GetComponent<GameManager>();
            SceneManager.sceneLoaded += VerifyScene;
            SceneManager.sceneUnloaded += RestoreScene;

            gameManager.onStateChange += UpdateGameState;
            OnNavmeshLoaded += gameManager.NotifyLevelLoaded;

            StartCoroutine(Autosave());
            DontDestroyOnLoad(gameObject);
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
                //ScreenCapture.)
                TemporalSaver.RemoveAllData();
                savingWrapper.Load(GetSlotData().Id);
            }
        }

        private void OnLevelWasLoaded()
        {
            path.LoadNavMesh();
            SetPaths();

            playerManager.UpdateLeaderPosition();
        }

        public void LoadPaths()
        {
            path.LoadNavMesh();
            print("path loaded");
        }

        public void SetPaths()
        {
            if (path.NodeList == null) return;

            //print("Valor encontrado");
            var movers = FindObjectsOfType<Movement.Movement>(true);

            foreach (var mover in movers)
            {
                mover.SetConnections(path.NodeList);
            }

            OnNavmeshLoaded?.Invoke();
        }

        public void SetPathToCharacter(Character character)
        {
            character.mover.SetConnections(path.NodeList);
        }
        //public INodeListSupplier SetNodeList()
        //{
        //    if (path == null && !path.Saved) return null;

        //    return path.SetNodeList();
        //}

        public void GoToMainMenu()
        {
            //SaveGame(slotIdx);
            gameManager.GoToMainMenu();
        }

        public void ExitGame()
        {
            //SaveGame(slotIdx);
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
                GetComponentInChildren<Camera>().gameObject.SetActive(false);

                foreach (var item in rootItems)
                {
                    menuCharacters = item.GetComponentInChildren<UIMenuCharacters>();

                    if (menuCharacters != null)
                    {
                        var pm = FindObjectOfType<PlayerManager>();
                        menuCharacters.SetPlayers(pm.Players);
                        menuCharacters.SetInventory(pm.MainInventory);
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

        public void RotatePlayer(Vector2 direction)
        {
            if (!menuCharacters) return;

            menuCharacters.RotatePlayer(direction);
        }

        public void ShowMoreOptions()
        {
            if (!menuCharacters) return;

            menuCharacters.SwitchExtraData();
        }

        public void Pause()
        {
            if (gameManager.GameState == GameManager.State.Pause)
            {
                if (gameManager.Continue())
                {
                    pauseMenu.gameObject.SetActive(false);
                    Time.timeScale = 1;
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

        public void ExitUI()
        {
            if (menuCharacters.curState != UIMenuCharacters.State.None) return;

            menuCharacters.UnloadMenu();
            GetComponentInChildren<Camera>(true).gameObject.SetActive(true);

            gameManager.ExitUI();
        }

        public void Remove()
        {
            if (!menuCharacters) return;

            menuCharacters.TryRemoveItem();
        }

        public void ChangeMenu()
        {
            if (!menuCharacters) return;

            menuCharacters.ChangeMenu();
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

                    break;
                case GameManager.State.Pause:
                    break;
                case GameManager.State.UI:
                    break;
                case GameManager.State.Loading:
                    break;
                case GameManager.State.Cinematic:
                    break;
                default:
                    break;
            }
        }

        IEnumerator Autosave()
        {
            while (true)
            {
                yield return new WaitForSeconds(600);

                while (gameManager.GameState != GameManager.State.Playing)
                {
                    yield return new WaitForSeconds(60);
                }

                var data = CaptureLevelData();
                FindObjectOfType<JsonSavingWrapper>().Save(0, data);
            }
        }

        private JObject CaptureLevelData()
        {
            var slotData = GetSlotData();

            JObject data = new JObject();
            data["Slot"] = slotData.Id;
            data["BuildIdx"] = slotData.BuildIdx;
            data["TimePlayed"] = slotData.PlayedTime;
            data["MembersCount"] = FindObjectOfType<PlayerManager>().Players.Count;

            return data;
        }

        private void LoadStage(int stage)
        {
            switch ((SavingExecution)stage)
            {
                case SavingExecution.System:
                    TemporalSaver.RemoveAllData();
                    break;
                case SavingExecution.General:
                    print("path setted");
                    FindObjectOfType<LevelManager>().SetPaths();
                    break;
            }
        }
    } 
}

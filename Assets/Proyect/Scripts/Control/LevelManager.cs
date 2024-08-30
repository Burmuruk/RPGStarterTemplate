using Burmuruk.Tesis.Movement.PathFindig;
using Burmuruk.Tesis.UI;
using Burmuruk.WorldG.Patrol;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace Burmuruk.Tesis.Control
{
    public class LevelManager : MonoBehaviour
    {
        [SerializeField] Path path;
        [SerializeField] UnityEvent onUILoaded;
        [SerializeField] UnityEvent onUIUnLoaded;
        [SerializeField] GameObject pauseMenu;

        GameManager gameManager;
        UIMenuCharacters menuCharacters;

        private int saveFileIdx = 0;
        private bool initialized = false;

        private void Awake()
        {

        }

        void Start()
        {
            gameManager = GetComponent<GameManager>();
            SceneManager.sceneLoaded += VerifyScene;
            SceneManager.sceneUnloaded += RestoreScene;
        }

        void Update()
        {
            SetPaths();
        }

        private void SetPaths()
        {
            if (initialized) return;

            if (path.Saved && path.Loaded)
            {
                if (path.NodeList == null) return;

                //print("Valor encontrado");
                var movers = FindObjectsOfType<Movement.Movement>(true);

                foreach (var mover in movers)
                {
                    mover.SetConnections(path.NodeList);
                }

                initialized = true;
            }
            else
            {
                path.LoadNavMesh();
            }
        }


        //public INodeListSupplier SetNodeList()
        //{
        //    if (path == null && !path.Saved) return null;

        //    return path.SetNodeList();
        //}

        public void LoadGame(int idx)
        {

        }

        public void SaveGame(int idx)
        {

        }

        public void GoToMainMenu()
        {
            SaveGame(saveFileIdx);
            gameManager.GoToMainMenu();
        }

        public void ExitGame()
        {
            SaveGame(saveFileIdx);
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
    } 
}

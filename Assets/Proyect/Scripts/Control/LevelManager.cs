using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Movement;
using Burmuruk.Tesis.Movement.PathFindig;
using Burmuruk.Tesis.Stats;
using Burmuruk.Tesis.UI;
using Burmuruk.WorldG.Patrol;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] Path path;
    [SerializeField] UnityEvent onUILoaded;
    [SerializeField] UnityEvent onUIUnLoaded;

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

        if (path.Loaded && path.m_nodeList != null)
        {
            //print("Valor encontrado");
            var movers = FindObjectsOfType<Movement>();

            foreach (var mover in movers)
            {
                mover.SetConnections(path.m_nodeList);
            }

            initialized = true;
        }
    }


    public INodeListSupplier GetNodeList()
    {
        if (path == null && !path.Loaded) return null;

        return path.GetNodeList();
    }

    public void PauseGame(bool shouldPause)
    {
        Time.timeScale = shouldPause ? 0 : 1;
    }

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
            //SceneManager.SetActiveScene(scene);
            var rootItems = SceneManager.GetSceneByBuildIndex(1).GetRootGameObjects();

            foreach (var item in rootItems)
            {
                menuCharacters = item.GetComponentInChildren<UIMenuCharacters>();

                if (menuCharacters != null)
                {
                    var cloths = FindObjectOfType<PlayerCustomizationManager>();
                    var pm = FindObjectOfType<PlayerManager>();
                    menuCharacters.SetPlayers(pm.Players, cloths);
                    menuCharacters.SetInventary(pm.MainInventary);
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
}

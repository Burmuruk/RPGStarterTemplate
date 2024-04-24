using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Burmuruk.Tesis.Control
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        State state;
        PlayerInput playerInput;

        public State GameState { get { return state; } }

        public enum State
        {
            Playing,
            Pause,
            UI,
            Loading,
            Cinematic
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public bool EnableUI(bool shouldEnable)
        {
            if (state != State.Playing)
                return false;

            state = State.UI;
            //playerInput.SwitchCurrentActionMap("UI");

            return true;
        }

        public bool FinishLevel()
        {
            return true;
        }

        public bool LoadFile(int idx)
        {
            if (state != State.UI) return false;

            state = State.Loading;

            return true;
        }

        public void GoToMainMenu()
        {
            SceneManager.LoadScene(0);
        }

        public void ChangeScene(int idx)
        {
            SceneManager.LoadScene(idx);
        }

        public void ExitGame()
        {
            Application.Quit();
        }

        public void StartGame()
        {
            SceneManager.LoadScene(2);
        }

        public void StartCinematic()
        {
            state = State.Cinematic;
        }

        public bool ShowCharactersMenu()
        {
            if (state != State.Playing)
                return false;

            state = State.UI;

            return true;
        }

        public void ExitUI()
        {
            if (state != State.UI) return;

            if (SceneManager.GetSceneByBuildIndex(1).isLoaded)
            {
                SceneManager.UnloadSceneAsync(1);
            }
            else
            {

            }

            //playerInput.SwitchCurrentActionMap("Player");
            state = State.Playing;
        }
    }
}

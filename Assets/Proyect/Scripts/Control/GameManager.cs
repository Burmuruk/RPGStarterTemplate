using System;
using System.Collections;
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

        public event Action<State> onStateChange;

        public State GameState
        {
            get => state;
            private set
            {
                if (state != value)
                    onStateChange?.Invoke(value);

                state = value;
            }
        }

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
            if (GameState != State.Playing)
                return false;

            GameState = State.UI;
            playerInput.SwitchCurrentActionMap("UI");

            return true;
        }

        public bool FinishLevel()
        {
            return true;
        }

        public bool SetState(State GameState)
        {
            this.GameState = GameState;
            return true;
        }

        public void GoToMainMenu()
        {
            SceneManager.LoadScene(0);
            Time.timeScale = 1;
        }

        public bool Continue()
        {
            if (GameState != State.Pause) return false;

            GameState = State.Playing;
            return true;
        }

        public bool PauseGame()
        {
            if (GameState != State.Playing) return false;

            GameState = State.Pause;

            return true;
        }

        public void ChangeScene(int idx)
        {
            GameState = State.Loading;
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
            GameState = State.Cinematic;
        }

        public bool ShowCharactersMenu()
        {
            if (GameState != State.Playing)
                return false;

            GameState = State.UI;

            return true;
        }

        public void ExitUI()
        {
            if (GameState != State.UI) return;

            if (SceneManager.GetSceneByBuildIndex(1).isLoaded)
            {
                SceneManager.UnloadSceneAsync(1);
            }
            else
            {

            }

            //playerInput.SwitchCurrentActionMap("Player");
            GameState = State.Playing;
        }

        public void NotifyLevelLoaded()
        {
            if (GameState == State.Loading)
                GameState = State.Playing;
        }
    }
}

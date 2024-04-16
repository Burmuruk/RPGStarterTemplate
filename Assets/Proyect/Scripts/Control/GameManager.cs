using UnityEngine;

namespace Burmuruk.Tesis.Control
{
    public class GameManager : MonoBehaviour
    {
        public GameManager Instance;
        State state;

        enum State
        {
            Playing,
            Pause,
            UI,
            Loading,
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
    }
}

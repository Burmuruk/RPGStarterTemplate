using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Stats;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Burmuruk.Tesis.Interaction
{
    public class CheckPoint : MonoBehaviour, IInteractable
    {
        GameManager gameManager;

        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
        }

        public void Interact()
        {
            if (!gameManager.ShowCharactersMenu()) return;

            SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        }
    }
}

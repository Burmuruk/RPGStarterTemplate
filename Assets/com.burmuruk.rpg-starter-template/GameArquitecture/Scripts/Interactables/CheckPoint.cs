using Burmuruk.RPGStarterTemplate.Control;
using Burmuruk.RPGStarterTemplate.Stats;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Burmuruk.RPGStarterTemplate.Interaction
{
    public class CheckPoint : MonoBehaviour, IInteractable
    {
        GameManager gameManager;
        LevelManager levelManager;

        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            levelManager = FindObjectOfType<LevelManager>();
        }

        public void Interact()
        {
            if (!gameManager.ShowCharactersMenu()) return;

            levelManager.ChangeMenu();
            SceneManager.LoadSceneAsync(1, LoadSceneMode.Additive);
        }
    }
}

using Burmuruk.RPGStarterTemplate.Control;
using UnityEngine;

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

        public void Interact(Character character)
        {
            if (!gameManager.ShowCharactersMenu()) return;

            levelManager.ChangeMenu();
        }
    }
}

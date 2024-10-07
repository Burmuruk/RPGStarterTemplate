using Burmuruk.Tesis.Control;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Burmuruk.Tesis.UI
{
    public class UICharactersController : MonoBehaviour
    {
        UIMenuCharacters menuCharacters;

        private void Start()
        {
            menuCharacters = FindObjectOfType<UIMenuCharacters>();
        }

        public void RotatePlayer(InputAction.CallbackContext context)
        {
            print("switched action map");
            if (GameManager.Instance.GameState != GameManager.State.UI)
                return;

            var value = context.ReadValue<Vector2>();

            menuCharacters.RotatePlayer(value);
        }
    }
}

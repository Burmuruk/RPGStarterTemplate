using UnityEngine;
using UnityEngine.InputSystem;

namespace Burmuruk.Tesis.Control
{
    class PlayerController : Character
    {
        public void Move(InputAction.CallbackContext context)
        {
            var dir = context.ReadValue<Vector2>();

            m_mover.MoveTo(dir);
        }
    }
}

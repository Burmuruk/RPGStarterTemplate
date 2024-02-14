using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

namespace Burmuruk.Tesis.Control
{
    class PlayerController : Character
    {
        bool m_isMoving = false;
        Vector3 m_direction = default;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (m_isMoving)
            {
                m_mover.MoveTo(transform.position + m_direction * 10);
            }
        }

        public void Move(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                print("Run");
                var dir = context.ReadValue<Vector2>();
                if (dir.magnitude <= 0)
                {
                    m_isMoving = false;
                    return;
                }

                m_direction = new Vector3(dir.x, 0, dir.y).normalized;
                m_isMoving = true;
            }
            else
            {
                m_direction = Vector3.zero;
                m_isMoving =false;
            }
        }
    }
}

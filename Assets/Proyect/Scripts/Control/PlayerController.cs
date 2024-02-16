using UnityEngine;
using UnityEngine.InputSystem;

namespace Burmuruk.Tesis.Control
{
    class PlayerController : Character
    {
        bool m_shouldMove = false;
        Vector3 m_direction = default;
        Collider m_target;

        protected override void FixedUpdate()
        {
            base.FixedUpdate();

            if (m_shouldMove)
            {
                m_mover.MoveTo(transform.position + m_direction * 10);
            }
        }

        protected override void DecisionManager()
        {
            base.DecisionManager();
        }

        public void Move(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                print("Run");
                var dir = context.ReadValue<Vector2>();
                if (dir.magnitude <= 0)
                {
                    m_shouldMove = false;
                    return;
                }

                m_direction = new Vector3(dir.x, 0, dir.y).normalized;
                m_shouldMove = true;
            }
            else
            {
                m_direction = Vector3.zero;
                m_shouldMove =false;
            }
        }

        public void SelectTarget(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                print("Right Click");
                var enemy = DetectEnemyInMouse();

                if (enemy)
                {
                    print(enemy.name);
                    m_target = enemy;
                    m_fighter.SetTarget(m_target.transform);
                }
            }
        }

        private Collider DetectEnemyInMouse()
        {
            Ray ray = GetRayFromMouseToWorld();
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 200, 1 << 10))
            {
                return hit.collider;
            }

            return null;

            Ray GetRayFromMouseToWorld()
            {
                Vector3 mousePos = Mouse.current.position.ReadValue();
                var cam = Camera.main;
                
                Vector3 screenPos = new(mousePos.x, cam.pixelHeight - mousePos.y, cam.nearClipPlane);

                return cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            }
        }
    }
}

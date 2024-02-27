using Burmuruk.Tesis.Stats;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Burmuruk.Tesis.Control
{
    class PlayerController : MonoBehaviour
    {
        bool m_shouldMove = false;
        Vector3 m_direction = default;
        Collider m_target;
        Character player;

        void FixedUpdate()
        {
            if (m_shouldMove && player)
            {
                player.mover.MoveTo(transform.position + m_direction * 10);
            }

            DetectItems();
        }

        public void SetPlayer(Character player)
        {
            var vollider = player.GetComponent<CapsuleCollider>();
            this.player = player;
        }

        public void Move(InputAction.CallbackContext context)
        {
            if (!player) return;

            if (context.performed)
            {
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
            if (!player) return;

            if (context.performed)
            {
                print("Right Click");
                var enemy = DetectEnemyInMouse();

                if (enemy)
                {
                    print(enemy.name);
                    m_target = enemy;
                    player.fighter.SetTarget(m_target.transform);
                }
            }
        }

        private Collider DetectEnemyInMouse()
        {
            if (!player) return null;

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

        private void OnTriggerEnter(Collider other)
        {
            //Physics.OverlapSphere(transform.position, .5f, 1<<11);
            //if (other.gameObject.GetComponent<Pickable>() is var item && item)
            //{
            //    player.inventary.Add(ItemType.Consumable, item);
            //    Destroy(other.gameObject);
            //}
        }

        private void DetectItems()
        {
            var items = Physics.OverlapSphere(player.transform.position, .5f, 1 << 11);

            foreach (var item in items)
            {
                var cmp = item.GetComponent<PickableItem>();
                if (cmp)
                {
                    var inventary = player.GetComponent<Inventary>();
                    print("Using Item");
                    inventary.Add(cmp.type, cmp.Item);
                    inventary.Equip(cmp.type, cmp.Item);
                    item.gameObject.SetActive(false);
                }
            }
        }
    }
}

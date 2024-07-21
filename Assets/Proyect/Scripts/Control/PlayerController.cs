using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Control.AI;
using Burmuruk.Tesis.Interaction;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Burmuruk.Tesis.Control
{
    class PlayerController : MonoBehaviour
    {
        Character player;
        GameManager gameManager;
        LevelManager levelManager;

        bool m_shouldMove = false;
        Vector3 m_direction = default;
        bool m_canChangeFormation = false;
        private Dictionary<Transform, PickableItem> m_pickables = new ();
        private List<IInteractable> m_interactables = new List<IInteractable>();
        int interactableIdx = 0;
        
        enum Interactions
        {
            None,
            Pickable,
            Talk,
            Interact
        }

        public event Action<bool> OnFormationHold;
        public event Action<Vector2, object> OnFormationChanged;
        public event Action<bool, string> OnPickableEnter;
        public event Action<bool, string> OnPickableExit;
        public event Action<string, Vector3> OnItemPicked;
        public event Action<bool, string> OnInteractableEnter;
        public event Action<bool, string> OnInteractableExit;

        public AIEnemyController Target { get; private set; }
        public bool HavePickable
        {
            get
            {
                if (m_pickables.Count > 0)
                {
                    return true;
                }

                return false;
            }
        }

        private void Start()
        {
            gameManager = GetComponent<GameManager>();
            levelManager = GetComponent<LevelManager>();
        }

        void FixedUpdate()
        {
            if (m_shouldMove && player && GameManager.Instance.GameState == GameManager.State.Playing)
            {
                try
                {
                    player.mover.MoveTo(player.transform.position + m_direction * 2);
                }
                catch (NullReferenceException)
                {

                    throw;
                }
            }

            DetectItems();
            DetectInteractables();
        }

        public void SetPlayer(Character player)
        {
            var vollider = player.GetComponent<CapsuleCollider>();
            this.player = player;
        }

        #region Inputs
        public void Move(InputAction.CallbackContext context)
        {
            if (!player) return;

            if (gameManager.GameState == GameManager.State.UI)
            {
                if (context.performed)
                {
                    var dir = context.ReadValue<Vector2>();
                    if (dir.magnitude <= 0)
                    {
                        m_shouldMove = false;
                        return;
                    }

                    levelManager.RotatePlayer(dir);
                    m_shouldMove = true;
                }
                else
                {
                    levelManager.RotatePlayer(Vector2.zero);
                    m_shouldMove = false;
                }
            }
            else if (gameManager.GameState != GameManager.State.Playing)
                return;

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
                m_shouldMove = false;
            }
        }

        public void SelectTarget(InputAction.CallbackContext context)
        {
            if (!player) return;

            if (context.performed)
            {
                var enemy = DetectEnemyInMouse();

                if (enemy)
                {
                    Target = enemy.GetComponent<AIEnemyController>();
                    ((AIGuildMember)player).SetTarget(Target);
                    //print(enemy.name);
                    //player.fighter.SetTarget(Target.transform);
                }
            }
        }

        public void DisplayFormations(InputAction.CallbackContext context)
        {
            if (!player) return;

            if (context.performed)
            {
                m_canChangeFormation = true;

                OnFormationHold?.Invoke(true);
            }
            else
            {
                if (m_canChangeFormation)
                    OnFormationHold?.Invoke(false);

                m_canChangeFormation = false;
            }
        }

        public void ChangeFormation(InputAction.CallbackContext context)
        {
            if (!player || gameManager.GameState != GameManager.State.Playing) return;

            if (context.performed && m_canChangeFormation)
            {
                var dir = context.ReadValue<Vector2>();

                if (dir.y == -1 && Target == null)
                    return;

                object args = dir switch
                {
                    { y: -1 } => Target,
                    _ => null
                };

                OnFormationChanged?.Invoke(dir, args);
            }
        }

        public void Interact(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (HavePickable)
            {
                var cmp = m_pickables.First().Value;
                player.Inventory.Add(cmp.ID);
                //var inventory = GetComponent<InventoryEquipDecorator>();
                //inventory.Add(cmp.itemType, cmp);
                //inventory.TryEquip(player, cmp.itemType, cmp.GetSubType());
                cmp.gameObject.SetActive(false);

                //m_pickables.Remove(cmp.transform);

                OnItemPicked?.Invoke(cmp.itemType.ToString(), cmp.transform.position);
            }
            else if (m_interactables.Count > 0)
            {
                m_interactables[0].Interact();
            }
        }

        public void Cross(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            var value = context.ReadValue<Vector2>();
        }

        public void Pause(InputAction.CallbackContext context)
        {
            if (gameManager.GameState == GameManager.State.UI)
            {
                levelManager.ExitUI();
            }
            else
            {
                levelManager.Pause();
            }
        }

        public void ShowMoreOptions(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (gameManager.GameState == GameManager.State.UI)
            {
                levelManager.ShowMoreOptions();
            }
        }

        public void Remove(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (gameManager.GameState == GameManager.State.UI)
            {
                levelManager.Remove();
            }
        }

        public void ChangeMenu(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (gameManager.GameState == GameManager.State.UI)
            {
                levelManager.ChangeMenu();
            }
        }

        public void UseAbility1(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
        }

        public void UseAbility2(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
        }

        public void UseAbility3(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
        }

        public void UseAbility4(InputAction.CallbackContext context)
        {
            if (!context.performed) return;
        }
        #endregion

        #region Private methods
        public void UseAbility(Ability ability)
        {
            if (gameManager.GameState != GameManager.State.Playing)
                return;

            if (ability == null) return;

            switch ((AbilityType)ability.GetSubType())
            { 
                case AbilityType.None:
                    break;
                case AbilityType.Dash:
                    //ability.Use();
                    break;
                case AbilityType.StealHealth:
                    break;
                case AbilityType.Jump:
                    break;
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
            //if (other.gameObject.GetComponent<Consumable>() is var itemType && itemType)
            //{
            //    player.inventory.Add(Type.Consumable, itemType);
            //    Destroy(other.gameObject);
            //}
        }

        private void DetectItems()
        {
            var items = Physics.OverlapSphere(player.transform.position, 1.5f, 1 << 11);
            var hadItem = m_pickables.Count > 0;
            //m_pickables.Clear();
            Dictionary<Transform, PickableItem> newList = new();
            List<PickableItem> newPickables = new();

            foreach (var item in items)
            {
                var cmp = item.GetComponent<PickableItem>();
                if (cmp)
                {
                    if (!m_pickables.ContainsKey(cmp.transform))
                    {
                        newPickables.Add(cmp);
                    }
                    else
                    {
                        m_pickables.Remove(cmp.transform);
                    }    
                    
                    newList.Add(cmp.transform, cmp);
                }
            }

            if (newList.Count <= 0)
                foreach (var item in m_pickables)
                    OnPickableExit?.Invoke(false, "");

            foreach (var item in newPickables)
                OnPickableEnter?.Invoke(true, "Tomar"/* + m_items[0].subType.ToString()*/);

            m_pickables = newList;
        }

        private void DetectInteractables()
        {
            var items = Physics.OverlapSphere(player.transform.position, 1f, 1 << 11);
            var hadItem = m_interactables.Count > 0;
            m_interactables.Clear();

            foreach (var item in items)
            {
                var cmp = item.GetComponent<IInteractable>();
                if (cmp != null)
                {
                    m_interactables.Add(cmp);
                }
            }

            if (hadItem && m_interactables.Count <= 0)
            {
                OnPickableExit?.Invoke(false, "");
            }
            else if (m_interactables.Count > 0)
            {
                OnPickableEnter?.Invoke(true, "Interact");
            }
        }

        //private void TakeItem()
        //{
        //    var cmp = m_pickables[0];
        //    var inventory = player.GetComponent<InventoryEquipDecorator>();
        //    inventory.Add(cmp.itemType, cmp);
        //    //inventory.ElementAction(cmp.subType, cmp.Item);
        //    cmp.gameObject.SetActive(false);

        //    m_pickables.Remove(cmp);

        //    OnItemPicked?.Invoke(cmp.itemType.ToString(), cmp.transform.position);
        //}
        #endregion
    }
}

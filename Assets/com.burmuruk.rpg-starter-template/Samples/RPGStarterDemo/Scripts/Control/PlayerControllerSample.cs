using Burmuruk.RPGStarterTemplate.Inventory;
using Burmuruk.RPGStarterTemplate.Stats;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Burmuruk.RPGStarterTemplate.Control.Samples
{
    public class PlayerControllerSample : PlayerController
    {
        public event Action<bool> OnFormationHold;
        public event Action<Vector2, object> OnFormationChanged;

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

        public void Cross(InputAction.CallbackContext context)
        {
            if (!context.performed) return;

            if (gameManager.GameState != GameManager.State.Playing || !player) return;

            var value = context.ReadValue<Vector2>();

            switch (value)
            {
                case { y: < 0 }:
                    ConsumeItem();
                    break;

                case { x: < 0 }:
                    ChangeItem(-1);
                    break;

                case { x: > 0 }:
                    ChangeItem(1);
                    break;

                case { y: > 0 }:
                    //ShowItems()
                    break;

                default:
                    break;
            }
        }

        protected void ConsumeItem()
        {
            var items = (player.Inventory as InventoryEquipDecorator).Equipped.GetItems((int)EquipmentLocation.Items);

            if (items == null || items.Count == 0)
                return;

            (items[0] as ConsumableItem).Use(player, null, null);
        }

        protected void ChangeItem(int v)
        {

        }
    }
}

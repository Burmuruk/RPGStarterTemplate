using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Control.AI
{
    public class EnemyManager : MonoBehaviour
    {
        [SerializeField] CharacterProgress progress;
        [SerializeField] Inventory.Inventory inventory;
        Dictionary<int, List<AIEnemyController>> m_enemies = new();

        [Serializable]
        public struct EnemyGroup
        {
            [SerializeField] Transform transform;
            [SerializeField] List<AIEnemyController> enemies;
            [SerializeField] bool canBeRespawned;
            public State GroupState { get; private set; }

            public enum State
            {
                None,
                Defeated,
                Inactive,
            }

            public event Action OnGroupDefeated;

            public void RespawnEnemies()
            {
                if (!canBeRespawned) return;

                foreach (var enemy in enemies)
                {

                    enemy.gameObject.SetActive(true);
                }
            }
        }

        private void Start()
        {
            inventory = FindObjectOfType<LevelManager>().gameObject.GetComponent<Inventory.Inventory>();

            if (inventory == null) return;

            var enemies = FindObjectsOfType<AIEnemyController>(true);

            foreach (var enemy in enemies)
            {
                try
                {
                    enemy.SetStats(progress.GetDataByLevel(enemy.CharacterType, 0));
                    (enemy.Inventory as InventoryEquipDecorator).SetInventory(inventory);
                    enemy.SetUpMods();
                    //enemy.Health.OnDied += 
                }
                catch (NullReferenceException)
                {

                    throw;
                }
            }
        }

        public void EnableGroup(int id)
        {

        }
    }
}

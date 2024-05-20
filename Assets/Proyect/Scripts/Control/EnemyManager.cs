using Assets.Proyect.Scripts.Control;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Control
{
    public class EnemyManager : MonoBehaviour
    {
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

        public void EnableGroup(int id)
        {

        }

        private void RestoreEnemies(int id)
        {
            
        }
    }
}

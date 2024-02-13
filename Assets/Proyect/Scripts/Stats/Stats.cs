using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/CharacterStats", order = 1)]
    public class Stats : ScriptableObject
    {
        [SerializeField] public float speed;
        [SerializeField] public float hp;
        [SerializeField] public float damage;
    }
}

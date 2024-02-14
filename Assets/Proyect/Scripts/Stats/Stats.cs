using UnityEngine;
using UnityEngine.InputSystem.XR;

namespace Burmuruk.Tesis.Stats
{
    [CreateAssetMenu(fileName = "Stats", menuName = "ScriptableObjects/CharacterStats", order = 1)]
    public class Stats : ScriptableObject
    {
        [Header("Basic stats")]
        [SerializeField] public float speed;
        [SerializeField] public int hp;
        [SerializeField] public float damage;
        [Space(), Header("Detection")]
        [SerializeField] public float eyesRadious;
        [SerializeField] public float earsRadious;
    }
}

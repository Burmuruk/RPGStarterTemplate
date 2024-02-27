using Burmuruk.Tesis.Control;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    class Pickable : MonoBehaviour, ISaveableItem
    {
        [SerializeField] int hp;

        public int Hp;
    }

    public interface IPickable
    {
        void Use();
    }

    public interface IItem
    {
        void Use();
    }
}

using Burmuruk.Tesis.Control;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    class Pickable : MonoBehaviour, ISaveableItem
    {
        [SerializeField] int hp;
        [SerializeField] string m_name;
        [SerializeField] string m_description;

        public int Hp;

        public int GetSubType()
        {
            return (int)hp;
        }

        public string GetName()
        {
            return m_name;
        }

        public string GetDescription()
        {
            return m_description;
        }
    }

    public enum PickableType
    {
        None,
        Hp,
        Speed
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

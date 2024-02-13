using UnityEngine;

namespace Burmuruk.Tesis.Control
{
    public class Character : MonoBehaviour
    {
        protected Movement.Movement m_mover;
        protected Fighting.Fighter m_fighter;

        protected virtual void Awake()
        {
            m_mover = GetComponent<Movement.Movement>();
            m_fighter = GetComponent<Fighting.Fighter>();
        }

        protected virtual void Start()
        {

        }
    }
}

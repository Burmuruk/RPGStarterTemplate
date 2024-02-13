using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Burmuruk.Tesis.Movement;
using UnityEngine.InputSystem;

namespace Burmuruk.Tesis.Control
{
    public class Character : MonoBehaviour
    {
        Movement.Movement m_mover;

        void Start()
        {

        }

        void Update()
        {

        }

        public void Move(InputAction.CallbackContext context)
        {
            var dir = context.ReadValue<Vector2>();

            m_mover.MoveTo(dir);
        }
    } 
}

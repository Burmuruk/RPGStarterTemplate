using Burmuruk.Tesis.Stats;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.UI
{
    public class UIMenuCharacters : MonoBehaviour
    {
        [SerializeField] StackableLabel elementPanel;
        [SerializeField] GameObject character;
        [SerializeField] float rotationVelocity;
        Vector2 direction;

        private void Update()
        {
            Rotate();
        }

        private void Rotate()
        {
            character.transform.Rotate(Vector3.up, direction.x * rotationVelocity * -1);
        }

        public void ShowModifications()
        {

        }

        public void ShowAbilities()
        {

        }

        public void ShowInvantary()
        {

        }

        public void ShowCharacters()
        {

        }

        public void SetInventary(Inventary inventary)
        {

        }

        public void RotatePlayer(Vector2 direction)
        {
            this.direction = direction;
        }
    } 
}

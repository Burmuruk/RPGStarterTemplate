﻿using UnityEngine;
using UnityEngine.Events;

namespace Burmuruk.RPGStarterTemplate.Dialogue
{
    public class DialogueTrigger : MonoBehaviour
    {
        [SerializeField] string action;
        [SerializeField] UnityEvent onTrigger;

        public void Trigger (string actionToTrigger)
        {
            if (actionToTrigger == action)
            {
                onTrigger?.Invoke();
            }
        }
    }
}

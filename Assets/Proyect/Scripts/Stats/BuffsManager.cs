using Burmuruk.Tesis.Control;
using Burmuruk.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public class BuffsManager : MonoBehaviour
    {
        private List<CoolDownAction> timers = new();

        public static BuffsManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
                Destroy(this);
        }

        public void AddBuff(Character character, ModsType type, object value, float time = 0)
        {
            if (time > 0)
            {
                timers.Add(new CoolDownAction(time, (_) => RemoveBuff(character, type)));
            }

            ModsList.Add(character, type, (float)value);
        }

        public void RemoveBuff(Character character, ModsType type)
        {

        }
    }
}
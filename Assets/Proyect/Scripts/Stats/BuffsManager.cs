using Burmuruk.Tesis.Control;
using Burmuruk.Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public class BuffsManager : MonoBehaviour
    {
        [SerializeField] int timersCount = 30;
        private Queue<CoolDownAction> timers = new();
        private List<(Character character, Coroutine coroutine, CoolDownAction cd)> runningTimers = new();

        public static BuffsManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
            {
                Initilize();
            }
            else
                Destroy(this);
        }

        public void AddBuff(Character character, ModifiableStat type, float value, float time = 0)
        {
            var mod = (float)value;

            if (time > 0)
            {
                if (timers.Count <= 0) return;

                SetTimer(value);
            }

            ModsList.AddModification(character, type, mod);

            void SetTimer(float mod)
            {
                var coolDown = timers.Dequeue();
                coolDown.ResetAttributes(time, (_) => RemoveBuff(coolDown, character, type, mod));

                var coroutine = StartCoroutine(coolDown.CoolDown());
                runningTimers.Add((character, coroutine, coolDown));
            }
        }

        public void RemoveBuff(CoolDownAction coolDown, Character character, ModifiableStat type, float modification)
        {
            ModsList.RemoveModification(character, type, modification);
            timers.Enqueue(coolDown);
        }

        public void RemoveAllBuffs(Character character)
        {
            ModsList.RemoveAllModifications(character);

            for (int i = 0; i < runningTimers.Count; i++)
            {
                if (runningTimers[i].character == character)
                {
                    StopCoroutine(runningTimers[i].coroutine);
                    timers.Enqueue(runningTimers[i].cd);

                    runningTimers.RemoveAt(i);
                    return;
                }
            }
        }

        private void Initilize()
        {
            Instance = this;

            for (int i = 0; i < timersCount; i++)
            {
                timers.Enqueue(new CoolDownAction(0));
            }
        }
    }
}
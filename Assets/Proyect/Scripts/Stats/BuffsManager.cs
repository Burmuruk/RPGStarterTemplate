using Burmuruk.Tesis.Control;
using Burmuruk.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace Burmuruk.Tesis.Stats
{
    public class BuffsManager : MonoBehaviour
    {
        [SerializeField] int timersCount = 30;
        private Queue<CoolDownAction> timers = new();
        private Dictionary<CoolDownAction, (Control.Character character, Coroutine coroutine)> runningTimers = new();

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

        public void AddBuff(Control.Character character, ModifiableStat type, float value, float time = 0)
        {
            var mod = (float)value;

            if (time > 0)
            {
                if (timers.Count <= 0) return;

                SetTimer(character, type, mod, time);
            }

            ModsList.AddModification(character, type, mod);
        }

        public void RemoveBuff(CoolDownAction coolDown, Control.Character character, ModifiableStat type, float modification)
        {
            ModsList.RemoveModification(character, type, modification);
            RemoveTimer(coolDown);
        }

        public void RemoveAllBuffs(Control.Character character)
        {
            ModsList.RemoveAllModifications(character);

            var coolDowns = (from timer in runningTimers
                            where timer.Value.character == character
                            select timer.Key)
                            .ToArray();

            foreach (var coolDown in coolDowns)
            {
                StopCoroutine(runningTimers[coolDown].coroutine);
                RemoveTimer(coolDown);
            }
        }

        public void HealOverTime(Control.Character character, int value, float time, float tickTime)
        {
            var coolDown = timers.Dequeue();

            coolDown.ResetAttributes(time, tickTime, () => character.Health.Heal(value), (_) => RemoveTimer(coolDown));

            var coroutine = StartCoroutine(coolDown.Tick());
            runningTimers.Add(coolDown, (character, coroutine));
        }

        private void Initilize()
        {
            Instance = this;

            for (int i = 0; i < timersCount; i++)
            {
                timers.Enqueue(new CoolDownAction(0));
            }
        }

        void SetTimer(Control.Character character, ModifiableStat type, float mod, float time)
        {
            var coolDown = timers.Dequeue();

            coolDown.ResetAttributes(time, (_) => RemoveBuff(coolDown, character, type, mod));

            var coroutine = StartCoroutine(coolDown.CoolDown());
            runningTimers.Add(coolDown, (character, coroutine));
        }

        private void RemoveTimer(CoolDownAction coolDown)
        {
            runningTimers.Remove(coolDown);
            timers.Enqueue(coolDown);
        }
    }
}
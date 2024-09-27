using Burmuruk.Tesis.Control;
using Burmuruk.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Burmuruk.Tesis.Stats
{
    public class BuffsManager : MonoBehaviour
    {
        const int timersCount = 30;
        private Queue<CoolDownAction> timers = new();
        private Dictionary<CoolDownAction, (Character character, Coroutine coroutine, BuffData buff)> runningTimers = new();

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

        public void AddBuff(Character character, in BuffData buff, Action tickAction = null)
        {
            if (buff.duration > 0)
            {
                if (timers.Count <= 0) return;

                SetTimer(character, buff, tickAction);
            }

            ModsList.AddModification(character, buff.stat, buff.value);
        }

        public void RemoveBuff(CoolDownAction coolDown, Character character, ModifiableStat type, float modification)
        {
            ModsList.RemoveModification(character, type, modification);
            RemoveTimer(coolDown);
        }

        public void RemoveAllBuffs(Character character)
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

        private void Initilize()
        {
            Instance = this;

            for (int i = 0; i < timersCount; i++)
            {
                timers.Enqueue(new CoolDownAction(0));
            }
        }

        void SetTimer(Character character, BuffData buff, Action tickAction)
        {
            var coolDown = timers.Dequeue();
            Coroutine coroutine = null;

            if (tickAction == null)
            {
                coolDown.ResetAttributes(buff.duration, (_) => RemoveBuff(coolDown, character, buff.stat, buff.value));
                coroutine = StartCoroutine(coolDown.CoolDown());
            }
            else
            {
                coolDown.ResetAttributes(buff.duration, buff.rate, tickAction, (_) => RemoveTimer(coolDown));
                coroutine = StartCoroutine(coolDown.Tick());
            }

            runningTimers.Add(coolDown, (character, coroutine, buff));
        }

        private void RemoveTimer(CoolDownAction coolDown)
        {
            runningTimers.Remove(coolDown);
            timers.Enqueue(coolDown);
        }

        public void RemoveAllBuffs()
        {
            StopAllCoroutines();

            var keys = runningTimers.Keys.ToArray();
            int i = 0;

            while (runningTimers.Count > 0)
            {
                RemoveTimer(keys[i]);
            }
        }

        public KeyValuePair<CoolDownAction, (Character character, Coroutine coroutine, BuffData buff)>[] GetCharacterTimers(Character character)
        {
            return (from timer in runningTimers
                            where timer.Value.character == character
                            select timer)
                            .ToArray();
        }
    }
}
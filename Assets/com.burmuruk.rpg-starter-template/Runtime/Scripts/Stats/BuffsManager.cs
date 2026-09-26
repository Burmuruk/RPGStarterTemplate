using Burmuruk.RPGStarterTemplate.Control;
using Burmuruk.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Burmuruk.RPGStarterTemplate.Stats
{
    public class BuffsManager : MonoBehaviour
    {
        private sealed class ActiveEffect
        {
            public Guid Id;
            public Guid ModifierId;
            public Character Character;
            public BuffData Data;
            public Action Tick;
            public CoolDownAction Timer;
        }

        private readonly Dictionary<Guid, ActiveEffect> active = new();
        const int timersCount = 30;
        private readonly Queue<CoolDownAction> timers = new();
        private readonly Dictionary<CoolDownAction, (Character character, Coroutine coroutine, BuffData buff)> runningTimers =
            new();
        public static BuffsManager Instance { get; private set; }
        public int ActiveEffectCount => active.Count;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Initilize();
        }

        private void OnDisable()
        {
            if (Instance == this)
                RemoveAllBuffs();
        }

        private void OnDestroy()
        {
            if (Instance != this)
                return;

            RemoveAllBuffs();
            Instance = null;
        }

        private void Initilize()
        {
            Instance = this;
            for (int i = 0; i < timersCount; i++)
                timers.Enqueue(new CoolDownAction(0));
        }

        public void AddBuff(Character character, in BuffData buff, Action tickAction = null)
        {
            ApplyEffect(character, buff, tickAction);
        }

        public Guid ApplyEffect(Character character, BuffData buff, Action action = null, bool rollProbability = false)
        {
            Guid? guid = VerifyRequest(character, buff, action, rollProbability, out bool modifier, out bool timed);

            if (guid.HasValue)
                return guid.Value;

            if (buff.effectType == EffectType.Instant)
            {
                ExecuteChange(character, buff, action);
                return Guid.Empty;
            }

            var effect = new ActiveEffect
            {
                Id = Guid.NewGuid(),
                Character = character,
                Data = buff,
                Tick = action
            };
            if (modifier)
            {
                float delta = buff.value;
                if (buff.percentage)
                {
                    ModsList.TryGetBaseValue(character, buff.stat, out float basis);
                    delta = basis * buff.value / 100f;
                }
                if (!ModsList.AddModification(character, buff.stat, delta, out effect.ModifierId))
                    return Reject("Could not register modifier: " + buff.stat);
            }
            active.Add(effect.Id, effect);
            if (timed)
                SetTimer(effect);
            return effect.Id;
        }

        private Guid? VerifyRequest(Character character, BuffData buff, Action action, bool rollProbability, out bool modifier, out bool timed)
        {
            modifier = false;
            timed = false;

            if (character == null || !character.gameObject.activeInHierarchy || !isActiveAndEnabled)
                return Guid.Empty;

            if (!ValidNumber(buff.value) || !ValidNumber(buff.duration) || !ValidNumber(buff.rate))
                return Reject("Effect contains a non-finite value.");

            if (!Enum.IsDefined(typeof(EffectType), buff.effectType))
                return Reject("Unknown effect type.");

            modifier = buff.effectType == EffectType.Temporary || buff.effectType == EffectType.UntilRemoved;
            timed = buff.effectType == EffectType.Temporary || buff.effectType == EffectType.Periodic;

            if (timed && buff.duration <= 0)
                return Reject("Temporary/Periodic effects require duration > 0.");

            if (buff.effectType == EffectType.Periodic && buff.rate <= 0)
                return Reject("Periodic effects require rate > 0.");

            if (modifier && action != null)
                return Reject("Reversible modifiers cannot use a one-way callback.");

            if (action == null && !ModsList.IsRegistered(character, buff.stat))
                return Reject("Stat is not registered: " + buff.stat);

            if (action == null && buff.stat == ModifiableStat.HP && character.Health == null)
                return Reject("Character has no Health component.");

            if (action == null && buff.stat == ModifiableStat.HP && buff.percentage)
                return Reject("HP percentages need an explicit callback defining current/max HP as the reference.");

            if (rollProbability)
            {
                if (!ValidNumber(buff.probability) || buff.probability < 0 || buff.probability > 1)
                    return Reject("Probability must be in [0, 1].");

                if (buff.probability <= 0 || (buff.probability < 1 && UnityEngine.Random.value >= buff.probability))
                    return Guid.Empty;
            }

            return null;
        }

        private static bool ValidNumber(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }

        private Guid Reject(string message) { Debug.LogWarning(message, this); return Guid.Empty; }

        private static void ExecuteChange(Character character, BuffData data, Action action)
        {
            if (action != null)
            { action(); return; }
            if (data.stat == ModifiableStat.HP)
            {
                int amount = Mathf.RoundToInt(Mathf.Abs(data.value));

                if (amount == 0)
                    return;

                if (data.value < 0)
                    character.Health.ApplyDamage(amount);
                else
                    character.Health.Heal(amount);
                return;
            }

            float delta = data.value;

            if (data.percentage)
            {
                if (!ModsList.TryGetBaseValue(character, data.stat, out float basis))
                    return;

                delta = basis * data.value / 100f;
            }

            ModsList.ApplyChange(character, data.stat, delta);
        }

        private void SetTimer(ActiveEffect effect)
        {
            CoolDownAction coolDown = timers.Count > 0 ? timers.Dequeue() : new CoolDownAction(0);
            effect.Timer = coolDown;

            if (effect.Data.effectType == EffectType.Periodic)
            {
                coolDown.ResetAttributes(effect.Data.duration, effect.Data.rate,
                    () =>
                    {
                        ExecuteChange(effect.Character, effect.Data, effect.Tick);

                        if (effect.Character == null || !effect.Character.gameObject.activeInHierarchy)
                            RemoveEffectInternal(effect.Id, false);
                    },
                    _ => RemoveEffectInternal(effect.Id, false));
            }
            else
                coolDown.ResetAttributes(effect.Data.duration, _ => RemoveEffectInternal(effect.Id, false));

            runningTimers.Add(coolDown, (effect.Character, null, effect.Data));
            Coroutine coroutine = StartCoroutine(RunTimer(effect));

            if (runningTimers.ContainsKey(coolDown))
                runningTimers[coolDown] = (effect.Character, coroutine, effect.Data);
        }

        private IEnumerator RunTimer(ActiveEffect effect)
        {
            IEnumerator routine = effect.Data.effectType == EffectType.Periodic
                ? effect.Timer.Tick() : effect.Timer.CoolDown();

            while (active.ContainsKey(effect.Id))
            {
                if (effect.Character == null || !effect.Character.gameObject.activeInHierarchy)
                { 
                    RemoveEffectInternal(effect.Id, false); 
                    yield break; 
                }
                bool hasNext;

                try
                { 
                    hasNext = routine.MoveNext(); 
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception, this);
                    RemoveEffectInternal(effect.Id, false);
                    hasNext = false;
                }

                if (!hasNext)
                { 
                    RemoveEffectInternal(effect.Id, false); 
                    yield break; 
                }

                yield return routine.Current;
            }
        }

        private void RemoveTimer(CoolDownAction coolDown, bool stopCoroutine = true)
        {
            if (!runningTimers.TryGetValue(coolDown, out (Character character, Coroutine coroutine, BuffData buff) timer))
                return;

            runningTimers.Remove(coolDown);
            coolDown.Cancel();

            if (stopCoroutine && timer.coroutine != null)
                StopCoroutine(timer.coroutine);

            timers.Enqueue(coolDown);
        }

        public bool RemoveEffect(Guid id)
        {
            return RemoveEffectInternal(id, true);
        }

        private bool RemoveEffectInternal(Guid id, bool stopCoroutine)
        {
            if (!active.TryGetValue(id, out ActiveEffect effect))
                return false;

            active.Remove(id);

            if (effect.Timer != null)
                RemoveTimer(effect.Timer, stopCoroutine);
            if (effect.ModifierId != Guid.Empty && effect.Character != null)
                ModsList.RemoveModification(effect.Character, effect.Data.stat, effect.ModifierId);

            return true;
        }

        public bool RefreshEffect(Guid id)
        {
            if (!active.TryGetValue(id, out ActiveEffect effect) || effect.Timer == null)
                return false;

            effect.Timer.Restart();
            return true;
        }

        public void RemoveBuff(CoolDownAction coolDown, Character character, ModifiableStat type, float modification)
        {
            ActiveEffect effect = active.Values.FirstOrDefault(item => item.Timer == coolDown &&
                item.Character == character && item.Data.stat == type);

            if (effect != null)
                RemoveEffect(effect.Id);
        }

        public KeyValuePair<CoolDownAction, (Character character, Coroutine coroutine, BuffData buff)>[] GetCharacterTimers(Character character)
        {
            return runningTimers.Where(timer => timer.Value.character == character).ToArray();
        }

        public void RemoveAllBuffs(Character character)
        {
            var ids = new List<Guid>();
            foreach (KeyValuePair<Guid, ActiveEffect> pair in active)
            {
                if (pair.Value.Character == character)
                    ids.Add(pair.Key);
            }

            foreach (Guid id in ids)
                RemoveEffect(id);
            
            if (character != null)
                ModsList.RemoveAllModifications(character);
        }

        public void RemoveAllBuffs()
        {
            foreach (Guid id in new List<Guid>(active.Keys))
                RemoveEffect(id);
        }
    }
}

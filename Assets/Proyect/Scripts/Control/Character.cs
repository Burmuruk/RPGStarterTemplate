using System;
using System.Collections.Generic;
using UnityEngine;
using Burmuruk.Tesis.Stats;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Utilities;
using Burmuruk.Tesis.Saving;
using Newtonsoft.Json.Linq;

namespace Burmuruk.Tesis.Control
{
    public class Character : MonoBehaviour, IJsonSaveable
    {
        [Header("References")]
        [SerializeField] protected Transform farPercept;
        [SerializeField] protected Transform closePercept;
        [Space(), Header("Status"), Space()]
        [SerializeField] protected bool hasFarPerception;
        [SerializeField] protected bool hasClosePerception;
        [SerializeField] CharacterType characterType;
        [SerializeField] protected Equipment equipment;

        [Space(), Header("Settings")]
        [SerializeField] protected string enemyTag;
        //[SerializeField] protected float farRadious;
        //[SerializeField] protected float closeRadious;

        [HideInInspector] protected Health health;
        [HideInInspector] public Movement.Movement mover;
        [HideInInspector] public Fighter fighter;
        [HideInInspector] public BasicStats stats = new();
        [HideInInspector] public ActionScheduler actionScheduler = new();
        [HideInInspector] protected IInventory inventory;

        protected Collider[] eyesPerceibed, earsPerceibed;
        protected bool isTargetFar = false;
        protected bool isTargetClose = false;

        public virtual event Action<bool> OnCombatStarted;

        public Health Health { get => health; }
        public IInventory Inventory {
            get
            {
                return (inventory ??= gameObject.GetComponent<IInventory>());
            }
            set => inventory = value;
        }
        public Collider[] CloseEnemies { get => earsPerceibed; }
        public Collider[] FarEnemies { get => eyesPerceibed; }
        public bool IsTargetFar { get => isTargetFar; }
        public bool IsTargetClose { get => isTargetClose; }
        public ref Equipment Equipment { get => ref equipment; }
        public CharacterType CharacterType { get => characterType; }

        protected virtual void Awake()
        {
            GetComponents();
            health.OnDied += Dead;
        }

        protected virtual void Update()
        {
            if (health.HP <= 0) return;

            DecisionManager();
        }

        protected virtual void FixedUpdate()
        {
            if (health.HP <= 0) return;

            eyesPerceibed = Physics.OverlapSphere(farPercept.position, stats.eyesRadious, 1 << 10);
            earsPerceibed = Physics.OverlapSphere(closePercept.position, stats.earsRadious, 1 << 10);

            PerceptionManager();
        }

        private void OnDrawGizmosSelected()
        {
            if (/*!stats.Initilized ||*/ !farPercept || !closePercept) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(farPercept.position, stats.eyesRadious);
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(closePercept.position, stats.earsRadious);
        }

        public void SetUpMods()
        {
            //ModsList.AddVariable(this, ModifiableStat.HP, _=>health.HP, (value) => health.HP = value);
            ModsList.AddVariable((Character)this, ModifiableStat.Speed, () => stats.Speed, (value) => stats.Speed = value);
            ModsList.AddVariable((Character)this, ModifiableStat.BaseDamage, () => stats.Damage, (value) => { stats.Damage = (int)value; });
            ModsList.AddVariable((Character)this, ModifiableStat.GunFireRate, () => stats.DamageRate, (value) => { stats.damageRate = value; });
            ModsList.AddVariable((Character)this, ModifiableStat.MinDistance, () => stats.MinDistance, (value) => { stats.MinDistance = value; });
        }

        public virtual void SetStats(BasicStats stats)
        {
            this.stats = stats;
            var invent = Inventory as InventoryEquipDecorator;
            if (!fighter)
                GetComponents();
            fighter.Initilize(invent, ref this.stats);
            mover.Initialize(invent, actionScheduler, this.stats);
        }

        protected virtual void PerceptionManager()
        {
            if (hasFarPerception)
            {
                isTargetFar = PerceptEnemy(ref eyesPerceibed);
            }
            if (hasClosePerception)
            {
                isTargetClose = PerceptEnemy(ref earsPerceibed);
            }
        }

        private void GetComponents()
        {
            health ??= GetComponent<Health>();
            mover ??= GetComponent<Movement.Movement>();
            fighter ??= GetComponent<Fighter>();
        }

        protected virtual bool PerceptEnemy(ref Collider[] perceibed)
        {
            if (perceibed == null) return false;

            List<Collider> enemies = new();
            bool founded = false;

            for (int i = 0; i < perceibed.Length; i++)
            {
                ref var cur = ref perceibed[i];

                if (cur.CompareTag(enemyTag))
                {
                    enemies.Add(cur);
                    founded = true;
                }
            }

            if (enemies.Count > 0)
            {
                perceibed = enemies.ToArray();
            }

            return founded;
        }

        protected virtual void DecisionManager() { }

        protected virtual void ActionManager() { }

        protected virtual void MovementManager() { }

        protected Transform GetNearestTarget(Collider[] eyesPerceibed)
        {
            (Transform enemy, float dis) closest = (null, float.MaxValue);

            foreach (var enemy in eyesPerceibed)
            {
                if (Vector3.Distance(enemy.transform.position, transform.position) is var d && d < closest.dis)
                {
                    closest = (enemy.transform, d);
                }
            }

            return closest.enemy;
        }

        protected virtual void Dead()
        {
            gameObject.SetActive(false);
            StopAllCoroutines();
            FindObjectOfType<BuffsManager>().RemoveAllBuffs(this);
        }

        #region Saving
        public JToken CaptureAsJToken()
        {
            return CaptureCharacterData();
        }

        public void RestoreFromJToken(JToken state)
        {
            RestoreCharacterData(state);
        }

        protected virtual JToken CaptureCharacterData()
        {
            JObject state = new JObject();

            state["Position"] = VectorToJToken.CaptureVector(transform.position);
            state["Rotation"] = VectorToJToken.CaptureVector(transform.rotation.eulerAngles);

            return state;
        }

        protected virtual void RestoreCharacterData(JToken jToken)
        {
            if (jToken is JObject jObject)
            {
                IDictionary<string, JToken> data = jObject;
                transform.position = data["Position"].ToObject<Vector3>();
                transform.rotation = Quaternion.Euler(data["Rotation"].ToObject<Vector3>());

                mover.UpdatePosition();
            }
        }

        protected JObject CaptureEquipment()
        {
            JObject equipmentData = new();

            foreach (var part in Enum.GetValues(typeof(EquipmentType)))
            {
                int partId = (int)part;
                var items = equipment.GetItems(partId);

                if (items != null)
                {
                    equipmentData[partId.ToString()] = items[0].ID;
                }
            }

            return equipmentData;
        }

        protected void RestoreEquipment(JToken jToken)
        {
            if (jToken is JObject state)
            {
                var equiper = inventory as InventoryEquipDecorator;
                var enumValues = Enum.GetValues(typeof(EquipmentType));

                foreach (var part in enumValues)
                {
                    var items = equipment.GetItems((int)part);

                    if (items == null) continue;

                    foreach (var item in items)
                    {
                        equiper.Unequip(this, item);
                    }
                }

                foreach (var part in enumValues)
                {
                    int partId = (int)part;

                    if (!state.ContainsKey(partId.ToString())) continue;

                    var item = inventory.GetItem(state[partId.ToString()].ToObject<int>());

                    equiper.TryEquip(this, item, out _);
                }
            }
        }

        protected JObject CaptureBasicStatus()
        {
            JObject basicStatsData = new();
            Character character = (Character)this;

            basicStatsData["Speed"] = ModsList.TryGetRealValue(stats.Speed, character, ModifiableStat.Speed);
            basicStatsData["Damage"] = ModsList.TryGetRealValue(stats.Damage, character, ModifiableStat.BaseDamage);
            basicStatsData["DamageRate"] = ModsList.TryGetRealValue(stats.DamageRate, character, ModifiableStat.GunFireRate);
            basicStatsData["Color"] = VectorToJToken.CaptureVector(stats.color);
            basicStatsData["EyesRadious"] = stats.eyesRadious;
            basicStatsData["EarsRadious"] = stats.earsRadious;
            basicStatsData["MinDistance"] = ModsList.TryGetRealValue(stats.MinDistance, character, ModifiableStat.MinDistance);

            return basicStatsData;
        }

        protected void RestoreBasicStatus(JToken jToken)
        {
            if (jToken is JObject state)
            {
                stats.Speed = state["Speed"].ToObject<float>();
                stats.Damage = state["Damage"].ToObject<int>();
                stats.damageRate = state["DamageRate"].ToObject<float>();
                stats.color = state["Color"].ToObject<Vector4>();
                stats.eyesRadious = state["EyesRadious"].ToObject<float>();
                stats.earsRadious = state["EarsRadious"].ToObject<float>();
                stats.MinDistance = state["MinDistance"].ToObject<float>();
            }
        } 

        public JToken CaptureInventory()
        {
            JObject state = new JObject();

            foreach (ItemType type in Enum.GetValues(typeof(ItemType)))
            {
                var items = inventory.GetList(type);

                if (items == null) continue;

                for (int i = 0; i < items.Count; i++)
                {
                    JObject itemState = new();

                    itemState["Id"] = items[i].ID;
                    itemState["Count"] = inventory.GetItemCount(items[i].ID);
                    state[i] = itemState;
                }
            }

            return state;
        }

        public void RestoreInventory(JToken jToken)
        {
            if (!(jToken is JObject state)) return;
            
            int i = 0;

            while (state.ContainsKey(i.ToString()))
            {
                for (int j = 0; j < state["Id"]["Count"].ToObject<int>(); j++)
                {
                    inventory.Add(state[i]["Id"].ToObject<int>());
                }

                i++;
            }
        }
        #endregion
    }
}

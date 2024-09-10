using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Control.AI;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Saving;
using Burmuruk.Tesis.Stats;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Burmuruk.Tesis.Control
{
    class PlayerManager : MonoBehaviour, IJsonSaveable
    {
        [SerializeField] CharacterProgress progress;
        [SerializeField] PlayerCustomization customization;

        List<AIGuildMember> players;
        PlayerController playerController;
        int? m_CurPlayer;
        [SerializeField] string curPlayerName;
        (Formation value, object args) curFormation = default;

        public event Action OnPlayerChanged;
        public event Action OnFormationChanged;
        public event Action<bool> OnCombatEnter;

        public List<AIGuildMember> Players
        {
            get
            {
                if (players == null)
                {
                    FindPlayers();
                    return players;
                }

                return players;
            }
        }
        public IInventory playerInventory
        {
            get
            {
                if (!m_CurPlayer.HasValue)
                {
                    return null;
                }

                return players[m_CurPlayer.Value].Inventory;
            }
        }
        public IInventory MainInventory { get; private set; }
        public Transform CurPlayer
        {
            get
            {
                if (m_CurPlayer.HasValue)
                {
                    return players[m_CurPlayer.Value].transform;
                }

                return null;
            }
        }
        public (Formation value, object args) CurFormation { get => curFormation; }

        private void Awake()
        {
            playerController = FindObjectOfType<PlayerController>();
            playerController.OnFormationChanged += ChangeFormation;
            var mainInventory = GetComponent<Inventory.Inventory>();
            MainInventory = mainInventory;
            
            foreach (var player in Players)
            {
                (player.Inventory as InventoryEquipDecorator).SetInventory((Inventory.Inventory)MainInventory);
                
                var lastColor = player.stats.color;
                player.SetStats(progress.GetDataByLevel(CharacterType.Player, 0));
                player.stats.color = lastColor;

                player.SetUpMods();
            }
        }

        private void Start()
        {
            SetPlayerControl();
        }

        public void SetPlayerControl(int idx)
        {
            if (players != null && idx < players.Count)
            {
                EnableIAInRestOfPlayers(idx);

                players[idx].enabled = false;
                players[idx].EnableControll();
                playerController.SetPlayer(players[idx]);

                m_CurPlayer = idx;
                curPlayerName = players[idx].name;

                OnPlayerChanged?.Invoke();
            }
        }

        public void UseItem(int id)
        {
            var item = MainInventory.GetItem(id);

            if (item == null) return;

            if (item.Type == ItemType.Ability)
            {
                playerController.UseAbility((Ability)item);
            }
        }

        private void FindPlayers()
        {
            var players = (from p in FindObjectsOfType<AIGuildMember>()
                           where p is IPlayable
                           select p).Distinct(new AIGuildMemberComparer()).ToList();

            if (players.Count == 0) return;

            this.players = new();

            foreach (var player in players)
            {
                AddMember(player);
            }
        }

        private void EnableIAInRestOfPlayers(int mainPlayerIdx)
        {
            players.ForEach(p =>
            {
                p.enabled = true;
                p.DisableControll();
                p.SetMainPlayer(players[mainPlayerIdx]);
                p.Fellows = players.Where(fellow => fellow != p && p != players[mainPlayerIdx]).ToArray();
            });
        }

        private void SetPlayerControl()
        {
            SetPlayerControl(0);
        }

        private void ChangeFormation(Vector2 value, object args)
        {
            Formation formation = value switch
            {
                { y: 1 } => Formation.Follow,
                { y: -1 } => Formation.LockTarget,
                { x: -1 } => Formation.Protect,
                { x: 1 } => Formation.Free,
                _ => Formation.None,
            };

            players.ForEach((player) =>
            {
                if (player.enabled)
                {
                    player.SetFormation(formation, args);
                }
            });

            curFormation = (formation, args);
            OnFormationChanged?.Invoke();
        }

        private void EnterToCombatMode(bool shouldEnter)
        {
            PlayerState state = shouldEnter ? PlayerState.Combat : PlayerState.None;

            players.ForEach((player) => { player.PlayerState = state; });

            OnCombatEnter?.Invoke(shouldEnter);
        }

        public void AddMember(AIGuildMember member)
        {
            member.OnCombatStarted += EnterToCombatMode;
            member.SetFormation(curFormation.value, curFormation.args);

            players.Add(member);
            SetColor (member);
        }

        public void RemoveMember(AIGuildMember member)
        {
            member.OnCombatStarted -= EnterToCombatMode;
        }

        private void SetColor(AIGuildMember member)
        {
            if (customization.DefaultColors.Length <= 0) return;

            Color? newColor = default;

            foreach (var color in customization.DefaultColors)
            {
                bool hasColor = false;

                foreach (var player in players)
                {
                    if (player.stats.Color == color)
                    {
                        hasColor = true;
                        break;
                    }
                }

                if (!hasColor)
                {
                    newColor = color;
                    break;
                }
            }

            if (!newColor.HasValue) return;

            member.stats.Color = newColor.Value;

            //List<Color> usedColors = new();

            //checkedPlayers.ForEach(p => usedColors.AddVariable(p.statsList.Color));

            //var availableColors = (from color in playerColors where !usedColors.Contains(color) select color).ToList();

            //var selectedColorIdx = UnityEngine.Random.Range(0, availableColors.MaxCount);

            //member.statsList.Color = playerColors[selectedColorIdx];
        }

        public JToken CaptureAsJToken()
        {
            JObject state = new JObject();

            state["Players"] = CapturePlayers();
            state["Inventory"] = CaptureInventory();

            return state;
        }

        public void RestoreFromJToken(JToken jToken)
        {
            RestorePlayers(jToken["Players"]);
            RestoreInventory(jToken["Inventory"]);
        }

        private JToken CapturePlayers()
        {
            if (players.Count == 0) return null;

            JObject playersState = new JObject();
            var leaderIdentifier = players[0].Leader.GetComponent<JsonSaveableEntity>().GetUniqueIdentifier();

            for (int i = 0; i < players.Count; i++)
            {
                JObject state = new JObject();

                state["ID"] = i;

                string identifier = players[i].GetComponent<JsonSaveableEntity>().GetUniqueIdentifier();
                playersState[identifier] = state;

                if (leaderIdentifier == identifier)
                    playersState["Leader"] = i;
            }

            playersState["Formation"] = (int)curFormation.value;

            return playersState;
        }

        private void RestorePlayers(JToken jToken)
        {
            if (!(jToken is JObject jObject)) return;

            AIGuildMember[] members = new AIGuildMember[players.Count];

            for (int i = 0; i < players.Count; i++)
            {
                string identifier = players[i].GetComponent<JsonSaveableEntity>().GetUniqueIdentifier();

                members[jObject[identifier]["ID"].ToObject<int>()] = players[i];
            }

            players.Clear();
            players.AddRange(members);
            curFormation = ((Formation)jObject["Formation"].ToObject<int>(), null);
            SetPlayerControl(jObject["Leader"].ToObject<int>());
        }

        private JToken CaptureInventory()
        {
            if (Players.Count == 0) return null;

            var itemsState = (JObject)players[0].CaptureInventory();

            int j = 0;

            while (itemsState.ContainsKey(j.ToString()))
            {
                int id = itemsState[j.ToString()]["Id"].ToObject<int>();
                var item = players[0].Inventory.GetItem(id);

                if (item is EquipeableItem equipeable)
                {
                    JObject equipmentState = new JObject();

                    for (int i = 0; i < players.Count; i++)
                    {
                        if (equipeable.Characters.Contains(players[i]))
                            equipmentState[i.ToString()] = true;
                    }

                    itemsState[j.ToString()]["Equipped"] = equipmentState;
                }

                j++;
            }

            return itemsState;
        }

        private void RestoreInventory(JToken jToken)
        {
            if (Players.Count == 0) return;

            if (!(jToken is JObject itemsState && itemsState != null)) return;

            players[0].RestoreInventory(jToken);

            int j = 0;

            while (itemsState.ContainsKey(j.ToString()))
            {
                var curItemState = itemsState[j.ToString()];
                int id = curItemState["Id"].ToObject<int>();

                AddItem(curItemState, id, curItemState["Count"].ToObject<int>());
                var item = players[0].Inventory.GetItem(id);

                if ((curItemState as JObject).ContainsKey("Equipped") && item is EquipeableItem equipeable)
                {
                    EquipItemToPlayers(curItemState, equipeable);
                }

                j++;
            }
            for (int i = 0; i < players.Count; i++)
            {
            }

            void AddItem(JToken curItemState, int id, int count)
            {
                for (int i = 0; i < count; i++)
                {
                    players[0].Inventory.Add(id);
                }
            }

            void EquipItemToPlayers(JToken curItemState, EquipeableItem equipeable)
            {
                int i = 0;
                var equipmentState = (JObject)curItemState["Equipped"];

                while (i < players.Count)
                {
                    if (equipmentState.ContainsKey(i.ToString()))
                    {
                        (players[i].Inventory as InventoryEquipDecorator).TryEquip(players[i], equipeable, out _);
                    }

                    i++;
                }
            }
        }
    }

    public interface IPlayable
    {
        bool IsControlled { get; set; }

        void EnableControll();
        void DisableControll();
    }

    public enum PlayerState
    {
        None,
        Combat,
        FollowPlayer,
        Patrol,
        Teleporting,
        Dead
    }
}

using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Control.AI;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Saving;
using Burmuruk.Tesis.Stats;
using Newtonsoft.Json.Linq;
using System;
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
                          select p).ToList();
            
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

            //players.ForEach(p => usedColors.AddVariable(p.statsList.Color));

            //var availableColors = (from color in playerColors where !usedColors.Contains(color) select color).ToList();

            //var selectedColorIdx = UnityEngine.Random.Range(0, availableColors.MaxCount);

            //member.statsList.Color = playerColors[selectedColorIdx];
        }

        public JToken CaptureAsJToken()
        {
            List<Vector3> positions = new();
            players.ForEach(player => { positions.Add(player.transform.position); });
            return JToken.FromObject(positions);
        }

        public void RestoreFromJToken(JToken state)
        {
            List<Vector3> positions = state.ToObject<List<Vector3>>();
            for (int i = 0; i < players.Count; i++)
            {
                players[i].transform.position = positions[i];
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

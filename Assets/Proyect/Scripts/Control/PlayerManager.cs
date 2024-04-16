using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Burmuruk.Tesis.Control
{
    class PlayerManager : MonoBehaviour
    {
        List<AIGuildMember> players;
        PlayerController playerController;
        int? m_CurPlayer;
        [SerializeField] string curPlayerName;
        (Formation value, object args) curFormation = default;

        public event Action OnPlayerChanged;
        public event Action OnFormationChanged;

        List<AIGuildMember> Players { get => players; }
        public Inventary playerInventary
        {
            get
            {
                if (!m_CurPlayer.HasValue)
                {
                    return null;
                }

                return players[m_CurPlayer.Value].Inventary;
            }
        }
        public Inventary MainInventary { get; private set; }
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

            var players = from p in FindObjectsOfType<AIGuildMember>() where p is IPlayable select p;
            this.players = players.ToList();

            playerController.OnFormationChanged += ChangeFormation;
        }

        private void Start()
        {
            SetPlayerControl();

            foreach (var player in players)
            {
                player.OnCombatStarted += EnterToCombatMode;
            }
        }

        public void SetPlayerControl(int idx)
        {
            if (players != null && idx < players.Count)
            {
                DisableRestOfPlayers(idx);

                players[idx].enabled = false;
                playerController.SetPlayer(players[idx]);

                m_CurPlayer = idx;
                curPlayerName = players[idx].name;

                OnPlayerChanged?.Invoke();
            }
        }

        private void DisableRestOfPlayers(int idx)
        {
            players.ForEach(p =>
            {
                p.enabled = true;
                p.SetMainPlayer(players[idx]);
                p.Fellows = players.Where(fellow => fellow != p && p != players[idx]).ToArray();
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

            foreach (var player in players)
            {
                if (player.enabled)
                {
                    player.SetFormation(formation, args);
                }
            }

            curFormation = (formation, args);
            OnFormationChanged?.Invoke();
        }

        private void EnterToCombatMode()
        {
            players.ForEach((player) => { player.State = PlayerState.Combat; });
        }

        public void AddMember(AIGuildMember member)
        {
            member.OnCombatStarted += EnterToCombatMode;
            member.SetFormation(curFormation.value, curFormation.args);
        }

        public void RemoveMember(AIGuildMember member)
        {
            member.OnCombatStarted -= EnterToCombatMode;
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

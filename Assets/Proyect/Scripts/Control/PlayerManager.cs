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

        public event Action OnPlayerChanged;

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

        private void ChangeFormation(Vector2 value)
        {
            foreach (var player in players)
            {
                if (player.enabled)
                {
                    player.SetFormation(value);
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
}

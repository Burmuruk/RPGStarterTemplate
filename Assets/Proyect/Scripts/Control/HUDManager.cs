using Burmuruk.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Burmuruk.Tesis.Control
{
    public class HUDManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] PlayerManager playerManager;
        PlayerController playerController;

        [Header("HUD Elements")]
        [SerializeField] GameObject pAbilites;
        [SerializeField] GameObject pAbilities;
        [SerializeField] GameObject pInteractables;
        [SerializeField] PopUp pFormation;
        [SerializeField] PopUp pLife;
        [SerializeField] PopUp pNotifications;
        [SerializeField] PopUp pMissions;

        State state;
        PopUps activePopUps;
        CoolDownAction cdFormations;
        CoolDownAction cdMissions;

        Queue<GameObject> lifeBarPool;

        enum State
        {
            None,
            HUD,
            MainMenu,
            Inventory,
        }

        [Flags]
        enum PopUps
        {
            None,
            Formation,
            Interactable,
            Life,
            Abilities,
            Notification,
            Mission,
            Damage,
            Effects
        }
        enum LifeBarType
        {
            None,
            Player,
            Guild,
            HurtedGuild,
            Enemies
        }

        [Serializable]
        record PopUp
        {
            public GameObject container;
            public TMPro.TextMeshPro text;
            public float showingTime;
        }

        private void Start()
        {
            FillLifePool();

            cdFormations = new CoolDownAction(pFormation.showingTime, (args) => { pFormation.container.SetActive(false); });
        }

        private void OnEnable()
        {
            playerManager.OnFormationChanged += ChangeFormation;
            playerController.OnInteractableEnter += () => ShowInteractionButton(true);
            playerController.OnInteractableEnter += () => ShowInteractionButton(false);
            //playerManager. combat mode -> abilities
        }

        private void LateUpdate()
        {
            
        }

        private void ChangeFormation()
        {
            if (!cdFormations.CanUse) return;

            StartCoroutine(cdFormations.CoolDown());

            pFormation.container.SetActive(true);
            pFormation.text.text = playerManager.CurFormation.value switch
            {
                Formation.Protect => "Protejer",
                Formation.Free => "Libre",
                Formation.LockTarget => "Fija objetivo",
                Formation.Follow => "Seguir",
                _ => pFormation.text.text
            };
        }

        private void ShowInteractionButton(bool shouldShow)
        {
            pInteractables.SetActive(shouldShow);
        }

        private void FillLifePool()
        {
            lifeBarPool = new();

            for (int i = 0; i < 30; i++)
            {
                lifeBarPool.Enqueue(Instantiate(pLife.container, pLife.container.transform.parent)); 
            }
        }
    }
}

using Burmuruk.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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
        [SerializeField] GameObject pFormation;
        [SerializeField] GameObject pFormationInfo;
        [SerializeField] GameObject pFormationState;
        [SerializeField] PopUp pLife;
        [SerializeField] PopUp pNotifications;
        [SerializeField] PopUp pMissions;

        State state;
        PopUps activePopUps;
        CoolDownAction cdChangeFormation;
        CoolDownAction cdMissions;
        List<CoolDownAction> cdNotifications;

        Queue<GameObject> lifeBarPool;
        Queue<PopUp> notificationsPool;

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
            CreateNotificationsPool();

            cdChangeFormation = new CoolDownAction(2, ShowFormations );
        }

        private void OnEnable()
        {
            playerController.OnFormationHold += ShowFormations;
            playerManager.OnFormationChanged += ChangeFormation;
            playerController.OnInteractableEnter += () => ShowInteractionButton(true);
            playerController.OnInteractableEnter += () => ShowInteractionButton(false);
            playerController.OnItemPicked += () => ShowNotification("Objeto recogido");
            //playerManager. combat mode -> abilities
        }

        private void LateUpdate()
        {
            playerController.OnFormationHold -= ShowFormations;
            playerManager.OnFormationChanged -= ChangeFormation;
            playerController.OnInteractableEnter -= () => ShowInteractionButton(true);
            playerController.OnInteractableEnter -= () => ShowInteractionButton(false);
            playerController.OnItemPicked -= () => ShowNotification("Objeto recogido");
        }

        private void ShowFormations(bool value)
        {
            if (value)
            {
                if (!cdChangeFormation.CanUse) return;

                StopCoroutine(cdChangeFormation.CoolDown());
                pFormation.SetActive(true);
                pFormationInfo.SetActive(true);
                pFormationState.SetActive(false);
            }
            else
            {
                pFormation.SetActive(true);
                pFormationInfo.SetActive(true);
                pFormationState.SetActive(false);
            }
        }

        private void ChangeFormation()
        {
            if (!cdChangeFormation.CanUse) return;

            StartCoroutine(cdChangeFormation.CoolDown());

            pFormation.SetActive(true);
            //pFormationState.tex = playerManager.CurFormation.value switch
            //{
            //    Formation.Protect => "Protejer",
            //    Formation.Free => "Libre",
            //    Formation.LockTarget => "Fija objetivo",
            //    Formation.Follow => "Seguir",
            //    _ => pFormation.text.text
            //};
        }

        private void ShowNewFormation()
        {

        }

        private void ShowInteractionButton(bool shouldShow)
        {
            pInteractables.SetActive(shouldShow);
        }

        private void ShowNotification(string message)
        {

        }

        private void FillLifePool()
        {
            if (pLife == null) return;

            lifeBarPool = new();

            for (int i = 0; i < 20; i++)
            {
                lifeBarPool.Enqueue(Instantiate(pLife.container, pLife.container.transform.parent)); 
            }
        }

        private void CreateNotificationsPool()
        {
            if (pLife == null) return;

            notificationsPool = new();

            for (int i = 0; i < 10; i++)
            {
                var notification = Instantiate(pNotifications.container, pLife.container.transform.parent);
                var newPopUp = pNotifications with
                {
                    container = notification,
                    text = notification.GetComponent<TextMeshPro>(),
                };

                notificationsPool.Enqueue(newPopUp);
                //cdNotifications.Add(new())
            }
        }
    }
}

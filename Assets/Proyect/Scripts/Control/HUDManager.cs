using Burmuruk.Tesis.Stats;
using Burmuruk.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Profiling;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace Burmuruk.Tesis.Control
{
    public class HUDManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] PlayerManager playerManager;
        PlayerController playerController;

        [Space()]
        [Header("Abilities"), Space()]
        [SerializeField] GameObject pAbilities;
        [SerializeField] GameObject pAbility;
        [Header("Formations"), Space()]
        [SerializeField] GameObject pFormationInfo;
        [SerializeField] StackableLabel pFormationState;
        [Header("Interactables"), Space()]
        [SerializeField] StackableLabel pInteractable;
        [Header("Notifications"), Space()]
        [SerializeField] StackableLabel pNotifications;
        [Header("Missions"), Space()]
        [SerializeField] StackableLabel pMissions;
        [Header("Life"), Space()]
        [SerializeField] StackableLabel pLife;

        State state;
        //PopUps activePopUps;
        CoolDownAction cdChangeFormation;
        CoolDownAction cdFormationInfo;
        Queue<CoolDownAction> cdMissions;
        List<(StackableNode node, CoolDownAction coolDown)> cdNotifications = new();
        public FormationState formationState = FormationState.None;

        Dictionary<GameObject, StackableNode> playersLife;

        enum State
        {
            None,
            HUD,
            MainMenu,
            Inventory,
        }
        public enum FormationState
        {
            None,
            Showing,
            Explaining,
            Changing
        }

        [Flags]
        enum HUDElements
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
        record StackableLabel
        {
            public GameObject container;
            public StackableNode node;
            public float showingTime;
            public int amount;
            public int maxAmount;
            public bool instanciateParent = false;

            private ObjectPool<StackableNode> pool;
            public List<StackableNode> activeNodes {  get; private set; }

            public void Initialize()
            {
                pool = new(CreateElement, GetElement, ReleaseElement, RemoveElement, defaultCapacity: amount, maxSize: maxAmount);
                activeNodes = new List<StackableNode>();
            }

            private void ReleaseElement(StackableNode node)
            {
                node.label.transform.parent.gameObject.SetActive(false);
            }

            public StackableNode Get()
            {
                return pool.Get();
            }

            public void Release(int idx = 0)
            {
                try
                {
                    Release(activeNodes[idx]);
                }
                catch (ArgumentOutOfRangeException)
                {

                }
            }

            public void Release(StackableNode node)
            {
                pool.Release(node);
                activeNodes.Remove(node);
            }

            private void GetElement(StackableNode node)
            {
                node.label.transform.parent.gameObject.SetActive(true);
                activeNodes.Add(node);
            }

            private StackableNode CreateElement()
            {
                if (pool.CountAll <= 0)
                    return node;

                GameObject newLabel;

                if (instanciateParent)
                {
                    newLabel = Instantiate(container, container.transform.parent);
                }
                else 
                {
                    newLabel = Instantiate(node.label.transform.parent.gameObject, node.label.transform.parent.parent);
                }

                StackableNode newNode = node with
                {
                    label = newLabel.GetComponentInChildren<TextMeshProUGUI>(),
                    image = newLabel.GetComponentInChildren<Image>()
                };

                return newNode;
            }

            private void RemoveElement(StackableNode node)
            {
                Destroy(node.label);
            }
        }

        [Serializable]
        record StackableNode
        {
            public TMPro.TextMeshProUGUI label;
            public Image image;
        }

        private void Awake()
        {
            playerController = FindObjectOfType<PlayerController>();
            playerManager = FindObjectOfType<PlayerManager>();
        }

        private void Start()
        {
            cdChangeFormation = new CoolDownAction(2, (value) => {
                formationState = FormationState.Showing;
                ShowFormations(!value);
            });
            cdFormationInfo = new CoolDownAction(.5f, EnableFormationsInfo);
            cdMissions = new Queue<CoolDownAction>();

            pFormationState.Initialize();
            pInteractable.Initialize();
            pNotifications.Initialize();
            pMissions.Initialize();
            pLife.Initialize();

            playersLife = new();
            foreach (var player in playerManager.Players)
            {
                StackableNode lifeBar = pLife.Get();
                playersLife.Add(player.gameObject, lifeBar);

                lifeBar.label.transform.parent.position = Camera.main.WorldToScreenPoint(player.transform.position);
                UpdateHealth(player.stats.Hp * 100 / player.stats.MaxHp, player.gameObject);
            }
        }

        private void OnEnable()
        {
            playerManager.OnCombatEnter += ShowAbilities;
            playerManager.OnFormationChanged += ChangeFormation;
            playerController.OnFormationHold += ShowFormations;
            playerController.OnPickableEnter += ShowInteractionButton;
            playerController.OnPickableExit += ShowInteractionButton;
            playerController.OnItemPicked += ShowNotification;
            //playerManager. combat mode -> abilities

            foreach (var player in playerManager.Players)
            {
                player.stats.OnDamage += (hp) => { UpdateHealth(hp, player.gameObject); };
            }
        }

        private void OnDisable()
        {
            playerManager.OnFormationChanged -= ChangeFormation;
            playerController.OnFormationHold -= ShowFormations;
            playerController.OnPickableEnter -= ShowInteractionButton;
            playerController.OnPickableExit -= ShowInteractionButton;
            playerController.OnItemPicked -= ShowNotification;

            foreach (var player in playerManager.Players)
            {
                player.stats.OnDamage -= (hp) => { UpdateHealth(hp, player.gameObject); };
            }
        }

        private void LateUpdate()
        {
            UpdateHealthPosition();
        }

        private void UpdateHealthPosition()
        {
            if (playersLife.Count <= 0) return;

            foreach (var player in playerManager.Players)
            {
                var position = Vector3.Lerp(playersLife[player.gameObject].label.transform.parent.position,
                    Camera.main.WorldToScreenPoint(player.transform.position + Vector3.up * 2),
                    Time.deltaTime * 20);

                playersLife[player.gameObject].label.transform.parent.position = position;
            }
        }

        private void UpdateHealth(float hp, GameObject player)
        {
            playersLife[player].image.fillAmount = hp / 100;
        }

        private void ShowFormations(bool value)
        {
            if (value)
            {
                if (formationState == FormationState.Changing)
                {
                    StopCoroutine(cdChangeFormation.CoolDown());
                    cdChangeFormation.Restart();

                    return;
                }

                pFormationState.container.SetActive(true);
                UpdateFormationText();
                pFormationInfo.SetActive(false);
                
                if (cdFormationInfo.CanUse)
                    StartCoroutine(cdFormationInfo.CoolDown());

                formationState = FormationState.Showing;
            }
            else
            {
                if (formationState == FormationState.Changing) return;
                
                StopCoroutine(cdFormationInfo.CoolDown());
                pFormationState.container.SetActive(false);
                pFormationState.Release();

                formationState = FormationState.None;
            }
        }

        private void ChangeFormation()
        {
            if (formationState == FormationState.Changing)
            {
                StopCoroutine(cdChangeFormation.CoolDown());
                cdChangeFormation.Restart();
            }

            StartCoroutine(cdChangeFormation.CoolDown());

            UpdateFormationText();
            formationState = FormationState.Changing;
        }

        private void EnableFormationsInfo(bool value)
        {
            if (formationState == FormationState.Changing)
                return;

            pFormationInfo.SetActive(value);
        }

        private void UpdateFormationText()
        {
            var newText = playerManager.CurFormation.value switch
            {
                Formation.Protect => "Protejer",
                Formation.Free => "Libre",
                Formation.LockTarget => "Fija objetivo",
                Formation.Follow => "Seguir",
                _ => "Libre"
            };

            StackableNode node;

            if (pFormationState.activeNodes.Count > 0)
            {
                node = pFormationState.activeNodes[0];
            }
            else
            {
                node = pFormationState.Get();
            }

            node.label.text = newText;
        }

        private void ShowMission(string[] missions)
        {
            if (!pMissions.container.activeSelf)
                pMissions.container.SetActive(true);

            foreach (var mission in missions)
            {
                var node = pMissions.Get();

                node.label.text = mission;
                cdMissions.Enqueue(new CoolDownAction(5, ReleaseMission));
            }
        }

        private void ReleaseMission(bool value)
        {
            pMissions.Release();

            if (pMissions.activeNodes.Count <= 0)
            {
                pMissions.container.SetActive(false);
            }
        }

        private void ShowInteractionButton(bool shouldShow, string name)
        {
            if (shouldShow)
            {
                StackableNode node;

                if (pInteractable.activeNodes.Count < 1)
                {
                    node = pInteractable.Get();
                }
                else
                    node = pInteractable.activeNodes[0];

                node.label.text = name;
            }
            else
            {
                pInteractable.Release();
            }
        }

        private void ShowAbilities(bool value)
        {
            if (value)
            {
                //playerManager.CurPlayer.

                //for (int i = 0; i < pAbilities.transform.childCount; i++)
                //{
                //    var pAbilty = pAbilities.transform.GetChild(i);

                //    pAbilities.SetActive(true);
                //}
            }

            pAbilities.SetActive(value);
        }

        private void ShowNotification(string itemName, Vector3 itemPosition)
        {
            if (!pNotifications.container.activeSelf)
            {
                pNotifications.container.transform.position = Camera.main.WorldToScreenPoint(itemPosition);
                pNotifications.container.SetActive(true);
            }

            var panel = pNotifications.Get();
            var coolDown = new CoolDownAction(pNotifications.showingTime,
                (value) => HideNotification(panel));

            cdNotifications.Add((panel, coolDown));

            StartCoroutine(coolDown.CoolDown());
        }

        private void HideNotification(StackableNode node)
        {
            pNotifications.Release(node);

            if (pNotifications.activeNodes.Count == 0)
                pNotifications.container.SetActive(false);
        }
    }
}

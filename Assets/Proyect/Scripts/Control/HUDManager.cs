using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Stats;
using Burmuruk.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Burmuruk.Tesis.UI
{
    public class HUDManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] PlayerManager playerManager;
        PlayerController playerController;

        [Space()]
        [Header("Abilities"), Space()]
        [SerializeField] GameObject pActiveAbilities;
        [SerializeField] GameObject pPasiveAbilities;
        [SerializeField] Sprite defaultAbilityIMG;
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

        Dictionary<AIGuildMember, StackableNode> playersLife;

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

        private void Awake()
        {
            playerController = FindObjectOfType<PlayerController>();
            playerManager = FindObjectOfType<PlayerManager>();
        }

        private void Start()
        {
            cdChangeFormation = new CoolDownAction(2, (value) =>
            {
                formationState = FormationState.Showing;
                ShowFormations(!value);
            });
            cdFormationInfo = new CoolDownAction(.5f, EnableFormationsInfo);
            cdMissions = new Queue<CoolDownAction>();

            InitializeStackables();
            CreateHPPlayersBar();
        }

        private void InitializeStackables()
        {
            pFormationState.Initialize();
            pInteractable.Initialize();
            pNotifications.Initialize();
            pMissions.Initialize();
            pLife.Initialize();
        }

        private void CreateHPPlayersBar()
        {
            playersLife = new();
            foreach (var player in playerManager.Players)
            {
                StackableNode lifeBar = pLife.Get();
                playersLife.Add(player, lifeBar);

                lifeBar.label.transform.parent.position = Camera.main.WorldToScreenPoint(player.transform.position);
                UpdateHealth(player.stats.Hp * 100 / player.stats.MaxHp, player);
                lifeBar.label.transform.parent.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            playerManager.OnCombatEnter += EnableHPPlayersBar;
            playerManager.OnCombatEnter += ShowAbilities;
            playerManager.OnFormationChanged += ChangeFormation;
            playerController.OnFormationHold += ShowFormations;
            playerController.OnPickableEnter += ShowInteractionButton;
            playerController.OnPickableExit += ShowInteractionButton;
            playerController.OnItemPicked += ShowNotification;
            //playerManager. combat mode -> abilities

            foreach (var player in playerManager.Players)
            {
                player.stats.OnDamage += (hp) => { UpdateHealth(hp, player); };
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
                player.stats.OnDamage -= (hp) => { UpdateHealth(hp, player); };
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
                var position = Vector3.Lerp(playersLife[player].label.transform.parent.position,
                    Camera.main.WorldToScreenPoint(player.transform.position + Vector3.up * 2),
                    Time.deltaTime * 20);

                playersLife[player].label.transform.parent.position = position;
            }
        }

        private void UpdateHealth(float hp, AIGuildMember player)
        {
            playersLife[player].image.fillAmount = hp / 100;

            if (hp <= player.stats.MaxHp * .7f)
                playersLife[player].image.transform.parent.gameObject.SetActive(true);
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
                var inventary = playerManager.MainInventory;
                var curPlayer = playerManager.CurPlayer.GetComponent<Character>();

                var abilities = (from ability in inventary.GetOwnedList(ItemType.Ability)
                                 where ((EquipeableItem)ability).Characters.Contains(curPlayer)
                                 select (Ability)inventary.GetOwnedItem(ability.ID))
                             .ToArray();

                int j = 0;
                for (int i = 0; i < pActiveAbilities.transform.childCount; i++)
                {
                    var imgAbilty = pActiveAbilities.transform.GetChild(i).GetComponent<Image>();

                    if (j < abilities.Length)
                    {
                        int id = abilities[j].ID;
                        var button = imgAbilty.GetComponent<MyItemButton>();

                        imgAbilty.sprite = abilities[j].Sprite;
                        button.onClick.RemoveAllListeners();
                        button.onClick.AddListener(() => UseAbility(id));

                        j++;
                        continue;
                    }

                    imgAbilty.sprite = defaultAbilityIMG;
                    imgAbilty.gameObject.SetActive(false);
                }
            }

            pActiveAbilities.transform.parent.gameObject.SetActive(value);

        }

        private void UseAbility(int id)
        {
            playerManager.UseItem(id);
        }

        private void EnableHPOnDamage()
        {

        }

        private void EnableHPPlayersBar(bool enable)
        {
            foreach (var bar in playersLife)
            {
                bar.Value.label.transform.parent.gameObject.SetActive(enable);
            }
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

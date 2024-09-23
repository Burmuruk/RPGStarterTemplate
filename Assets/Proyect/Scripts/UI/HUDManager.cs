using Burmuruk.Tesis.Combat;
using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Control.AI;
using Burmuruk.Tesis.Inventory;
using Burmuruk.Tesis.Saving;
using Burmuruk.Utilities;
using System;
using System.Collections;
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
        [SerializeField] Camera mainCamera;
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

        [Space, Header("Saving")]
        [SerializeField] Image imgSaving;

        State state;
        //PopUps activePopUps;
        Coroutine savingNotification;
        CoolDownAction cdChangeFormation;
        CoolDownAction cdFormationInfo;
        Queue<CoolDownAction> cdMissions;
        List<(StackableNode node, CoolDownAction coolDown)> cdNotifications = new();
        public FormationState formationState = FormationState.None;
        bool hasInitialized = false;

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
            var savingWrapper = FindObjectOfType<JsonSavingWrapper>();
            if (savingWrapper)
            {
                savingWrapper.OnSaving += ShowSavingIcon;
                savingWrapper.OnLoading += ShowSavingIcon;
            }
        }

        private void Start()
        {
            
        }

        private void InitializeStackables()
        {
            pFormationState.Initialize();
            pInteractable.Initialize();
            pNotifications.Initialize();
            pMissions.Initialize();
            pLife.Initialize();
        }

        public void CreateHPPlayersBar()
        {
            playersLife = new();
            foreach (var player in playerManager.Players)
            {
                StackableNode lifeBar = pLife.Get();
                playersLife.Add(player, lifeBar);

                lifeBar.label.transform.parent.position = mainCamera.WorldToScreenPoint(player.transform.position);
                UpdateHealth(player.Health.HP, player);
                lifeBar.label.transform.parent.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (!hasInitialized) { return; }

            UpdateSubscripttions();
        }

        private void OnDisable()
        {
            if (!hasInitialized) { return; }

            playerManager.OnCombatEnter -= EnableHPPlayersBar;
            playerManager.OnCombatEnter -= ShowAbilities;
            playerManager.OnFormationChanged -= ChangeFormation;
            playerController.OnFormationHold -= ShowFormations;
            playerController.OnPickableEnter -= ShowInteractionButton;
            playerController.OnPickableExit -= ShowInteractionButton;
            playerController.OnItemPicked -= ShowNotification;

            foreach (var player in playerManager.Players)
            {
                player.Health.OnDamaged -= (hp) => { UpdateHealth(hp, player); };
            }
        }

        private void LateUpdate()
        {
            //return;
            UpdateHealthPosition();
        }

        private void UpdateSubscripttions()
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
                player.Health.OnDamaged += (hp) => { UpdateHealth(hp, player); };
            }
        }

        private void UpdateHealthPosition()
        {
            if (playersLife.Count <= 0) return;

            foreach (var player in playerManager.Players)
            {
                var position = Vector3.Lerp(playersLife[player].label.transform.parent.position,
                    mainCamera.WorldToScreenPoint(player.transform.position + Vector3.up * 2),
                    Time.deltaTime * 20);

                playersLife[player].label.transform.parent.position = position;
            }
        }

        public void Init()
        {
            cdChangeFormation = new CoolDownAction(2, (value) =>
            {
                formationState = FormationState.Showing;
                ShowFormations(!value);
            });
            cdFormationInfo = new CoolDownAction(.5f, EnableFormationsInfo);
            cdMissions = new Queue<CoolDownAction>();
            mainCamera = Camera.main;

            InitializeStackables();
            CreateHPPlayersBar();
            hasInitialized = true;

            UpdateSubscripttions();
            DontDestroyOnLoad(transform.parent.gameObject);
        }

        public void RestartPlayersTags()
        {
            CreateHPPlayersBar();
        }

        private void UpdateHealth(float hp, AIGuildMember player)
        {
            playersLife[player].image.fillAmount = hp / player.Health.MaxHp;

            if (hp <= player.Health.MaxHp )
                playersLife[player].image.transform.parent.parent.gameObject.SetActive(true);
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

        private void ShowAbilities(bool shouldShow)
        {
            if (shouldShow)
            {
                var inventory = playerManager.MainInventory;
                var curPlayer = playerManager.CurPlayer.transform.GetComponent<Character>();
                Ability[] abilities = GetEquippedAbilitites(inventory, curPlayer);

                for (int i = 0, j = 0; i < pActiveAbilities.transform.childCount; i++)
                {
                    var imgAbilty = pActiveAbilities.transform.GetChild(i).GetComponent<Image>();

                    if (j < abilities.Length)
                    {
                        SetupAbilityButton(abilities, j++, imgAbilty);

                        continue;
                    }

                    imgAbilty.sprite = defaultAbilityIMG;
                    imgAbilty.gameObject.SetActive(false);
                }
            }

            pActiveAbilities.transform.parent.gameObject.SetActive(shouldShow);

            void SetupAbilityButton(Ability[] abilities, int j, Image imgAbilty)
            {
                int id = abilities[j].ID;
                var button = imgAbilty.GetComponent<MyItemButton>();

                imgAbilty.sprite = abilities[j].Sprite;

                imgAbilty.gameObject.SetActive(true);
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => UseAbility(id));
            }

            Ability[] GetEquippedAbilitites(IInventory inventory, Character curPlayer)
            {
                return (from ability in inventory.GetList(ItemType.Ability)
                        where ((EquipeableItem)ability).Characters.Contains(curPlayer)
                        select (Ability)inventory.GetItem(ability.ID))
                                             .ToArray();
            }
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
                pNotifications.container.transform.position = mainCamera.WorldToScreenPoint(itemPosition);
                pNotifications.container.SetActive(true);
            }
            
            var panel = pNotifications.Get();
            panel.label.text = itemName;

            var coolDown = new CoolDownAction(pNotifications.showingTime,
                (_) => { HideNotification(panel); });

            cdNotifications.Add((panel, coolDown));

            StartCoroutine(coolDown.CoolDown());
        }

        private void HideNotification(StackableNode node)
        {
            pNotifications.Release(node);

            if (pNotifications.activeNodes.Count == 0)
                pNotifications.container.SetActive(false);
        }

        private void ShowSavingIcon(float progress)
        {
            if (savingNotification != null && progress < 1)
                return;
            else if (progress >= 1)
            {
                Invoke("StopSavingNotification", 2);
                return;
            }
            
            savingNotification = StartCoroutine(RotateSavingImage());
        }

        private void StopSavingNotification()
        {
            StopCoroutine(savingNotification);
            imgSaving.gameObject.SetActive(false);
            imgSaving.transform.rotation = Quaternion.identity;
            
            savingNotification = null;
        }
            

        private IEnumerator RotateSavingImage()
        {
            float maxTime = 30;
            float curTime = 0;

            imgSaving.gameObject.SetActive(true);

            while (curTime < maxTime)
            {
                yield return new WaitForEndOfFrame();

                imgSaving.transform.Rotate(Vector3.forward, 2);
            }
        }
    }
}

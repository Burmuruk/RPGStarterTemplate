using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Fighting;
using Burmuruk.Tesis.Stats;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Burmuruk.Tesis.UI
{
    public class UIMenuCharacters : MonoBehaviour
    {
        #region Variables
        [SerializeField] StackableLabel elementPanel;
        [SerializeField] TextMeshProUGUI txtExtraInfo;
        [SerializeField] GameObject characterModel;
        [SerializeField] Image[] playersImg;
        [SerializeField] float rotationVelocity;
        [SerializeField] TextMeshProUGUI txtWarning;
        [SerializeField] MyItemButton[] WarningButtons;
        [SerializeField] GameObject AmountPanel;
        [SerializeField] GameObject colorsPanel;
        [SerializeField] PlayerCustomization customization;
        [SerializeField] Image[] btnModificationSlots;
        [SerializeField] Image[] btnHumanAbilities;
        [SerializeField] Image[] btnAlienAbilities;
        [SerializeField] Image[] btnAbilitiesSlot;
        [SerializeField] Color selectedColor;
        [SerializeField] Sprite defaultBTNSprite;
        [SerializeField] ItemsList itemsList;

        PlayerCustomizationManager clothingManager;

        public enum State
        {
            None,
            Notice,
            Loading
        }

        public State curState = State.None;
        Vector2 direction;
        int curPlayerIdx;
        int curTabIdx;
        bool showDescription = false;
        int curElementId = 0;
        int curBtnId = 0;
        int? curModificationSlot;
        int? curAbiltySlot;

        List<AIGuildMember> players;
        InventaryTab curInventaryTab;
        IInventary inventary;
        WarningProblem curWarningProblem;
        MyItemButton[] btnColors;
        Dictionary<int, Image> btnColorsDict;
        Dictionary<int, (Image image, int subType)> btnAbilitiesDict;
        Dictionary<int, (Image image, int subType)> btnAbilitiesSlotDict;
        Dictionary<int, (Image image, int subType)> btnModsDict;
        Dictionary<int, (StackableNode panel, EquipedItem item, ISaveableItem realItem)> curElementLabels = new();

        enum InventaryTab
        {
            None,
            Modifications,
            Weapons,
            Inventary
        }

        enum WarningProblem
        {
            None,
            EquipEquiped,
            RemoveEquiped
        }
        #endregion

        #region Unity Methods
        private void Awake()
        {
            elementPanel.Initialize();
            WarningButtons[0].onClick.AddListener(AceptWarning);
            WarningButtons[1].onClick.AddListener(CancelWarning);

            InitializeHabilityButtons();
        }

        private void Start()
        {
            colorsPanel.SetActive(false);
        }

        private void Update()
        {
            Rotate();
        } 
        #endregion

        #region Elements panel
        public void ShowModifications()
        {
            if (curState != State.None) return;

            ShowElements(GetSortedItems(InventaryTab.Modifications));
            curInventaryTab = InventaryTab.Modifications;
        }

        public void ShowWeapons()
        {
            if (curState != State.None) return;

            ShowElements(GetSortedItems(InventaryTab.Weapons));
            curInventaryTab = InventaryTab.Weapons;
        }

        public void ShowInvantary()
        {
            if (curState != State.None) return;

            ShowElements(GetSortedItems(InventaryTab.Inventary));
            curInventaryTab = InventaryTab.Inventary;
        }

        public void SwitchExtraData()
        {
            if (curState != State.None) return;

            showDescription = !showDescription;
            ShowExtraData(showDescription);
        }

        public void SelectElement(int id)
        {
            if (curState != State.None) return;

            curElementId = id;
            ShowExtraData(showDescription);
        }

        public void ShowExtraData(bool showInfo)
        {
            if (showInfo)
            {
                txtExtraInfo.text = curElementLabels[curElementId].realItem.GetDescription();
            }
            else
            {
                txtExtraInfo.text = curElementLabels[curElementId].realItem.GetName();
            }
        }

        public void ElementAction(int idx)
        {
            switch ((curElementLabels[idx].item.Type))
            {
                case ItemType.Consumable:

                    break;

                case ItemType.Ability:
                case ItemType.Weapon:
                case ItemType.Modification:

                    EquipItem(idx);
                    break;

                default:
                    break;
            }
            print("Interacting");
        }

        public void ElementCancelAction(int idx)
        {
            switch ((curElementLabels[idx].item.Type))
            {
                case ItemType.Consumable:
                    break;

                case ItemType.Ability:
                case ItemType.Weapon:
                case ItemType.Modification:

                    UnEquipItem(idx);
                    break;

                default:
                    break;
            }

            print("Unequiping");
            txtWarning.transform.parent.gameObject.SetActive(false);
            curWarningProblem = WarningProblem.None;
            curState = State.None;
        }

        public void TryRemoveItem()
        {
            if (!curElementLabels.ContainsKey(curElementId)) return;

            var item = curElementLabels[curElementId].item;
            if (!inventary.Remove(item.Type, item.GetSubType())) return;

            if (inventary.GetItemCount(item.Type, item.GetSubType()) > 0)
            {
                SetElementInfo(curElementLabels[curElementId].panel, item, out _);
            }
            else
            {
                RemoveElement(curElementLabels[curElementId].panel);
            }
        }

        public void AceptWarning()
        {
            switch (curWarningProblem)
            {
                case WarningProblem.None:
                    break;
                case WarningProblem.EquipEquiped:
                    ChangeEquiped();
                    break;

                case WarningProblem.RemoveEquiped:
                    RemoveItem();
                    break;

                default:
                    break;
            }
        }

        private void ChangeEquiped()
        {
            var item = curElementLabels[curElementId].item;
            var lastPlayer = item.Characters.Last();
            var inventaryDecorator = (inventary as InventaryEquipDecorator);

            inventaryDecorator.Unequip(lastPlayer, item);
            inventaryDecorator.Equip(players[curPlayerIdx], item.Type, item.GetSubType());

            SetPlayersColors(item, curElementLabels[curElementId].panel);
            ShowCharacterModel();
        }

        private void RemoveItem()
        {
            var item = curElementLabels[curElementId].item;

            var lastPlayer = item.Characters.Last();
            var inventaryDecorator = (inventary as InventaryEquipDecorator);

            inventaryDecorator.Unequip(lastPlayer, item);
            ShowCharacterModel();

            inventary.Remove(item.Type, item.GetSubType());

            if (inventary.GetItemCount(item.Type, item.GetSubType()) > 0)
            {
                SetElementInfo(curElementLabels[curElementId].panel, item, out _);
            }
            else
            {
                RemoveElement(curElementLabels[curElementId].panel);
            }

            txtWarning.transform.parent.gameObject.SetActive(false);
            curWarningProblem = WarningProblem.None;
            curState = State.None;
        }

        private void EquipItem(int idx)
        {
            var item = curElementLabels[idx].item;
            (inventary as InventaryEquipDecorator).Equip(players[curPlayerIdx], item.Type, item.GetSubType());

            SetPlayersColors(curElementLabels[idx].item, curElementLabels[idx].panel);
            ShowCharacterModel();
        }

        private void UnEquipItem(int idx)
        {
            var item = curElementLabels[idx].item;
            (inventary as InventaryEquipDecorator).Unequip(players[curPlayerIdx], curElementLabels[idx].item);

            SetPlayersColors(curElementLabels[idx].item, curElementLabels[idx].panel);
            ShowCharacterModel();
        }

        private void ShowElements(List<ISaveableItem> items)
        {
            CleanElements();

            int i = 0;
            foreach (var item in items)
            {
                var panel = elementPanel.Get();

                EquipedItem equipedItem = null;
                SetElementInfo(panel, item, out equipedItem);

                int buttonId = i++;
                SubscribeToEvents(panel, buttonId);

                curElementLabels.Add(buttonId, (panel, equipedItem, inventary.GetItem(item.Type, item.GetSubType())));
            }

            curElementId = 0;
            ShowExtraData(true);

            void SubscribeToEvents(StackableNode panel, int buttonId)
            {
                var button = panel.label.transform.parent.GetComponent<MyItemButton>();
                button.SetId(buttonId);
                button.onClick.AddListener(() => { ElementAction(buttonId); });
                button.OnRightClick += () => ElementCancelAction(buttonId);
                button.OnPointerEnterEvent += SelectElement;
            }
        }

        private void SetElementInfo(StackableNode panel, ISaveableItem item, out EquipedItem equipedItem)
        {
            panel.label.text = item.GetName();
            equipedItem = inventary.GetOwnedItem(item.Type, item.GetSubType()) as EquipedItem;

            var txtCount = (from txt in panel.label.transform.GetComponentsInChildren<TextMeshProUGUI>()
                            where txt.gameObject != panel.label.gameObject
            select txt).First();
            txtCount.text = inventary.GetItemCount(item.Type, item.GetSubType()).ToString();

            SetPlayersColors(equipedItem, panel);
        }

        private void SetPlayersColors(EquipedItem equipedItem, StackableNode panel)
        {
            Image[] images = panel.image.transform.GetComponentsInChildren<Image>(true)
                .Where(image => image.transform != panel.image.transform)
                .OrderBy(i => i.transform.name)
                .ToArray();

            if (equipedItem is EquipedItem equiped && equiped.IsEquip)
            {

                Color[] colors = (from player in players
                                  let cur =
                                     from character in equiped.Characters
                                     where character.stats.ID == player.stats.ID
                                     select character
                                  where cur.Any()
                                  select cur.First().stats.Color)
                            .ToArray();

                for (int i = 0; i < images.Count(); i++)
                {
                    if (i < colors.Count())
                    {
                        images[i].gameObject.SetActive(true);
                        images[i].color = colors[i];
                    }
                    else
                    {
                        images[i].gameObject.SetActive(false);
                    }
                }
            }
            else
            {
                foreach (var image in images)
                {
                    image.gameObject.SetActive(false);
                }
            }
        }

        private void UpdatePlayersColors()
        {
            foreach (var element in curElementLabels.Values)
            {
                SetPlayersColors(element.item, element.panel);
            }
        }

        private void CleanElements()
        {
            while (elementPanel.activeNodes.Count > 0)
            {
                RemoveElement(elementPanel.activeNodes[0]);
            }

            curElementLabels.Clear();
        }

        private void RemoveElement(StackableNode node)
        {
            node.label.transform.parent.GetComponent<MyItemButton>().onClick.RemoveAllListeners();
            elementPanel.Release(node);
        }

        private List<ISaveableItem> GetSortedItems(InventaryTab tab)
        {
            List<ISaveableItem> items = null;

            ItemType type = tab switch
            {
                InventaryTab.Modifications => ItemType.Modification,
                InventaryTab.Weapons => ItemType.Weapon,
                _ => ItemType.None
            };

            if (type != ItemType.None)
                items = inventary.GetList(type);
            else
            {
                items = inventary.GetList(ItemType.Modification);
                inventary.GetList(ItemType.Consumable).ForEach(i => items.Add(i));
                inventary.GetList(ItemType.Weapon).ForEach(w => items.Add(w));
            }

            items.OrderBy(item => item.GetName());

            return items;
        }

        private void ShowEquipedWarning()
        {
            curState = State.Notice;
            if (txtWarning != null)
                txtWarning.text = "El objeto está actualmente equipado.\n¿Desea equiparlo?";
            else
            {
                txtWarning = GameObject.FindGameObjectsWithTag("Respawn")[0].GetComponent<TextMeshProUGUI>();
                txtWarning.text = "El objeto está actualmente equipado.\n¿Desea equiparlo?";
            }
            WarningButtons[0].transform.parent.gameObject.SetActive(true);
        }

        private void ShowDeleteEquipedWarning()
        {
            curState = State.Notice;
            txtWarning.text = "El objeto está actualmente equipado.\n¿Desea eliminarlo?";
            WarningButtons[0].transform.parent.gameObject.SetActive(true);
        }

        private void ShowAmountNotice()
        {

        }

        private void CancelWarning()
        {
            WarningButtons[0].transform.parent.gameObject.SetActive(false);
            curState = State.None;
            curWarningProblem = WarningProblem.None;
        }
        #endregion

        #region Characters panel
        public void ShowCharacters()
        {
            playersImg[0].color = players[GetNextPlayerIdx(-1)].stats.Color;
            playersImg[1].color = players[curPlayerIdx].stats.Color;
            playersImg[2].color = players[GetNextPlayerIdx(1)].stats.Color;

            ShowCharacterModel();
            UpdateModsSprites();
        }

        private void ShowCharacterModel()
        {
            RemovePreviousModel();

            var prefab = players[curPlayerIdx].BodyManager.GetPart(BodyManager.BodyPart.Body);
            var inst = Instantiate(prefab, characterModel.transform);
            inst.transform.localPosition = Vector3.zero;

            void RemovePreviousModel()
            {
                if (characterModel.transform.childCount > 0)
                {
                    Destroy(characterModel.transform.GetChild(0).gameObject);
                    //while (characterModel.transform.childCount > 0)
                    //{
                    //}
                }
            }
        }

        private int GetNextPlayerIdx(int idx)
        {
            if (players.Count == 1)
            {
                return curPlayerIdx;
            }
            else if (players.Count == 2)
            {
                return curPlayerIdx == 0 ? 1 : 0;
            }
            else if (players.Count > 2)
            {
                if (idx < 0)
                    return curPlayerIdx == 0 ? players.Count - 1 : curPlayerIdx - 1;
                else
                    return curPlayerIdx == players.Count ? 0 : curPlayerIdx + 1;
            }

            return 0;
        }

        public void ShowNextPlayer()
        {
            if (curState != State.None) return;

            curPlayerIdx = GetNextPlayerIdx(1);
            ShowCharacters();
        }

        public void ShowPreviourPlayer()
        {
            if (curState != State.None) return;

            curPlayerIdx = GetNextPlayerIdx(-1);
            ShowCharacters();
        }

        public void ChangePlayerColor()
        {
            colorsPanel.SetActive(!colorsPanel.activeSelf);
        }

        private void ChangeColor()
        {
            players[curPlayerIdx].stats.Color = btnColorsDict[curBtnId].color;

            ShowCharacters();
            colorsPanel.SetActive(false);
            InitializeColorButtons();
            UpdatePlayersColors();
        }

        public void SelectBtnColor(int id) => curBtnId = id;

        private void InitializeColorButtons()
        {
            btnColors = colorsPanel.GetComponentsInChildren<MyItemButton>(true);
            btnColorsDict = new();

            int btnId = 0;
            for (int i = 0; i < btnColors.Length; i++)
            {
                var btn = btnColors[i];

                if (i >= customization.Colors.Length || !VerifyColor(i))
                {
                    btn.gameObject.SetActive(false);
                    continue;
                }

                btn.SetId(btnId);
                btn.OnPointerEnterEvent += SelectBtnColor;
                btn.onClick.AddListener(ChangeColor);
                btnColorsDict.Add(btnId, btn.GetComponent<Image>());

                btnColorsDict[btnId].color = customization.Colors[i];
                btn.gameObject.SetActive(true);
                btnId++; 
            }

            bool VerifyColor(int idx)
            {
                foreach (var player in players)
                {
                    if (player.stats.Color == customization.Colors[idx])
                        return false;
                }

                return true;
            }
        }
        #endregion

        #region Initialization
        public void SetInventary(IInventary inventary)
        {
            this.inventary = inventary;
            var inventaryDecorator = inventary as InventaryEquipDecorator;
            inventaryDecorator.OnTryAlreadyEquiped += () =>
            {
                curWarningProblem = WarningProblem.EquipEquiped;
                ShowEquipedWarning();
            };
            inventaryDecorator.OnTryDeleteEquiped += () =>
            {
                curWarningProblem = WarningProblem.RemoveEquiped;
                ShowDeleteEquipedWarning();
            };
            curInventaryTab = InventaryTab.Modifications;

            ShowInvantary();
        }

        public void SetPlayers(List<AIGuildMember> players, PlayerCustomizationManager clothingManager)
        {
            this.players = players;

            for (int i = 0; i < players.Count; i++)
            {
                if (!players[i].enabled)
                {
                    curPlayerIdx = i;
                    break;
                }
            }

            this.clothingManager = clothingManager;
            ShowCharacters();
            InitializeColorButtons();
            //playersImg[1] = this.players[curPlayerIdx];
        }

        private void UpdateModsSprites()
        {
            var mods = (from mod in inventary.GetOwnedList(ItemType.Modification)
                       let equiped = (EquipedItem)mod
                       where equiped.IsEquip && equiped.Characters.Contains(players[curPlayerIdx])
                       select (Modification)inventary.GetItem(mod.Type, mod.GetSubType())
                       ).ToArray();

            int i = 0;
            foreach (var modSlot in btnModificationSlots)
            {
                for (; i < mods.Length; i++)
                {
                    modSlot.sprite = mods[i].Sprite;
                    continue;
                }

                modSlot.sprite = defaultBTNSprite;
            }
        }
        #endregion

        public void RotatePlayer(Vector2 direction)
        {
            this.direction = direction;
        }

        private void Rotate()
        {
            characterModel.transform.Rotate(Vector3.up, direction.x * rotationVelocity * -1);
        }

        #region Abilities
        private void InitializeHabilityButtons()
        {
            for (int i = 0; i < btnAbilitiesSlot.Length; i++)
            {
                int id = i;
                btnAbilitiesSlotDict.Add(id, (btnAbilitiesSlot[i], -1));
                AddAbilityListeners(btnAbilitiesSlot[i], SelectAbilitySlot, id);
            }

            var collection = btnHumanAbilities;

            for (int i = 0, j = 0, k = 0; i < collection.Count() && j < 2; i++, k++)
            {
                int id = k;
                AddAbilitiesImage(id, collection[i]);
                AddAbilityListeners(collection[i], SelectAbility, id);

                if (i == btnHumanAbilities.Count() - 1)
                {
                    collection = btnAlienAbilities;
                    j++;
                    i = 0;
                }
            }
        }

        private void AddAbilitiesImage(int id, Image image)
        {
            btnAbilitiesDict.Add(id, (image, GetAbilitiesSubType().Current));
        }

        private IEnumerator<int> GetAbilitiesSubType()
        {
            foreach (var type in Enum.GetValues(typeof(AbilityType)))
            {
                yield return (int)type;
            }
        }

        private void AddAbilityListeners(Image image, Action<int> listener, int id)
        {
            var button = image.transform.parent.GetComponent<MyItemButton>();
            button.onClick.AddListener(() => listener(id));
        }

        private void UnequipAbilty(int id)
        {

        }

        private void SelectAbilitySlot(int id)
        {
            if (curAbiltySlot.HasValue)
            {
                ChangeSelectedColor();
            }

            curAbiltySlot = id;
            ChangeSelectedColor();
        }

        private void SelectAbility(int id)
        {
            if (!curAbiltySlot.HasValue) return;

            var slotImage = btnAbilitiesDict[curAbiltySlot.Value].image;
            slotImage.sprite = btnAbilitiesDict[id].image.sprite;

            ((InventaryEquipDecorator)inventary).Equip(players[curPlayerIdx], ItemType.Ability, btnAbilitiesDict[id].subType);

            ChangeSelectedColor();
            curAbiltySlot = null;
        }

        private void ChangeSelectedColor()
        {
            var parentImage = btnAbilitiesDict[curAbiltySlot.Value].image.transform.parent.GetComponent<Image>();
            var prevColor = parentImage.color;
            parentImage.color = selectedColor;

            selectedColor = prevColor;
        }
        #endregion
    }
}

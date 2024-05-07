using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Stats;
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

        List<AIGuildMember> players;
        InventaryTab curInventaryTab;
        IInventary inventary;
        MyItemButton[] btnColors;
        Dictionary<int, Image> btnColorsDict;
        Dictionary<int, (StackableNode panel, EquipedItem item, ISaveableItem realItem)> curElementLabels = new();

        enum InventaryTab
        {
            None,
            Modifications,
            Weapons,
            Inventary
        }
        #endregion

        #region Unity Methods
        private void Awake()
        {
            elementPanel.Initialize();
            WarningButtons[0].onClick.AddListener(ChangeEquipedPlayer);
            WarningButtons[1].onClick.AddListener(CancelWarning);
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
        }

        public void ShowWeapons()
        {
            if (curState != State.None) return;

            ShowElements(GetSortedItems(InventaryTab.Weapons));
        }

        public void ShowInvantary()
        {
            if (curState != State.None) return;

            ShowElements(GetSortedItems(InventaryTab.Inventary));
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

        private void EquipItem(int idx)
        {
            var item = curElementLabels[idx].item;
            (inventary as InventaryEquipDecorator).Equip(players[curPlayerIdx], item.Type, item.GetSubType());

            SetPlayersColors(curElementLabels[idx].item, curElementLabels[idx].panel);
            ShowCharacterModel();
        }

        public void ChangeEquipedPlayer()
        {
            var item = curElementLabels[curElementId].item;
            var lastPlayer = item.Characters.Last();
            var inventaryDecorator = (inventary as InventaryEquipDecorator);

            inventaryDecorator.Unequip(lastPlayer, item);
            inventaryDecorator.Equip(players[curPlayerIdx], item.Type, item.GetSubType());

            SetPlayersColors(item, curElementLabels[curElementId].panel);
            txtWarning.transform.parent.gameObject.SetActive(false);
            curState = State.None;
        }

        private void ShowElements(List<ISaveableItem> items)
        {
            CleanElements();

            int i = 0;
            foreach (var item in items)
            {
                var panel = elementPanel.Get();
                panel.label.text = item.GetName();
                var equipedItem = inventary.GetOwnedItem(item.Type, item.GetSubType()) as EquipedItem;

                var txtCount = (from txt in panel.label.transform.GetComponentsInChildren<TextMeshProUGUI>()
                               where txt.gameObject != panel.label.gameObject
                               select txt).First();
                txtCount.text = inventary.GetItemCount(item.Type, item.GetSubType()).ToString();

                int buttonId = i++;
                SubscribeToEvents(panel, buttonId);

                curElementLabels.Add(buttonId, (panel, equipedItem, inventary.GetItem(item.Type, item.GetSubType())));

                SetPlayersColors(equipedItem, panel);
            }

            curElementId = 0;
            ShowExtraData(true);

            void SubscribeToEvents(StackableNode panel, int buttonId)
            {
                var button = panel.label.transform.parent.GetComponent<MyItemButton>();
                button.SetId(buttonId);
                button.onClick.AddListener(() => { ElementAction(buttonId); });
                button.OnPointerEnterEvent += SelectElement;
            }
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
                                  where cur.Count() > 0
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

        private void CleanElements()
        {
            while (elementPanel.activeNodes.Count > 0)
            {
                elementPanel.Release(elementPanel.activeNodes[0]);
                elementPanel.activeNodes[0].label.transform.parent.GetComponent<MyItemButton>().onClick.RemoveAllListeners();
            }
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
            txtWarning.text = "El objeto está actualmente equipado.\n¿Desea equiparlo?";
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
        }
        #endregion

        #region Characters panel
        public void ShowCharacters()
        {
            clothingManager.EquipModifications(players[curPlayerIdx]);

            playersImg[0].color = players[GetNextPlayerIdx(-1)].stats.Color;
            playersImg[1].color = players[curPlayerIdx].stats.Color;
            playersImg[2].color = players[GetNextPlayerIdx(1)].stats.Color;

            ShowCharacterModel();
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
            inventaryDecorator.OnTryAlreadyEquiped += ShowEquipedWarning;
            inventaryDecorator.OnTryDeleteEquiped += ShowDeleteEquipedWarning;
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
        #endregion

        public void RotatePlayer(Vector2 direction)
        {
            this.direction = direction;
        }

        private void Rotate()
        {
            characterModel.transform.Rotate(Vector3.up, direction.x * rotationVelocity * -1);
        }
    }
}

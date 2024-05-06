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
        [SerializeField] StackableLabel elementPanel;
        [SerializeField] TextMeshProUGUI txtExtraInfo;
        [SerializeField] GameObject characterModel;
        [SerializeField] Image[] playersImg;
        [SerializeField] float rotationVelocity;
        [SerializeField] TextMeshProUGUI txtWarning;
        [SerializeField] MyItemButton[] WarningButtons;
        [SerializeField] GameObject AmountPanel;
        PlayerCustomizationManager clothingManager;

        enum State
        {
            None,
            Notice,
            Loading
        }

        Vector2 direction;
        int curPlayerIdx;
        int curTabIdx;
        bool showDescription = false;
        int curElementId = 0;
        State curState = State.None;

        List<AIGuildMember> players;
        InventaryTab curInventaryTab;
        IInventary inventary;
        Dictionary<int, (StackableNode panel, EquipedItem item, ISaveableItem realItem)> curElementLabels = new();

        enum InventaryTab
        {
            None,
            Modifications,
            Weapons,
            Inventary
        }

        private void Awake()
        {
            elementPanel.Initialize();
            WarningButtons[1].onClick.AddListener(CancelWarning);
        }

        private void Update()
        {
            Rotate();
        }

        private void Rotate()
        {
            characterModel.transform.Rotate(Vector3.up, direction.x * rotationVelocity * -1);
        }

        #region Elements panel
        public void ShowModifications()
        {
            ShowElements(GetSortedItems(InventaryTab.Modifications));
        }

        public void ShowWeapons()
        {
            ShowElements(GetSortedItems(InventaryTab.Weapons));
        }

        public void ShowInvantary()
        {
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
                    var item = curElementLabels[idx].item;
                    (inventary as InventaryEquipDecorator).Equip(players[curPlayerIdx], item.Type, item.GetSubType());

                    SetPlayersColors(curElementLabels[idx].item, curElementLabels[idx].panel);
                    break;

                default:
                    break;
            }
            print("Interacting");
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
                txtCount.text = equipedItem.Count.ToString();

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
                                  let id = player.stats.ID
                                  select
                                     (from character in equiped.Characters
                                      where character.stats.ID == id
                                      select character.stats.Color).First())
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
            WarningButtons[0].gameObject.SetActive(true);
            txtWarning.text = "El objeto está actualmente equipado.\n¿Desea equiparlo?";
        }

        private void ShowDeleteEquipedWarning()
        {
            curState = State.Notice;
            WarningButtons[0].gameObject.SetActive(true);
            txtWarning.text = "El objeto está actualmente equipado.\n¿Desea eliminarlo?";
        }

        private void ShowAmountNotice()
        {

        }

        private void CancelWarning()
        {
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
            curPlayerIdx = GetNextPlayerIdx(1);
            ShowCharacters();
        }

        public void ShowPreviourPlayer()
        {
            curPlayerIdx = GetNextPlayerIdx(-1);
            ShowCharacters();
        }
        #endregion

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
            //playersImg[1] = this.players[curPlayerIdx];
        }

        public void RotatePlayer(Vector2 direction)
        {
            this.direction = direction;
        }
    }
}

using Burmuruk.Tesis.Control;
using Burmuruk.Tesis.Stats;
using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Burmuruk.Tesis.UI
{
    public class UIMenuCharacters : MonoBehaviour
    {
        [SerializeField] StackableLabel elementPanel;
        [SerializeField] GameObject characterModel;
        [SerializeField] GameObject[] playersImg;
        [SerializeField] float rotationVelocity;
        PlayerCustomizationManager clothingManager;

        Vector2 direction;
        int curPlayerIdx;
        int curTabIdx;
        bool showDescription = false;

        List<AIGuildMember> players;
        InventaryTab curInventaryTab;
        IInventary inventary;

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

        public void ShowExtraData()
        {

        }

        public void Equip()
        {

        }

        private void ShowElements(List<ISaveableItem> items)
        {
            CleanElements();

            foreach (var item in items)
            {
                var panel = elementPanel.Get();
                panel.label.text = item.GetName();
            }
        }

        private void CleanElements()
        {
            while (elementPanel.activeNodes.Count > 0)
            {
                elementPanel.Release(elementPanel.activeNodes[0]);
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
        #endregion

        #region Characters panel
        public void ShowCharacters()
        {
            clothingManager.EquipModifications(players[curPlayerIdx]);
        }

        public void ShowNextPlayer()
        {

        }

        public void ShowPreviourPlayer()
        {

        } 
        #endregion

        public void SetInventary(IInventary inventary)
        {
            this.inventary = inventary;
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

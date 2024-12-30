using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public class BuffAdderUI
    {
        List<BuffsDataUI> buffs = new();
        VisualElement elementsContainer;
        List<string> buffTypes;

        public Foldout BuffsList { get; private set; }
        public UnsignedIntegerField BuffsCount { get; private set; }
        public Button BtnAddBuff { get; private set; }
        public Button BtnRemoveBuff { get; private set; }

        public BuffAdderUI(VisualElement container)
        {
            BuffsList = container.Q<Foldout>("mainFoldOut");
            BtnAddBuff = container.Q<Button>("btnAdd");
            BtnRemoveBuff = container.Q<Button>("btnRemove");
            BuffsCount = container.Q<UnsignedIntegerField>("uiAmount");

            SetupFoldOut(container);
            BuffsCount.RegisterCallback<KeyUpEvent>(OnValueChanged_BuffsCount);
        }

        private void OnValueChanged_BuffsCount(KeyUpEvent evt)
        {
            if (evt.keyCode != KeyCode.Return && evt.keyCode != KeyCode.KeypadEnter) return;

            int amount = ((int)BuffsCount.value) - buffs.Count;

            if (amount == 0)
            {
                buffs.ForEach(buff => { elementsContainer.Remove(buff.Element); });
                buffs.Clear();
            }
            else if (amount > 0)
            {
                while (amount > 0)
                {
                    AddBuff();
                    --amount;
                }
            }
            else
            {
                while (amount < 0)
                {
                    RemoveBuff();
                    ++amount;
                }
            }
        }

        public void SetBuffs(List<string> buffTypes) => this.buffTypes = buffTypes;

        private void SetupFoldOut(VisualElement container)
        {
            BuffsList.text = "Buffs";
            elementsContainer = new VisualElement();

            ScrollView scrollView = new ScrollView();
            scrollView.style.maxHeight = 180;
            scrollView.style.flexGrow = 1;
            scrollView.verticalScrollerVisibility = ScrollerVisibility.Auto;
            scrollView.Add(elementsContainer);
            BuffsList.Add(scrollView);

            var buttons = container.Q<VisualElement>("buttonsContainer");
            buttons.parent.Remove(buttons);
            BuffsList.Add(buttons);

            BtnAddBuff.clicked += () => AddBuff();
            BtnRemoveBuff.clicked += () => RemoveBuff();
        }

        private VisualElement AddBuff()
        {
            var buff = new BuffsDataUI();
            buff.SetValues(buffTypes);

            buffs.Add(buff);
            elementsContainer.Add(buff.Element);

            BuffsCount.SetValueWithoutNotify((uint)buffs.Count);
            return buff.Element;
        }

        private void RemoveBuff()
        {
            var buff = buffs[buffs.Count - 1];

            elementsContainer.Remove(buff.Element);
            buffs.RemoveAt(buffs.Count - 1);
            BuffsCount.SetValueWithoutNotify((uint)buffs.Count);
        }
    }
}

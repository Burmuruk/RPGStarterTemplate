using Burmuruk.Tesis.Stats;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor
{
    public class BuffAdderUI : IClearable
    {
        List<BuffsDataUI> buffs = new();
        VisualElement elementsContainer;
        List<string> buffTypes;

        public const string INVALIDNAME = "Custom";

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

        public void SetBuffs(List<string> buffTypes)
        {
            this.buffTypes = buffTypes;

            foreach (var buff in buffs)
            {
                buff.DDBuff.choices.Clear();
                buff.DDBuff.choices.Add("None");

                foreach (var type in buffTypes)
                {
                    buff.DDBuff.choices.Add(type);
                }
            }
        }

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
            if (buffs.Count == 0) return;

            var buff = buffs[buffs.Count - 1];

            elementsContainer.Remove(buff.Element);
            buffs.RemoveAt(buffs.Count - 1);
            BuffsCount.SetValueWithoutNotify((uint)buffs.Count);
        }

        public void UpdateData(List<(string name, BuffData? buff)> buffDatas)
        {
            int max = Mathf.Min(buffs.Count, buffDatas.Count);

            int i = 0;
            for (; i < max; i++)
            {
                if (!TryGetBuffName(buffDatas[i].name, out string curName))
                    continue;

                buffs[i].UpdateData(curName, buffDatas[i].buff);
            }

            if (buffDatas.Count > buffs.Count)
            {
                for (int j = i; j < buffDatas.Count; j++)
                {
                    if (!TryGetBuffName(buffDatas[j].name, out string curName)) 
                        continue;

                    AddBuff();
                    buffs[buffs.Count - 1].UpdateData(curName, buffDatas[j].buff);
                }
            }
            else if (buffDatas.Count < buffs.Count)
            {
                for (int j = i; j < buffs.Count; j++)
                {
                    RemoveBuff();
                }
            }
        }

        private bool TryGetBuffName(string name, out string newName)
        {
            newName = name switch
            {
                null => null,
                "" => INVALIDNAME,
                _ => name
            };

            return newName is not null;
        }

        public List<NamedBuff> GetBuffsData()
        {
            var buffsData = new List<NamedBuff>();

            foreach (var buff in buffs)
            {
                var data = buff.GetInfo();

                if (string.IsNullOrEmpty(data.Name))
                {
                    buffsData.Add(default);
                    continue;
                }

                buffsData.Add(data);
            }

            return buffsData;
        }

        public virtual void Clear()
        {
            elementsContainer.Clear();
            buffs.Clear();
            BuffsCount.value = 0;
            buffTypes?.Clear();
        }
    }
}

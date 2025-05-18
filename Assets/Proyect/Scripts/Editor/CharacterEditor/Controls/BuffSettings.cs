using Burmuruk.Tesis.Stats;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor.Controls
{
    public class BuffSettings : BaseInfoTracker, ISaveable
    {
        private NamedBuff _changesBuff;
        private string _id;

        public FloatField Value { get; private set; }
        public FloatField Duration { get; private set; }
        public FloatField Rate { get; private set; }
        public Toggle Percentage { get; private set; }
        public FloatField Probability { get; private set; }
        public EnumField Stat { get; private set; }

        public override void Initialize(VisualElement container, CreationsBaseInfo name)
        {
            _container = container;
            _nameControl = name;

            Value = container.Q<FloatField>("ffValue");
            Duration = container.Q<FloatField>("ffDuration");
            Rate = container.Q<FloatField>("ffRate");
            Percentage = container.Q<Toggle>("ffPercentage");
            Probability = container.Q<FloatField>("ffProbability");
            Stat = container.Q<EnumField>("ffStat");

            Stat.Init(ModifiableStat.None);
            _nameControl.TxtName.RegisterValueChangedCallback((evt) =>
            {
                if (IsActive)
                    TempName = evt.newValue;
            });
        }

        public BuffData GetInfo() =>
            new BuffData()
            {
                name = TxtName.value,
                value = Value.value,
                duration = Duration.value,
                rate = Rate.value,
                percentage = Percentage.value,
                probability = Probability.value,
                stat = (ModifiableStat)Stat.value
            };

        public void UpdateInfo(BuffData data)
        {
            TempName = data.name;
            TxtName.value = data.name;
            Value.value = data.value;
            Duration.value = data.duration;
            Rate.value = data.rate;
            Percentage.value = data.percentage;
            Probability.value = data.probability;
            Stat.value = data.stat;
            _changesBuff = new NamedBuff(data.name, data);
        }

        public override void UpdateName()
        {
            TxtName.value = TempName;
        }

        public override void Clear()
        {
            Value.value = 0;
            Duration.value = 0;
            Rate.value = 0;
            Percentage.value = false;
            Probability.value = 0;
            Stat.value = ModifiableStat.None;
            _changesBuff = new("", null);
            _id = null;
            base.Clear();
        }

        public override void Remove_Changes()
        {
            var data = _changesBuff.Data.Value;

            UpdateInfo(data);
        }

        public override ModificationTypes Check_Changes()
        {
            if (_changesBuff.Data == null) return CurModificationType = ModificationTypes.Add;

            CurModificationType = ModificationTypes.None;
            BuffData data = _changesBuff.Data.Value;

            if ((_nameControl.Check_Changes() & ModificationTypes.None) == 0)
                CurModificationType = ModificationTypes.Rename;

            if (data.stat != (ModifiableStat)Stat.value)
            {
                CurModificationType = ModificationTypes.EditData;
            }
            if (data.value != Value.value)
            {
                CurModificationType = ModificationTypes.EditData;

            }
            if (data.duration != Duration.value)
            {
                CurModificationType = ModificationTypes.EditData;

            }
            if (data.rate != Rate.value)
            {
                CurModificationType = ModificationTypes.EditData;

            }
            if (data.percentage != Percentage.value)
            {
                CurModificationType = ModificationTypes.EditData;

            }
            if (data.probability != Probability.value)
            {
                CurModificationType = ModificationTypes.EditData;
            }

            //if ((CurModificationType & ModificationTypes.EditData & ModificationTypes.Rename) != 0)
            //{
            //    if (string.IsNullOrEmpty(_changesBuff.Name) && data == default)
            //        return ModificationTypes.Add; 
            //}

            return CurModificationType;
        }

        public string Save()
        {
            var result = Check_Changes();
            if ((result & ModificationTypes.None) != 0)
                return "No changes found";

            var data = new CreationData(_nameControl.TxtName.value, GetInfo());

            return SavingSystem.SaveCreation(ElementType.Buff, _id, data, CurModificationType);
        }

        public CreationData Load(ElementType type, string id)
        {
            var data = SavingSystem.Load(type, id);

            if (data == null)
            {
                return default;
            }
            else
            {
                BuffData newData = (BuffData)data.Value.data;
                _id = id;
                UpdateInfo(newData);
                Set_CreationState(CreationsState.Editing);
                return data.Value;
            }
        }
    }

    public record BuffDataArgs : ItemDataArgs
    {
        public readonly BuffData Buff;

        public BuffDataArgs(BuffData buff)
        {
            Buff = buff;
        }
    }
}

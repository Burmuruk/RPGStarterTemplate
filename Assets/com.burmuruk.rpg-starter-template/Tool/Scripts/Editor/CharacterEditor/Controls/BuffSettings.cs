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
            _originalName = data.name;
            UpdateName();
            Value.value = data.value;
            Duration.value = data.duration;
            Rate.value = data.rate;
            Percentage.value = data.percentage;
            Probability.value = data.probability;
            Stat.value = data.stat;
            _changesBuff = new NamedBuff(data.name, data);
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

        public override void Load_Changes()
        {
            var data = _changesBuff.Data.Value;

            UpdateInfo(data);
        }

        public override ModificationTypes Check_Changes()
        {
            try
            {
                if (_changesBuff.Data == null) return CurModificationType = ModificationTypes.Add;

                CurModificationType = ModificationTypes.None;
                BuffData data = _changesBuff.Data.Value;

                if (_nameControl.Check_Changes() != ModificationTypes.None)
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
            catch (InvalidDataExeption e)
            {
                throw e;
            }
        }

        public override bool VerifyData()
        {
            return _nameControl.VerifyData();
        }

        public bool Save()
        {
            if (!VerifyData())
            {
                Utilities.UtilitiesUI.Notify("Invalid Data", BorderColour.Error);
                return false;
            }

            try
            {
                if (_creationsState == CreationsState.Editing && Check_Changes() == ModificationTypes.None)
                {
                    Utilities.UtilitiesUI.Notify("No changes were found", BorderColour.HighlightBorder);
                    return false;
                }
                else
                    CurModificationType = ModificationTypes.Add;

                Utilities.UtilitiesUI.DisableNotification();
                var data = new CreationData(_nameControl.TxtName.value.Trim(), GetInfo());

                return SavingSystem.SaveCreation(ElementType.Buff, _id, data, CurModificationType);
            }
            catch (InvalidDataExeption e)
            {
                throw e;
            }
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
                Set_CreationState(CreationsState.Editing);
                UpdateInfo(newData);
                return data.Value;
            }
        }

        public override void Remove_Changes()
        {
            _changesBuff = new("", null);
            _id = null;
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

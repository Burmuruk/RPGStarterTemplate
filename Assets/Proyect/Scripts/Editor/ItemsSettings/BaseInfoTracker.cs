using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor.Controls
{
    public abstract class BaseInfoTracker : SubWindow
    {
        protected CreationsBaseInfo _nameControl;
        protected CreationsState _creationsState = CreationsState.Creating;

        public string TempName { get; protected set; } = "";
        public TextField TxtName => _nameControl.TxtName;
        public CreationsState CreationsState => _creationsState;

        public virtual void Initialize(VisualElement container, CreationsBaseInfo name)
        {
            _nameControl = name;
            _container = container;
            _nameControl.TxtName.RegisterValueChangedCallback((evt) =>
            {
                if (IsActive)
                    TempName = evt.newValue;
            });
            _nameControl.CreationsStateChanged += Set_CreationState;
        }

        public override void Clear()
        {
            TempName = "";
            TxtName.value = "";
        }

        public virtual void UpdateName()
        {
            TxtName.value = TempName;
            _nameControl.BtnState.text = _creationsState.ToString();
        }

        public virtual void Set_CreationState(CreationsState state)
        {
            if (state == CreationsState.Creating)
                Clear();

            _nameControl.SetState(state);
        }
    }
}

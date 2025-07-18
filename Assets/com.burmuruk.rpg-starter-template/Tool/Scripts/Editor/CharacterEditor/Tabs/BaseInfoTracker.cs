﻿using UnityEngine.UIElements;

namespace Burmuruk.RPGStarterTemplate.Editor.Controls
{
    public abstract class BaseInfoTracker : SubWindow
    {
        protected CreationsBaseInfo _nameControl;
        protected string _originalName = "";
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
            _originalName = "";
            _nameControl.UpdateName(TempName, _originalName);
        }

        public override void Enable(bool enabled)
        {
            base.Enable(enabled);

            if (enabled)
            {
                _nameControl.SetState(_creationsState);
                UpdateName();
            }
        }

        public virtual void UpdateName()
        {
            _nameControl.UpdateName(TempName, _originalName);
            _nameControl.SetState(_creationsState);
        }

        public virtual void Set_CreationState(CreationsState state)
        {
            if (state == CreationsState.Creating)
                Remove_Changes();

            _creationsState = state;
        }
    }
}

using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor.Controls
{
    public abstract class SubWindow : UnityEditor.Editor, IClearable, IChangesObserver, INameTracker
    {
        protected NameSettings _nameControl;
        protected VisualElement _container;
        protected VisualElement _instance;
        protected ModificationType _modificationType;

        public VisualElement Container { get => _container; }
        public VisualElement Instance { get => _instance; }
        public TextField TxtName => _nameControl.TxtName;
        protected ModificationType CurModificationType
        {
            get => _modificationType;
            set
            {
                if (value == ModificationType.None)
                {
                    _modificationType = value;
                    return;
                }
                else if (value == ModificationType.Rename)
                {
                    _modificationType = ModificationType.Rename;
                    return;
                }
                else if ((_modificationType | ModificationType.Rename) != 0)
                {
                    _modificationType |= value;
                    return;
                }

                _modificationType = value;
            }
        }

        public Action GoBack;

        public virtual void Initialize(VisualElement container, NameSettings name)
        {
            _nameControl = name;
            _container = container;
        }

        public abstract ModificationType Check_Changes();

        public abstract void Clear();

        public abstract void Remove_Changes();
    }

    public interface INameTracker
    {
        public TextField TxtName { get; }

        public void Initialize(VisualElement container, NameSettings name);
    }
}

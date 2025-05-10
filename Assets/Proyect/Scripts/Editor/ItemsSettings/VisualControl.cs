using Burmuruk.Tesis.Inventory;
using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Burmuruk.Tesis.Editor.Controls
{
    public abstract class VisualControl : UnityEditor.Editor, IClearable
    {
        public abstract void Clear();
    }
}

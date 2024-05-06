using System;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Burmuruk.Tesis.UI
{
    public class MyItemButton : Button
    {
        int id;
        Action callback;
        public event Action<int> OnPointerEnterEvent;

        public void SetId(int id)
        {
            this.id = id;
        }

        public void SetCallback(Action callback)
        {
            this.callback = callback;
        }

        public override void OnPointerClick(PointerEventData eventData)
        {
            base.OnPointerClick(eventData);

            callback?.Invoke();
            callback = null;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData);

            OnPointerEnterEvent?.Invoke(id);
        }
    }
}
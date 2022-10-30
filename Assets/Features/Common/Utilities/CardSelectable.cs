using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Common.Utilities
{
    public class CardSelectable : MonoBehaviour, IPointerDownHandler
    {
        private bool selected = false;
        private Action<CardSelectable> _selectAction;
        private Action<CardSelectable> _unSelectAction;

        public CardSelectable WithSelectAction(Action<CardSelectable> action) {
            _selectAction = action;
            return this;
        }
        public CardSelectable WithUnselectAction(Action<CardSelectable> action)
        {
            _unSelectAction = action;
            return this;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            selected = !selected;
            if (selected && _selectAction != null)
                _selectAction(this);
            if (!selected && _unSelectAction != null)
                _unSelectAction(this);
        }
    }
}

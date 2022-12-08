using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Features.Common.Utilities
{
    public class Draggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {

        private RectTransform _dropAreaPlay;
        private Action<Draggable> _onPlayCallback;
        private bool dragging = false;
        private Action<bool> _onDrag;

        public Draggable WithDropArea(RectTransform dropAreaPlay)
        {
            _dropAreaPlay = dropAreaPlay;
            return this;
        }

        public Draggable WithCallback(Action<Draggable> onPlayCallback)
        {
            _onPlayCallback = onPlayCallback;
            return this;
        }

        public Draggable WithDragAction(Action<bool> onDrag)
        {
            _onDrag = onDrag;
            return this;
        }
        // Update is called once per frame
        void Update()
        {
            if (!dragging)
                return;
            var screenPoint = Input.mousePosition;
            screenPoint.z = 10.0f; //distance of the plane from the camera
            transform.position = Camera.main.ScreenToWorldPoint(screenPoint);
            transform.rotation = Quaternion.identity;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            dragging = false;
            if (_onPlayCallback == null)
                return;

            if (ViewsHelper.IsOverlapped(_dropAreaPlay, this.transform.position))
                _onPlayCallback(this);

            if (_onDrag != null)
                _onDrag(false);
            ViewsHelper.RefreshView(GetComponent<RectTransform>());
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            dragging = true;
            if (_onDrag != null)
                _onDrag(true);
        }

    
    }
}

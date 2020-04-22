using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    private RectTransform _dropAreaPlay;
    private Action<Draggable> _onPlayCallback;
    private bool dragging = false;

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

    // Update is called once per frame
    void Update()
    {
        if (!dragging)
            return;
        var screenPoint = Input.mousePosition;
        screenPoint.z = 10.0f; //distance of the plane from the camera
        transform.position = Camera.main.ScreenToWorldPoint(screenPoint);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
        if (_onPlayCallback == null)
            return;

        if (ViewsHelper.IsOverlapped(_dropAreaPlay, this.transform.position))
            _onPlayCallback(this);

        ViewsHelper.RefreshView(GetComponent<RectTransform>());
    }


    public void OnPointerDown(PointerEventData eventData)
    {
        dragging = true;
    }
}

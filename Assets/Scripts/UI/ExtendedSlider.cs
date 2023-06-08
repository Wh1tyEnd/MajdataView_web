using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable] 
public class ExtendedEvent : UnityEvent<float> { } //定义拓展的事件

public class ExtendedSlider : Slider, IBeginDragHandler, IEndDragHandler
{
    public ExtendedEvent DragStart;

    public ExtendedEvent DragStop;

    public ExtendedEvent PointerDown;

    public void OnBeginDrag(PointerEventData eventData)
    {
        DragStart.Invoke(m_Value);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DragStop.Invoke(m_Value);
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        PointerDown.Invoke(m_Value);
    }

}

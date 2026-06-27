using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public static Vector2 Direction { get; private set; }
    public float radius = 60f;

    RectTransform bgRect;
    RectTransform handleRect;
    Canvas canvas;
    Camera cam;

    void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        bgRect = GetComponent<RectTransform>();
        handleRect = transform.GetChild(0).GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            bgRect, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            Vector2 clamped = Vector2.ClampMagnitude(localPoint, radius);
            handleRect.anchoredPosition = clamped;
            Direction = clamped / radius;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        handleRect.anchoredPosition = Vector2.zero;
        Direction = Vector2.zero;
    }
}

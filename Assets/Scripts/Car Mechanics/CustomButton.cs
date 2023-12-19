using UnityEngine;
using UnityEngine.EventSystems;

public class CustomButton : MonoBehaviour
{
    [Header("Button Settings")]
    public bool IsPressed;
    public float SmoothInput = 0;
    public float Sensitivity = 2f;

    private void Start()
    {
        SetUpButton();
    }

    private void Update()
    {
        UpdateDampenPress();
    }

    private void SetUpButton()
    {
        EventTrigger trigger = gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((e) => OnClickDown());

        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((e) => OnClickUp());

        trigger.triggers.Add(pointerDown);
        trigger.triggers.Add(pointerUp);
    }

    public void OnClickDown()
    {
        IsPressed = true;
    }

    public void OnClickUp()
    {
        IsPressed = false;
    }

    private void UpdateDampenPress()
    {
        if (IsPressed)
        {
            SmoothInput += Sensitivity * Time.deltaTime;
        }
        else
        {
            SmoothInput -= Sensitivity * Time.deltaTime;
        }
        SmoothInput = Mathf.Clamp01(SmoothInput);
    }
}

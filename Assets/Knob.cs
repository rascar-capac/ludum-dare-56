using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Knob : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Transform Indicator;
    public Image Fill;
    public TMP_Text Value;
    public int MaxOffsetInPixels;

    public float InitialMouseX;
    public bool IsTurning;
    public float InitialValue;
    public float CurrentValue;

    public UnityEvent OnValueChanged { get; } = new();

    public void OnPointerDown(PointerEventData eventData)
    {
        InitialMouseX = Input.mousePosition.x;
        IsTurning = true;
        InitialValue = CurrentValue;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        IsTurning = false;
    }

    public void SetValue(float value)
    {
        if(CurrentValue == value)
        {
            return;
        }

        Indicator.rotation = Quaternion.Euler(Indicator.rotation.x, Indicator.rotation.y, value * -360f);
        Fill.fillAmount = value;
        Value.text = value.ToString( "N1" );
        CurrentValue = value;
        OnValueChanged.Invoke();
    }

    private void Awake()
    {
        Indicator.rotation = Quaternion.Euler(Indicator.rotation.x, Indicator.rotation.y, 0f);
        Fill.fillAmount = 0f;
        Value.text = 0f.ToString( "N1" );
    }

    private void Update()
    {
        if(!IsTurning)
        {
            return;
        }

        float offsetX = Input.mousePosition.x - InitialMouseX;
        float value = InitialValue + offsetX / MaxOffsetInPixels;
        value = Mathf.Clamp01(value);
        SetValue(value);
    }
}

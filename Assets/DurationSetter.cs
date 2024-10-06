using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DurationSetter : MonoBehaviour
{
    public TMP_Text PreviousValueText;
    public TMP_Text SelectedValueText;
    public TMP_Text NextValueText;
    public Button PreviousButton;
    public Button NextButton;

    public int SelectedValue;

    public UnityEvent OnValueChanged { get; } = new();

    public void RefreshValues(int selectedValue)
    {
        IntegerRange minMaxTickCount = ParametersManager.Instance.MinMaxTickCount;
        SelectedValue = minMaxTickCount.Clamp(selectedValue);
        SelectedValueText.text = SelectedValue.ToString();

        int previousValue = SelectedValue - 1;

        if(previousValue < minMaxTickCount.MinimumValue)
        {
            PreviousValueText.enabled = false;
            PreviousButton.interactable = false;
        }
        else
        {
            PreviousValueText.enabled = true;
            PreviousButton.interactable = true;
            PreviousValueText.text = previousValue.ToString();
        }

        int nextValue = SelectedValue + 1;

        if(nextValue > minMaxTickCount.MaximumValue)
        {
            NextValueText.enabled = false;
            NextButton.interactable = false;
        }
        else
        {
            NextValueText.enabled = true;
            NextButton.interactable = true;
            NextValueText.text = nextValue.ToString();
        }
    }

    private void PreviousButton_OnClick()
    {
        RefreshValues(SelectedValue - 1);
        OnValueChanged.Invoke();
    }

    private void NextButton_OnClick()
    {
        RefreshValues(SelectedValue + 1);
        OnValueChanged.Invoke();
    }

    private void Awake()
    {
        RefreshValues(1);

        PreviousButton.onClick.AddListener(PreviousButton_OnClick);
        NextButton.onClick.AddListener(NextButton_OnClick);
    }

    private void OnDestroy()
    {
        PreviousButton.onClick.RemoveListener(PreviousButton_OnClick);
        NextButton.onClick.RemoveListener(NextButton_OnClick);
    }
}

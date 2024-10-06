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
        SelectedValue = selectedValue;
        SelectedValueText.text = SelectedValue.ToString();
        PreviousValueText.text = (SelectedValue - 1).ToString();
        NextValueText.text = (SelectedValue + 1).ToString();
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
        RefreshValues(0);

        PreviousButton.onClick.AddListener(PreviousButton_OnClick);
        NextButton.onClick.AddListener(NextButton_OnClick);
    }

    private void OnDestroy()
    {
        PreviousButton.onClick.RemoveListener(PreviousButton_OnClick);
        NextButton.onClick.RemoveListener(NextButton_OnClick);
    }
}

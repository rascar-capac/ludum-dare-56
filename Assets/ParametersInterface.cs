using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ParametersInterface : MonoBehaviour
{
    public SerializableDictionary<ParameterData, Knob> Knobs;
    public Button PreviewButton;
    public Button CommitButton;
    public Image[] AttemptLights;
    public Sprite AttemptOffSprite;
    public Sprite AttemptOnSprite;
    public DurationSetter DurationSetter;
    public Button ClosePreviewButton;
    public GameObject NoMoreAttemptsText;

    public IReadOnlyDictionary<ParameterData, float> CurrentParameters => Knobs.ToDictionary(knob => knob.Key, knob => knob.Value.CurrentValue);

    public void RefreshKnobs()
    {
        foreach((ParameterData type, Knob knob) in Knobs)
        {
            knob.SetValue(ParametersManager.Instance.Parameters[type]);
        }
    }

    public void RefreshAttemptLights()
    {
        for(int attemptIndex = 0; attemptIndex < AttemptLights.Length; attemptIndex++)
        {
            Image attemptLight = AttemptLights[attemptIndex];
            attemptLight.sprite = attemptIndex < ParametersManager.Instance.CurrentAttemptsCount ? AttemptOnSprite : AttemptOffSprite;
        }

        PreviewButton.interactable = ParametersManager.Instance.CurrentAttemptsCount < AttemptLights.Length;
    }

    public void Preview()
    {
        ParametersManager.Instance.Preview(CurrentParameters, DurationSetter.SelectedValue);
    }

    public void ClosePreview()
    {
        ParametersManager.Instance.TryClosePreview();
    }

    public void Commit()
    {
        ParametersManager.Instance.Commit();
    }

    private void Knob_OnValueChanged()
    {
        PreviewButton.interactable = ParametersManager.Instance.CurrentAttemptsCount < AttemptLights.Length;
    }

    private void DurationSetter_OnValueChanged()
    {
        PreviewButton.interactable = ParametersManager.Instance.CurrentAttemptsCount < AttemptLights.Length;
    }

    private void ParametersManager_OnParametersChanged(IReadOnlyDictionary<ParameterData, float> parameters, int tickCount)
    {
        RefreshKnobs();
    }

    private void ParametersManager_OnPreviewed(IReadOnlyDictionary<ParameterData, float> parameters, int tickCount)
    {
        RefreshAttemptLights();
        PreviewButton.interactable = false;
        CommitButton.gameObject.SetActive(true);
        ClosePreviewButton.gameObject.SetActive(ParametersManager.Instance.CurrentAttemptsCount < AttemptLights.Length);
        NoMoreAttemptsText.SetActive(ParametersManager.Instance.CurrentAttemptsCount == AttemptLights.Length);
    }

    private void ParametersManager_OnPreviewClosed()
    {
        RefreshAttemptLights();
        CommitButton.gameObject.SetActive(false);
        ClosePreviewButton.gameObject.SetActive(false);
        NoMoreAttemptsText.SetActive(false);
    }

    private void ParametersManager_OnCommited(IReadOnlyDictionary<ParameterData, float> parameters, int tickCount)
    {
        RefreshAttemptLights();
        CommitButton.gameObject.SetActive(false);
        ClosePreviewButton.gameObject.SetActive(false);
        NoMoreAttemptsText.SetActive(false);
    }

    private void PreviewButton_OnClick()
    {
        Preview();
    }

    private void CommitButton_OnClick()
    {
        Commit();
    }

    private void ClosePreviewButton_OnClick()
    {
        ClosePreview();
    }

    private void Awake()
    {
        foreach(Image attemptLight in AttemptLights)
        {
            attemptLight.sprite = AttemptOffSprite;
        }

        foreach(Knob knob in Knobs.Values)
        {
            knob.OnValueChanged.AddListener(Knob_OnValueChanged);
        }

        RefreshKnobs();

        CommitButton.gameObject.SetActive(false);
        ClosePreviewButton.gameObject.SetActive(false);
        NoMoreAttemptsText.SetActive(false);

        PreviewButton.onClick.AddListener(PreviewButton_OnClick);
        CommitButton.onClick.AddListener(CommitButton_OnClick);
        ClosePreviewButton.onClick.AddListener(ClosePreviewButton_OnClick);

        DurationSetter.OnValueChanged.AddListener(DurationSetter_OnValueChanged);

        ParametersManager.Instance.OnParametersChanged.AddListener(ParametersManager_OnParametersChanged);
        ParametersManager.Instance.OnPreviewed.AddListener(ParametersManager_OnPreviewed);
        ParametersManager.Instance.OnPreviewClosed.AddListener(ParametersManager_OnPreviewClosed);
        ParametersManager.Instance.OnCommited.AddListener(ParametersManager_OnCommited);
    }

    private void OnDestroy()
    {
        foreach(Knob knob in Knobs.Values)
        {
            knob.OnValueChanged.RemoveListener(Knob_OnValueChanged);
        }

        PreviewButton.onClick.RemoveListener(PreviewButton_OnClick);
        CommitButton.onClick.RemoveListener(CommitButton_OnClick);
        ClosePreviewButton.onClick.RemoveListener(ClosePreviewButton_OnClick);

        DurationSetter.OnValueChanged.RemoveListener(DurationSetter_OnValueChanged);

        if(ParametersManager.HasInstance)
        {
            ParametersManager.Instance.OnParametersChanged.RemoveListener(ParametersManager_OnParametersChanged);
            ParametersManager.Instance.OnPreviewed.RemoveListener(ParametersManager_OnPreviewed);
            ParametersManager.Instance.OnPreviewClosed.RemoveListener(ParametersManager_OnPreviewClosed);
            ParametersManager.Instance.OnCommited.RemoveListener(ParametersManager_OnCommited);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;
using UnityEngine.Rendering;
using DG.Tweening;

public class ParametersManager : Singleton<ParametersManager>
{
    public SerializableDictionary<ParameterData, float> Parameters;
    public IntegerRange MinMaxTickCount;
    public Volume Volume;
    public VolumeProfile DefaultProfile;
    public VolumeProfile PreviewProfile;

    public int CurrentAttemptsCount;
    public bool IsInPreview;
    public IReadOnlyDictionary<ParameterData, float> PreviewParameters;
    public int PreviewTickCount;
    public int TotalTicksCounter;
    public int TotalTicksCounterBeforePreview;
    public Tween VolumeTween;

    public UnityEvent<IReadOnlyDictionary<ParameterData, float>, int> OnParametersChanged { get; } = new();
    public UnityEvent<IReadOnlyDictionary<ParameterData, float>, int> OnPreviewed { get; } = new();
    public UnityEvent<bool> OnPreviewClosed { get; } = new();
    public UnityEvent<IReadOnlyDictionary<ParameterData, float>, int> OnCommited { get; } = new();

    public void SetParameters(IReadOnlyDictionary<ParameterData, float> parameters, int tickCount)
    {
        List<ParameterData> parameterTypes = Parameters.Keys.ToList();

        foreach(ParameterData type in parameterTypes)
        {
            Parameters[type] = parameters[type];
        }

        OnParametersChanged.Invoke(Parameters, tickCount);
    }

    public void Preview(IReadOnlyDictionary<ParameterData, float> parameters, int tickCount)
    {
        if(CurrentAttemptsCount >= 3)
        {
            return;
        }

        VolumeTween.Kill();

        if(IsInPreview)
        {
            Volume.profile = PreviewProfile;
            Volume.weight = 1f;
        }
        else
        {
            Volume.profile = PreviewProfile;
            Volume.weight = 0.2f;
            VolumeTween = DOTween.To(() => Volume.weight, x => Volume.weight = x, 1f, 0.5f).SetEase(Ease.OutQuint);
        }

        TryClosePreview(isCommitting: false, applyVolumeEffect: false);
        CurrentAttemptsCount++;
        IsInPreview = true;
        PreviewParameters = parameters;
        PreviewTickCount = tickCount;

        TotalTicksCounterBeforePreview = TotalTicksCounter;
        TotalTicksCounter += tickCount;
        BogbogsManager.Instance.SaveCurrentState();
        OnPreviewed.Invoke(PreviewParameters, PreviewTickCount);
    }

    public void TryClosePreview(bool isCommitting, bool applyVolumeEffect = true)
    {
        if(!IsInPreview)
        {
            return;
        }

        IsInPreview = false;
        PreviewParameters = null;
        PreviewTickCount = 0;

        TotalTicksCounter = TotalTicksCounterBeforePreview;
        BogbogsManager.Instance.RestorePreviousState();

        if(applyVolumeEffect)
        {
            VolumeTween.Kill();
            VolumeTween = DOTween.To(() => Volume.weight, x => Volume.weight = x, 0.2f, 0.5f)
                .SetEase(Ease.InQuint)
                .OnComplete(() =>
                {
                    Volume.profile = DefaultProfile;
                    Volume.weight = 1f;
                });
        }

        OnPreviewClosed.Invoke(isCommitting);
    }

    public void Commit()
    {
        if(!IsInPreview)
        {
            return;
        }

        IReadOnlyDictionary<ParameterData, float> previewParameters = PreviewParameters;
        int previewTickCount = PreviewTickCount;
        TryClosePreview(isCommitting: true);
        SetParameters(previewParameters, previewTickCount);
        CurrentAttemptsCount = 0;
        TotalTicksCounter += previewTickCount;

        //handle fade in/out
        //potential defeat/win
        OnCommited.Invoke(previewParameters, previewTickCount);
    }

    public override void Awake()
    {
        base.Awake();

        TotalTicksCounter = 1;
        Volume.profile = DefaultProfile;
    }
}

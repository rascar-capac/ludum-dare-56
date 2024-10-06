using System.Collections.Generic;
using System.Linq;
using UnityEngine.Events;

public class ParametersManager : Singleton<ParametersManager>
{
    public SerializableDictionary<ParameterData, float> Parameters;
    public IntegerRange MinMaxTickCount;

    public int CurrentAttemptsCount;
    public bool IsInPreview;
    public IReadOnlyDictionary<ParameterData, float> PreviewParameters;
    public int TotalTicksCounter;
    public int SavedTotalTicksCounter;

    public UnityEvent<IReadOnlyDictionary<ParameterData, float>, int> OnParametersChanged { get; } = new();
    public UnityEvent<IReadOnlyDictionary<ParameterData, float>, int> OnPreviewed { get; } = new();
    public UnityEvent OnPreviewLeft { get; } = new();
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

        TryQuitPreview();
        CurrentAttemptsCount++;
        IsInPreview = true;
        PreviewParameters = parameters;

        SavedTotalTicksCounter = TotalTicksCounter;
        TotalTicksCounter += tickCount;
        BogbogsManager.Instance.SaveCurrentState();
        OnPreviewed.Invoke(PreviewParameters, tickCount);
        //post process
    }

    public void TryQuitPreview()
    {
        if(!IsInPreview)
        {
            return;
        }

        IsInPreview = false;
        PreviewParameters = null;

        TotalTicksCounter = SavedTotalTicksCounter;
        BogbogsManager.Instance.RestorePreviousState();
        OnPreviewLeft.Invoke();
        //post process
    }

    public void Commit(int tickCount)
    {
        if(!IsInPreview)
        {
            return;
        }

        IReadOnlyDictionary<ParameterData, float> previewParameters = PreviewParameters;
        TryQuitPreview();
        SetParameters(previewParameters, tickCount);
        CurrentAttemptsCount = 0;
        TotalTicksCounter += tickCount;

        //handle fade in/out
        //potential defeat/win
        //stop post process
        OnCommited.Invoke(previewParameters, tickCount);
    }

    public override void Awake()
    {
        base.Awake();

        TotalTicksCounter = 1;
    }
}
